using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PearlMetric.GatewayApi.Contracts.Api.Runs;
using PearlMetric.GatewayApi.Contracts.CvWorker;
using PearlMetric.GatewayApi.Data;
using PearlMetric.GatewayApi.Domain;
using PearlMetric.GatewayApi.Models;
using PearlMetric.GatewayApi.Services.Cv;

namespace PearlMetric.GatewayApi.Services;

public sealed class ScanAnalysisService(
    PearlMetricDb db,
    ICvAnalysisClient cvAnalysisClient)
{
    private static readonly JsonSerializerOptions MatrixJsonOptions = new()
    {
        WriteIndented = false
    };

    public async Task<ScanRunResponse?> AnalyzeAsync(Guid runId, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        // Load frames only. Avoid including optional 1:1 Calibration in the same graph —
        // EF can mis-track that dependent and emit UPDATE instead of INSERT.
        var run = await db.Runs
            .Include(item => item.Frames)
            .FirstOrDefaultAsync(item => item.Id == runId, cancellationToken);

        if (run is null)
        {
            return null;
        }

        var frameCount = run.Frames.Count;

        // Idempotent success path.
        if (run.Status == ScanRunStatus.Completed)
        {
            var sampleCount = await db.ColorMetricSamples
                .CountAsync(item => item.ScanRunId == run.Id, cancellationToken);
            return ToResponse(run, frameCount, sampleCount);
        }

        if (frameCount == 0)
        {
            throw new InvalidOperationException("Cannot analyze a run with no uploaded frames.");
        }

        if (run.Status == ScanRunStatus.Failed)
        {
            ScanRunStatusTransitions.EnsureCanTransition(run.Status, ScanRunStatus.Pending);
            run.Status = ScanRunStatus.Pending;
            run.FailureReason = null;
        }

        ScanRunStatusTransitions.EnsureCanTransition(run.Status, ScanRunStatus.Processing);
        run.Status = ScanRunStatus.Processing;
        await db.SaveChangesAsync(cancellationToken);

        var baseline = await ResolveBaselineAsync(run, cancellationToken);
        var correlationId = Guid.NewGuid().ToString("N");
        var idempotencyKey = $"analyze:{run.Id}";

        var request = new AnalyzeScanRequest(
            CvWorkerProtocol.Version,
            run.Id,
            correlationId,
            idempotencyKey,
            run.Frames
                .OrderBy(frame => frame.SequenceIndex)
                .Select(frame => new FrameReferenceDto(
                    frame.SequenceIndex,
                    frame.StorageKey,
                    frame.CapturedAtUtc))
                .ToArray(),
            baseline,
            DeltaEFormula.CIEDE2000);

        AnalyzeScanResponse response;
        try
        {
            response = await cvAnalysisClient.AnalyzeAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            ScanRunStatusTransitions.EnsureCanTransition(run.Status, ScanRunStatus.Failed);
            run.Status = ScanRunStatus.Failed;
            run.FailureReason = TrimFailure($"CV analysis threw: {ex.Message}");
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return ToResponse(run, frameCount, sampleCount: 0);
        }

        if (response.Status != CvAnalysisStatus.Success
            || response.Calibration is null
            || response.Samples.Count == 0)
        {
            ScanRunStatusTransitions.EnsureCanTransition(run.Status, ScanRunStatus.Failed);
            run.Status = ScanRunStatus.Failed;
            run.FailureReason = TrimFailure(
                response.Error?.Message ?? "CV analysis failed without a structured error.");
            run.AlgorithmVersion = response.AlgorithmVersion;
            run.DeltaEFormula = response.DeltaEFormula.ToString();
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return ToResponse(run, frameCount, sampleCount: 0);
        }

        // Replace any prior partial results for safe retries.
        var priorCalibration = await db.CalibrationProfiles
            .FirstOrDefaultAsync(item => item.ScanRunId == run.Id, cancellationToken);
        if (priorCalibration is not null)
        {
            db.CalibrationProfiles.Remove(priorCalibration);
        }

        var priorSamples = await db.ColorMetricSamples
            .Where(item => item.ScanRunId == run.Id)
            .ToListAsync(cancellationToken);
        if (priorSamples.Count > 0)
        {
            db.ColorMetricSamples.RemoveRange(priorSamples);
        }

        db.CalibrationProfiles.Add(new CalibrationProfile
        {
            Id = Guid.NewGuid(),
            ScanRunId = run.Id,
            AlignmentMatrixJson = JsonSerializer.Serialize(
                response.Calibration.AlignmentMatrix,
                MatrixJsonOptions),
            CalibrationReferenceValue = response.Calibration.CalibrationReferenceValue,
            CalibratedAtUtc = EnsureUtc(response.Calibration.CalibratedAtUtc)
        });

        foreach (var sample in response.Samples.OrderBy(item => item.SequenceIndex))
        {
            db.ColorMetricSamples.Add(new ColorMetricSample
            {
                ScanRunId = run.Id,
                SequenceIndex = sample.SequenceIndex,
                CapturedAtUtc = EnsureUtc(sample.CapturedAtUtc),
                L = sample.L,
                A = sample.A,
                B = sample.B,
                DeltaE = sample.DeltaE,
                ShadeGuideValue = sample.ShadeGuideValue,
                ConfidenceScore = sample.ConfidenceScore
            });
        }

        run.BaselineScanRunId = baseline?.BaselineScanRunId;
        run.DeltaEFormula = response.DeltaEFormula.ToString();
        run.AlgorithmVersion = response.AlgorithmVersion;
        run.FailureReason = null;

        ScanRunStatusTransitions.EnsureCanTransition(run.Status, ScanRunStatus.Completed);
        run.Status = ScanRunStatus.Completed;

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ToResponse(run, frameCount, response.Samples.Count);
    }

    private async Task<BaselineColorDto?> ResolveBaselineAsync(
        ScanRun run,
        CancellationToken cancellationToken)
    {
        var baselineRun = await db.Runs
            .AsNoTracking()
            .Where(item =>
                item.RegimenId == run.RegimenId
                && item.Id != run.Id
                && item.Status == ScanRunStatus.Completed)
            .OrderBy(item => item.CapturedAtUtc)
            .ThenBy(item => item.Id)
            .Select(item => new
            {
                item.Id,
                Sample = item.Samples
                    .OrderBy(sample => sample.SequenceIndex)
                    .Select(sample => new { sample.L, sample.A, sample.B })
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (baselineRun?.Sample is null)
        {
            return null;
        }

        return new BaselineColorDto(
            baselineRun.Id,
            baselineRun.Sample.L,
            baselineRun.Sample.A,
            baselineRun.Sample.B);
    }

    private static string TrimFailure(string message) =>
        message.Length <= ScanRun.FailureReasonMaxLength
            ? message
            : message[..ScanRun.FailureReasonMaxLength];

    private static DateTime EnsureUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

    private static ScanRunResponse ToResponse(ScanRun run, int frameCount, int sampleCount) =>
        new(
            run.Id,
            run.RegimenId,
            run.CapturedAtUtc,
            run.DeviceId,
            run.Status,
            run.BaselineScanRunId,
            run.DeltaEFormula,
            run.AlgorithmVersion,
            run.FailureReason,
            frameCount,
            sampleCount);
}
