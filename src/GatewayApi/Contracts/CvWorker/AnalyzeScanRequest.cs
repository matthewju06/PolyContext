namespace PearlMetric.GatewayApi.Contracts.CvWorker;

public sealed record AnalyzeScanRequest(
    Guid ScanRunId,
    IReadOnlyList<FrameReferenceDto> Frames,
    BaselineColorDto? Baseline,
    DeltaEFormula DeltaEFormula = DeltaEFormula.CIEDE2000);
