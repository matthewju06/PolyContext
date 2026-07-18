namespace PearlMetric.GatewayApi.Contracts.CvWorker;

public sealed record CalibrationResultDto(
    double[][] AlignmentMatrix,
    double CalibrationReferenceValue,
    DateTime CalibratedAtUtc);
