namespace PearlMetric.GatewayApi.Contracts.CvWorker;

public sealed record AnalyzeScanResponse(
    Guid ScanRunId,
    string AlgorithmVersion,
    DeltaEFormula DeltaEFormula,
    CalibrationResultDto Calibration,
    IReadOnlyList<ColorMetricResultDto> Samples);
