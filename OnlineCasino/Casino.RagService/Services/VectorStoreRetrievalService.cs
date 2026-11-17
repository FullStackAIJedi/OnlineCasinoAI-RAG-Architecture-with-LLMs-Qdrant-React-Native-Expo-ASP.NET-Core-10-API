namespace Casino.RagService.Services
{
    using Microsoft.Extensions.VectorData;

    using Casino.RagService.Abstractions;
    using Casino.RagService.Models; 

    /// <summary>
    /// Retrieval service backed by the VectorStore abstraction.
    /// Does not depend on Qdrant directly – any VectorStore implementation can be plugged in.
    /// </summary>
    public class VectorStoreRetrievalService : IRetrievalService
    {
        private const string CollectionName = "casino_policies";

        private readonly VectorStore _vectorStore;

        public VectorStoreRetrievalService(VectorStore vectorStore)
        {
            _vectorStore = vectorStore;
        }

        public async Task<IReadOnlyList<PolicyChunk>> SearchAsync(
            ReadOnlyMemory<float> queryVector,
            int limit,
            double minScore,
            CancellationToken ct = default)
        {
            var collection = _vectorStore.GetCollection<Guid, PolicyChunk>(CollectionName);
            var chunks = new List<PolicyChunk>();

            var options = new VectorSearchOptions<PolicyChunk>
            {
                ScoreThreshold = (float)minScore
            };

            await foreach (var result in collection.SearchAsync(
                queryVector, top: limit, options, cancellationToken: ct))
            {
                result.Record.Score = (float)result.Score;
                chunks.Add(result.Record);
            }

            return chunks;
        }
    }
}
