namespace Casino.RagService.Infrastructure
{
    using System.Net.Http.Json;
    using System.Runtime.CompilerServices;
    using System.Text.Json;
    
    using Microsoft.Extensions.AI;

    using Casino.RagService.Options;

    /// <summary>
    /// IChatClient implementation for Groq Cloud (OpenAI-compatible endpoint).
    /// Allows the application to use Groq through the unified .NET AI abstraction.
    /// </summary>
    public class GroqChatClient : IChatClient
    {
        private readonly HttpClient _http;
        private readonly GroqOptions _opts;

        public GroqChatClient(HttpClient http, GroqOptions opts)
        {
            _http = http;
            _opts = opts;
            _http.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
            _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_opts.ApiKey}");
        }

        public ChatClientMetadata Metadata => new(
            providerName: "Groq",
            providerUri: new Uri("https://groq.com"),
            defaultModelId: _opts.Model);

        public async Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var request = new
            {
                model = _opts.Model,
                messages = messages.Select(m => new
                {
                    role = m.Role == ChatRole.User ? "user" : "assistant",
                    content = m.Text
                }),
                temperature = options?.Temperature ?? 0.0,
                max_tokens = options?.MaxOutputTokens ?? 300
            };

            var response = await _http.PostAsJsonAsync("chat/completions", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
            var content = json.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            return new ChatResponse(new ChatMessage(ChatRole.Assistant, content));
        }

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        { 
            await Task.CompletedTask;
            yield break;
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose() => _http.Dispose();
    }
}
