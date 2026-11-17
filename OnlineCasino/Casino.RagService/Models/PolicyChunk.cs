namespace Casino.RagService.Models
{
    using Microsoft.Extensions.VectorData;

    /// <summary>
    /// Represents a single indexed policy chunk stored in the vector database.
    /// Uses Microsoft.Extensions.VectorData attributes so the schema is portable across providers.
    /// </summary>
    public class PolicyChunk
    {
        [VectorStoreKey]
        public Guid Id { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public string Category { get; set; } = null!;

        [VectorStoreData]
        public string Content { get; set; } = null!;

        [VectorStoreData]
        public string SourceFile { get; set; } = null!;

        [VectorStoreVector(768, DistanceFunction = DistanceFunction.CosineSimilarity)]
        public ReadOnlyMemory<float> Vector { get; set; }

        [VectorStoreData]
        public float Score { get; set; }
    }
}