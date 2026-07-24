using PearlMetric.GatewayApi.Contracts.CvWorker;

namespace PearlMetric.GatewayApi.Services.Cv;

public interface ICvAnalysisClient
{
    Task<AnalyzeScanResponse> AnalyzeAsync(
        AnalyzeScanRequest request,
        CancellationToken cancellationToken);
}
