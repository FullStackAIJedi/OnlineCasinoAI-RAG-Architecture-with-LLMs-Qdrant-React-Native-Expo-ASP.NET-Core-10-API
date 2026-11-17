using Microsoft.Extensions.AI;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;

using Casino.RagService.Abstractions;
using Casino.RagService.Infrastructure;
using Casino.RagService.Options;
using Casino.RagService.Services;
using CommunityToolkit.VectorData.Qdrant;
 
using Qdrant.Client;

var builder = WebApplication.CreateBuilder(args);
 
builder.Services.Configure<QdrantOptions>(builder.Configuration.GetSection("Qdrant"));
builder.Services.Configure<GroqOptions>(builder.Configuration.GetSection("Groq"));
builder.Services.Configure<CloudflareOptions>(builder.Configuration.GetSection("Cloudflare"));
 
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<GroqOptions>>().Value);
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<CloudflareOptions>>().Value);

builder.Services.AddSingleton<QdrantClient>(sp =>
{
    var opts = sp.GetRequiredService<IOptions<QdrantOptions>>().Value;
    return new QdrantClient(opts.Host, opts.Port);
});

builder.Services.AddHttpClient<IChatClient, GroqChatClient>()
    .AddStandardResilienceHandler();

builder.Services.AddHttpClient<IEmbeddingGenerator<string, Embedding<float>>, CloudflareEmbeddingGenerator>()
    .AddStandardResilienceHandler();

builder.Services.AddSingleton<VectorStore>(sp =>
{
    var qdrant = sp.GetRequiredService<QdrantClient>();
    var embeddingGenerator = sp.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

    return new QdrantVectorStore(qdrant, ownsClient: false, new QdrantVectorStoreOptions
    {
        EmbeddingGenerator = embeddingGenerator
    });
});

builder.Services.AddSingleton<IRetrievalService, VectorStoreRetrievalService>();
builder.Services.AddSingleton<IRagService, RagService>();
builder.Services.AddSingleton<IPolicyIngestionService, PolicyIngestionService>();
builder.Services.AddHostedService<PolicyIngestionHostedService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();