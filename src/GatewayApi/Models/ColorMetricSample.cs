namespace PearlMetric.GatewayApi.Models;

public class ColorMetricSample
{
    public long Id { get; set; }
    public Guid ScanRunId { get; set; }
    public int SequenceIndex { get; set; }
    public DateTime CapturedAtUtc { get; set; }
    public double L { get; set; }
    public double A { get; set; }
    public double B { get; set; }
    public double DeltaE { get; set; }
    public string? ShadeGuideValue { get; set; }
    public double ConfidenceScore { get; set; }

    public ScanRun ScanRun { get; set; } = null!;
}
