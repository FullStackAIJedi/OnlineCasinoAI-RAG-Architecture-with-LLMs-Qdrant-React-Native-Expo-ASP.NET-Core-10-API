namespace Casino.RagService.Models
{ 
    /// <summary>
    /// Response returned to the client after RAG processing.
    /// Contains the generated answer and the list of source policy files.
    /// </summary>
    public class ChatResponse
    {
        public string Answer { get; set; } = null!;
        public string[] Sources { get; set; } = Array.Empty<string>();
    }
}
