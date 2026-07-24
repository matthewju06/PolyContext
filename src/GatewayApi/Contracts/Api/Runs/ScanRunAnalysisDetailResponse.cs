namespace PearlMetric.GatewayApi.Contracts.Api.Runs;

public sealed record ColorMetricSampleResponse(
    long Id,
    Guid ScanRunId,
    int SequenceIndex,
    DateTime CapturedAtUtc,
    double L,
    double A,
    double B,
    double DeltaE,
    string? ShadeGuideValue,
    double ConfidenceScore);

public sealed record CalibrationProfileResponse(
    Guid Id,
    Guid ScanRunId,
    string AlignmentMatrixJson,
    double CalibrationReferenceValue,
    DateTime CalibratedAtUtc);

public sealed record ScanRunAnalysisDetailResponse(
    ScanRunResponse Run,
    CalibrationProfileResponse? Calibration,
    IReadOnlyList<ColorMetricSampleResponse> Samples);
