namespace Casino.RagService.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    using Casino.RagService.Abstractions;
    using Casino.RagService.Models; 
 
    /// <summary>
    /// Chat endpoints for answering user questions about casino policies.
    /// Uses RAG: retrieves relevant policies from Qdrant and generates an answer via LLM.
    /// </summary>
    [ApiController]
    [Route("api/chat")]
    public class PolicyChatController : ControllerBase
    {
        private readonly IRagService _rag;

        public PolicyChatController(IRagService rag) => _rag = rag;

        /// <summary>
        /// Answers a user question about casino policies.
        /// Returns the generated answer and the list of source policy files.
        /// </summary>
        [HttpPost("policy-question")]
        public async Task<IActionResult> Ask([FromBody] ChatRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.Question))
                return BadRequest(new { message = "Question is required." });

            var result = await _rag.AnswerAsync(req.Question, ct);
            return Ok(result);
        }
    }
}
