namespace Casino.RagService.Options
{ 
    /// <summary>
    /// Configuration for the Qdrant vector database connection.
    /// Bound from the "Qdrant" section in appsettings.
    /// </summary>
    public class QdrantOptions
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 6334;
        public string CollectionName { get; set; } = "casino_policies";
        public int VectorSize { get; set; } = 768;
    }
}
