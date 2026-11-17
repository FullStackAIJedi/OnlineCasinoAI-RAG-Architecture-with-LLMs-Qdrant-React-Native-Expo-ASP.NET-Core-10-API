using Casino.RagService.Models;

namespace Casino.RagService.Abstractions
{
    /// <summary>
    /// RAG orchestrator: receives a question, retrieves relevant policies from Qdrant,
    /// generates an answer using the LLM, and returns the answer along with its sources.
    /// </summary>
    public interface IRagService
    {
        Task<ChatResponse> AnswerAsync(string question, CancellationToken ct = default);
    }
}
