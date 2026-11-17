namespace Casino.RagService.Options
{ 
    /// <summary>
    /// Configuration for Cloudflare Workers AI embedding provider.
    /// Bound from the "Cloudflare" section in appsettings.
    /// </summary>
    public class CloudflareOptions
    {
        public string AccountId { get; set; } = null!;
        public string ApiToken { get; set; } = null!;
        public string Model { get; set; } = "@cf/baai/bge-base-en-v1.5";
        public int VectorSize { get; set; } = 768;
    }
}
