namespace PearlMetric.GatewayApi.Models;

public class CalibrationProfile
{
    public Guid Id { get; set; }
    public Guid ScanRunId { get; set; }
    public string AlignmentMatrixJson { get; set; } = string.Empty;
    public double CalibrationReferenceValue { get; set; }
    public DateTime CalibratedAtUtc { get; set; }

    public ScanRun ScanRun { get; set; } = null!;
}
