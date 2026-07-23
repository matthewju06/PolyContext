namespace PearlMetric.GatewayApi.Configuration;

public sealed class CvWorkerOptions
{
    public const string SectionName = "CvWorker";

    public string BaseUrl { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; }
}
