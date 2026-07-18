namespace PearlMetric.GatewayApi.Contracts.CvWorker;

public sealed record FrameReferenceDto(
    int SequenceIndex,
    string ImageUri,
    DateTime CapturedAtUtc);
