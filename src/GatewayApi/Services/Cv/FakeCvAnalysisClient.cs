using PearlMetric.GatewayApi.Contracts.CvWorker;

namespace PearlMetric.GatewayApi.Services.Cv;

/// <summary>
/// In-process stand-in for the Python CV worker. Returns deterministic calibration
/// and Lab/DeltaE samples so Gateway persistence can be proven without Python.
/// </summary>
public sealed class FakeCvAnalysisClient : ICvAnalysisClient
{
    public const string AlgorithmVersion = "fake-cv-1.0.0";

    public Task<AnalyzeScanResponse> AnalyzeAsync(
        AnalyzeScanRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!CvWorkerProtocol.IsSupportedProtocolVersion(request.ProtocolVersion))
        {
            return Task.FromResult(Failed(
                request,
                CvWorkerErrorCodes.UnsupportedProtocolVersion,
                $"Unsupported protocol version '{request.ProtocolVersion}'."));
        }

        if (!CvWorkerProtocol.IsValidFrameCount(request.Frames.Count))
        {
            return Task.FromResult(Failed(
                request,
                CvWorkerErrorCodes.FrameLimitExceeded,
                "Frame count is outside the allowed protocol limits."));
        }

        foreach (var frame in request.Frames)
        {
            if (!CvWorkerProtocol.IsValidStorageKey(frame.StorageKey))
            {
                return Task.FromResult(Failed(
                    request,
                    CvWorkerErrorCodes.InvalidStorageKey,
                    $"Invalid storage key '{frame.StorageKey}'."));
            }
        }

        var samples = request.Frames
            .OrderBy(frame => frame.SequenceIndex)
            .Select(frame =>
            {
                // Deterministic "measured" color from sequence index.
                var l = 78.0 - (frame.SequenceIndex * 0.4);
                var a = 2.5 + (frame.SequenceIndex * 0.1);
                var b = 14.0 - (frame.SequenceIndex * 0.2);
                var deltaE = request.Baseline is null
                    ? 0.0
                    : Math.Round(
                        Math.Sqrt(
                            Math.Pow(l - request.Baseline.L, 2)
                            + Math.Pow(a - request.Baseline.A, 2)
                            + Math.Pow(b - request.Baseline.B, 2)),
                        4);

                return new ColorMetricResultDto(
                    frame.SequenceIndex,
                    frame.CapturedAtUtc,
                    Math.Round(l, 4),
                    Math.Round(a, 4),
                    Math.Round(b, 4),
                    deltaE,
                    ShadeGuideValue: $"A{Math.Clamp(2 - frame.SequenceIndex, 1, 3)}",
                    ConfidenceScore: 0.92);
            })
            .ToArray();

        var calibration = new CalibrationResultDto(
            AlignmentMatrix:
            [
                [1.0, 0.0, 0.0],
                [0.0, 1.0, 0.0],
                [0.0, 0.0, 1.0]
            ],
            CalibrationReferenceValue: 0.98,
            CalibratedAtUtc: DateTime.UtcNow);

        return Task.FromResult(new AnalyzeScanResponse(
            CvWorkerProtocol.Version,
            request.ScanRunId,
            request.CorrelationId,
            request.IdempotencyKey,
            CvAnalysisStatus.Success,
            AlgorithmVersion,
            request.DeltaEFormula,
            calibration,
            samples,
            Error: null));
    }

    private static AnalyzeScanResponse Failed(
        AnalyzeScanRequest request,
        string code,
        string message) =>
        new(
            CvWorkerProtocol.Version,
            request.ScanRunId,
            request.CorrelationId,
            request.IdempotencyKey,
            CvAnalysisStatus.Failed,
            AlgorithmVersion,
            request.DeltaEFormula,
            Calibration: null,
            Samples: [],
            Error: new CvWorkerErrorDto(code, message));
}
