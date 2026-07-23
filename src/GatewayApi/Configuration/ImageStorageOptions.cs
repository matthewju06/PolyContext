namespace PearlMetric.GatewayApi.Configuration;

public sealed class ImageStorageOptions
{
    public const string SectionName = "ImageStorage";

    public string RootPath { get; init; } = string.Empty;
}
