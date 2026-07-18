namespace PearlMetric.GatewayApi.Models;

public class Patient
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }

    public ICollection<WhiteningRegimen> Regimens { get; set; } = new List<WhiteningRegimen>();
}
