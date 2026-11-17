namespace Casino.RagService.Infrastructure
{
    using System.Text.Json;
    using Microsoft.Extensions.AI;

    using Casino.RagService.Options;

    /// <summary>
    /// IEmbeddingGenerator implementation for Cloudflare Workers AI (@cf/baai/bge-base-en-v1.5).
    /// Generates 768-dimensional vectors for RAG retrieval.
    /// </summary>
    public class CloudflareEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
    {
        private readonly HttpClient _http;
        private readonly CloudflareOptions _opts;

        public CloudflareEmbeddingGenerator(HttpClient http, CloudflareOptions opts)
        {
            _http = http;
            _opts = opts;
        }

        public EmbeddingGeneratorMetadata Metadata => new(
            providerName: "Cloudflare Workers AI",
            providerUri: new Uri("https://api.cloudflare.com"),
            defaultModelId: _opts.Model);

        public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
            IEnumerable<string> values,
            EmbeddingGenerationOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var texts = values.ToList();
            var results = new List<Embedding<float>>();

            foreach (var text in texts)
            {
                var request = new { text = new[] { text } };
                var url = $"https://api.cloudflare.com/client/v4/accounts/{_opts.AccountId}/ai/run/{_opts.Model}";

                var req = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = JsonContent.Create(request)
                };
                req.Headers.Add("Authorization", $"Bearer {_opts.ApiToken}");

                var response = await _http.SendAsync(req, cancellationToken);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
                var vector = json.GetProperty("result").GetProperty("data")[0]
                    .EnumerateArray().Select(e => e.GetSingle()).ToArray();

                results.Add(new Embedding<float>(vector));
            }

            return new GeneratedEmbeddings<Embedding<float>>(results);
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose() => _http.Dispose();
    }
}
