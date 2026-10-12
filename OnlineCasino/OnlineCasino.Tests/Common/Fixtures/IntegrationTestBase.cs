namespace OnlineCasino.Tests.Common.Fixtures;

using System.Net.Http.Json;

using Xunit;

using OnlineCasino.Data;
using OnlineCasino.Tests.Common.TestData;

[CollectionDefinition("Integration", DisableParallelization = true)]
public sealed class IntegrationCollectionMarker { }

[Collection("Integration")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected SqliteWebApplicationFactory Factory { get; private set; } = null!;
    protected HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Factory = new SqliteWebApplicationFactory();
        Client = Factory.CreateClient();
        await Factory.InitializeDatabaseAsync();
        await SeedDefaultUserAsync();
    }

    public Task DisposeAsync()
    {
        Client.Dispose();
        Factory.Dispose();
        return Task.CompletedTask;
    }

    protected virtual Task SeedDefaultUserAsync() => SeedUserAsync(TestUser.Valid);

    protected async Task SeedUserAsync(TestUser user)
    {
        await Factory.ExecuteDbAsync(async db =>
        {
            db.Users.Add(user.ToEntity());
            await db.SaveChangesAsync();
        });
    }

    protected async Task SeedGameAsync(TestGame game)
    {
        await Factory.ExecuteDbAsync(async db =>
        {
            db.Games.Add(game.ToEntity());
            await db.SaveChangesAsync();
        });
    }

    protected async Task<T> QueryDbAsync<T>(Func<AppDbContext, Task<T>> query)
        => await Factory.QueryDbAsync(query);

    protected void SetUser(int userId, string role = "User",
        string email = "test@test.com", string nick = "tester")
        => SqliteWebApplicationFactory.SetUser(userId, role, email, nick);

    protected async Task<T?> GetJsonAsync<T>(string url)
        => await Client.GetFromJsonAsync<T>(url);

    protected async Task<HttpResponseMessage> PostJsonAsync<T>(string url, T body)
        => await Client.PostAsJsonAsync(url, body);

    protected async Task<HttpResponseMessage> PutJsonAsync<T>(string url, T body)
        => await Client.PutAsJsonAsync(url, body);

    protected Task<AppDbContext> GetSeededDbAsync() => Factory.GetSeededDbAsync();
}