namespace PearlMetric.GatewayApi.Contracts.CvWorker;

public sealed record BaselineColorDto(
    Guid BaselineScanRunId,
    double L,
    double A,
    double B);
