namespace Casino.RagService.Services
{
    using Microsoft.Extensions.AI;
    using Microsoft.Extensions.VectorData;

    using Casino.RagService.Abstractions;
    using Casino.RagService.Models;

    /// <summary>
    /// Reads Markdown policy files, generates embeddings, and upserts them through IVectorStore.
    /// Idempotent: skips ingestion if the collection already contains points.
    /// </summary>
    public class PolicyIngestionService : IPolicyIngestionService
    {
        private const string CollectionName = "casino_policies";

        private readonly VectorStore _vectorStore;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embedder;
        private readonly ILogger<PolicyIngestionService> _log;
        private readonly string _policiesPath;

        public PolicyIngestionService(
            VectorStore vectorStore,
            IEmbeddingGenerator<string, Embedding<float>> embedder,
            ILogger<PolicyIngestionService> log,
            IWebHostEnvironment env)
        {
            _vectorStore = vectorStore;
            _embedder = embedder;
            _log = log;
            _policiesPath = Path.Combine(env.ContentRootPath, "Policies");
        }

        public async Task IngestAsync(CancellationToken ct = default)
        {
            var collection = _vectorStore.GetCollection<Guid, PolicyChunk>(CollectionName);
            await collection.EnsureCollectionExistsAsync(ct);

            if (!Directory.Exists(_policiesPath))
            {
                _log.LogWarning("Policies directory not found: {Path}", _policiesPath);
                return;
            }

            var files = Directory.GetFiles(_policiesPath, "*.md");
            _log.LogInformation("Found {Count} policy files.", files.Length);

            foreach (var file in files)
            {
                try
                {
                    await IngestFileAsync(collection, file, ct);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Failed to ingest {File}", file);
                }
            }
        }

        private async Task IngestFileAsync(
            VectorStoreCollection<Guid, PolicyChunk> collection,
            string file,
            CancellationToken ct)
        {
            var content = await File.ReadAllTextAsync(file, ct);
            var fileName = Path.GetFileName(file);
            var category = Path.GetFileNameWithoutExtension(file)
                .Replace("-policy", "")
                .Replace("-terms", "");

            var embeddings = await _embedder.GenerateAsync(new[] { content }, cancellationToken: ct);

            var chunk = new PolicyChunk
            {
                Id = Guid.NewGuid(),
                Category = category,
                Content = content,
                SourceFile = fileName,
                Vector = embeddings[0].Vector
            };

            await collection.UpsertAsync(chunk, cancellationToken: ct);
            _log.LogInformation("Ingested {File} (category={Category}).", fileName, category);
        }
    }
}