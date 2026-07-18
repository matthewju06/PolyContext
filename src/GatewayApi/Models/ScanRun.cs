namespace PearlMetric.GatewayApi.Models;

public class ScanRun
{
    public Guid Id { get; set; }
    public Guid RegimenId { get; set; }
    public DateTime CapturedAtUtc { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public ScanRunStatus Status { get; set; }

    public WhiteningRegimen Regimen { get; set; } = null!;
    public ICollection<ColorMetricSample> Samples { get; set; } = new List<ColorMetricSample>();
    public CalibrationProfile? Calibration { get; set; }
}
