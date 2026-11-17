namespace Casino.RagService.Options
{ 
    /// <summary>
    /// Configuration for Groq Cloud LLM provider.
    /// Bound from the "Groq" section in appsettings.
    /// </summary>
    public class GroqOptions
    {
        public string ApiKey { get; set; } = null!;
        public string Model { get; set; } = "openai/gpt-oss-20b";
        public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1/";
    }
}
