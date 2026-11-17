namespace Casino.RagService.Models
{ 
    /// <summary>
    /// Incoming request payload for the policy question endpoint.
    /// </summary>
    public class ChatRequest
    {
        public string Question { get; set; } = null!;
    }
}
