namespace Casino.RagService.Services
{
    using Microsoft.Extensions.AI; 
    using Casino.RagService.Abstractions;

    public class RagService : IRagService
    {
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embedder;
        private readonly IRetrievalService _retrieval;
        private readonly IChatClient _llm;

        public RagService(
            IEmbeddingGenerator<string, Embedding<float>> embedder,
            IRetrievalService retrieval,
            IChatClient llm)
        {
            _embedder = embedder;
            _retrieval = retrieval;
            _llm = llm;
        }

        public async Task<Models.ChatResponse> AnswerAsync(string question, CancellationToken ct = default)
        {
            var embeddings = await _embedder.GenerateAsync(new[] { question }, cancellationToken: ct);
            var queryVector = embeddings[0].Vector;

            var chunks = await _retrieval.SearchAsync(queryVector, limit: 3, minScore: 0.65, ct);

            if (chunks.Count == 0)
            {
                return new Models.ChatResponse
                {
                    Answer = "I don't have information about that. Please contact support.",
                    Sources = Array.Empty<string>()
                };
            }

            var context = string.Join("\n\n---\n\n", chunks.Select(c => c.Content));

            var messages = new[]
            {
                new ChatMessage(ChatRole.System,
                    "You are a casino support assistant. Answer ONLY using the provided policy context. " +
                    "If the context doesn't contain the answer, say you don't know. Be concise and accurate."),
                new ChatMessage(ChatRole.User,
                    $"Context:\n{context}\n\nQuestion: {question}")
            };

            var response = await _llm.GetResponseAsync(messages,
                new ChatOptions { Temperature = 0.0f, MaxOutputTokens = 300 }, ct);

            return new Models.ChatResponse
            {
                Answer = response.Text ?? "",
                Sources = chunks.Select(c => c.SourceFile).Distinct().ToArray()
            };
        }
    }
}