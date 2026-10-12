namespace OnlineCasino.Tests.Common.Fixtures;

using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

using OnlineCasino.Data;

public sealed class SqliteWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;
    private readonly bool _withAuth;

    public SqliteWebApplicationFactory(bool withAuth = true)
    {
        _withAuth = withAuth;
        var connectionString = $"Data Source=file:testdb-{Guid.NewGuid():N}?mode=memory&cache=shared";
        _connection = new SqliteConnection(connectionString);
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var ctxOptions = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (ctxOptions != null) services.Remove(ctxOptions);

            var ctx = services.SingleOrDefault(d => d.ServiceType == typeof(AppDbContext));
            if (ctx != null) services.Remove(ctx);

            services.AddSingleton<SqliteConnection>(_connection);

            services.AddDbContext<AppDbContext>((sp, opt) =>
            {
                var conn = sp.GetRequiredService<SqliteConnection>();
                opt.UseSqlite(conn);
            });

            if (_withAuth)
            {
                services.AddAuthentication(TestAuthHandler.SchemeName)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.SchemeName, _ => { });
            }
            else
            {
                services.AddAuthentication()
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandlerThatFails>(
                        "NoAuth", _ => { });
            }
        });
    }

    public async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
        await TestDbFactory.SeedLookupsAsync(db);
    }

    public async Task ExecuteDbAsync(Func<AppDbContext, Task> action)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await action(db);
    }

    public async Task<T> QueryDbAsync<T>(Func<AppDbContext, Task<T>> query)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await query(db);
    }

    public async Task<AppDbContext> GetSeededDbAsync()
    {
        var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await TestDbFactory.SeedLookupsAsync(db);
        return db;
    }

    public static void SetUser(int userId, string role = "User",
        string email = "test@test.com", string nick = "tester")
    {
        TestAuthHandler.UserId = userId;
        TestAuthHandler.Role = role;
        TestAuthHandler.Email = email;
        TestAuthHandler.NickName = nick;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Close();
            _connection.Dispose();
        }
    }
}

public sealed class TestAuthHandlerThatFails : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandlerThatFails(
        Microsoft.Extensions.Options.IOptionsMonitor<AuthenticationSchemeOptions> options,
        Microsoft.Extensions.Logging.ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        => Task.FromResult(AuthenticateResult.NoResult());
}
