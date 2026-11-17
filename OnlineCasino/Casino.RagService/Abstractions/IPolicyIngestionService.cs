namespace Casino.RagService.Abstractions
{ 
    /// <summary>
    /// Abstraction for loading policy documents into the vector store.
    /// Runs once at startup or on demand via an admin endpoint.
    /// </summary>
    public interface IPolicyIngestionService
    {
        Task IngestAsync(CancellationToken ct = default);
    }
}
