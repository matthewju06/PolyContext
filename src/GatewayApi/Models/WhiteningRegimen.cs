namespace PearlMetric.GatewayApi.Models;

public class WhiteningRegimen
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public DateTime StartedAtUtc { get; set; }
    public int DurationDays { get; set; }
    public int ScheduledIntervalHours { get; set; }

    public Patient Patient { get; set; } = null!;
    public ICollection<ScanRun> Runs { get; set; } = new List<ScanRun>();
}
