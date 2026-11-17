namespace Casino.RagService.Services
{
    using Casino.RagService.Abstractions; 

    /// <summary>
    /// Hosted service that triggers policy ingestion once at application startup.
    /// Ensures the vector store is populated before serving chat requests.
    /// </summary>
    public class PolicyIngestionHostedService : IHostedService
    {
        private readonly IPolicyIngestionService _ingestion;
        private readonly ILogger<PolicyIngestionHostedService> _log;

        public PolicyIngestionHostedService(
            IPolicyIngestionService ingestion,
            ILogger<PolicyIngestionHostedService> log)
        {
            _ingestion = ingestion;
            _log = log;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _log.LogInformation("Starting policy ingestion...");
            try
            {
                await _ingestion.IngestAsync(cancellationToken);
                _log.LogInformation("Policy ingestion completed successfully.");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Policy ingestion failed. RAG chat will return 'no information' until fixed.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
