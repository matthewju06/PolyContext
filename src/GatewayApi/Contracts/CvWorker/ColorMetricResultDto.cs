namespace PearlMetric.GatewayApi.Contracts.CvWorker;

public sealed record ColorMetricResultDto(
    int SequenceIndex,
    DateTime CapturedAtUtc,
    double L,
    double A,
    double B,
    double DeltaE,
    string? ShadeGuideValue,
    double ConfidenceScore);
