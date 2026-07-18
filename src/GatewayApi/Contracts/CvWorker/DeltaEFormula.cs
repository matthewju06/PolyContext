using System.Text.Json.Serialization;

namespace PearlMetric.GatewayApi.Contracts.CvWorker;

[JsonConverter(typeof(JsonStringEnumConverter<DeltaEFormula>))]
public enum DeltaEFormula
{
    CIEDE2000,
    CIE76,
    CIE94
}
