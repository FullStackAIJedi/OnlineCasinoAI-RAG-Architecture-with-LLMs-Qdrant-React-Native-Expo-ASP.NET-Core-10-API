namespace Casino.RagService.Abstractions
{
    using Casino.RagService.Models;  

    /// <summary>
    /// Abstraction for retrieving relevant policy chunks from a vector store.
    /// Implementations must return chunks ordered by descending similarity score.
    /// </summary>
    public interface IRetrievalService
    {
        Task<IReadOnlyList<PolicyChunk>> SearchAsync(
            ReadOnlyMemory<float> queryVector,
            int limit,
            double minScore,
            CancellationToken ct = default);
    }
}
