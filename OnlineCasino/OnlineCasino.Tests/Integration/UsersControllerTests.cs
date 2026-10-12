namespace OnlineCasino.Tests.Integration;

using System.Net;
using System.Net.Http.Json;

using Xunit;
using FluentAssertions;

using OnlineCasino.DTOs.Users;
using OnlineCasino.Tests.Common.Fixtures;

public sealed class UsersControllerTests : IntegrationTestBase
{
    [Fact]
    public async Task GetMe_NoAuthCookie_Returns401()
    {
        using var factory = new SqliteWebApplicationFactory(withAuth: false);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateMe_NoAuthCookie_Returns401()
    {
        using var factory = new SqliteWebApplicationFactory(withAuth: false);
        var client = factory.CreateClient();

        var response = await client.PutAsJsonAsync("/api/users/me",
            new OnlineCasino.DTOs.Users.UpdateProfileRequest { FirstName = "x" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteMe_NoAuthCookie_Returns401()
    {
        using var factory = new SqliteWebApplicationFactory(withAuth: false);
        var client = factory.CreateClient();

        var response = await client.DeleteAsync("/api/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAll_NoAuthCookie_Returns401()
    {
        using var factory = new SqliteWebApplicationFactory(withAuth: false);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Block_NoAuthCookie_Returns401()
    {
        using var factory = new SqliteWebApplicationFactory(withAuth: false);
        var client = factory.CreateClient();

        var response = await client.PostAsync("/api/users/1/block", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    [Fact]
    public async Task GetMe_Authenticated_ReturnsProfile()
    {
        SetUser(1);

        var response = await Client.GetAsync("/api/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<UserProfileDto>();
        body!.Id.Should().Be(1);
        body.Email.Should().Be("test@test.com");
    }

    [Fact]
    public async Task UpdateMe_ValidInput_UpdatesProfile()
    {
        SetUser(1);

        var response = await PutJsonAsync("/api/users/me",
            new UpdateProfileRequest { FirstName = "Ivan", LastName = "Ivanov" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<UserProfileDto>();
        body!.FirstName.Should().Be("Ivan");
        body.LastName.Should().Be("Ivanov");
    }

    [Fact]
    public async Task DeleteMe_Authenticated_RemovesUser()
    {
        SetUser(1);

        var response = await Client.DeleteAsync("/api/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var count = await Factory.QueryDbAsync(db =>
            Task.FromResult(db.Users.Count()));
        count.Should().Be(0);
    }

    [Fact]
    public async Task GetAll_AsAdmin_ReturnsUsers()
    {
        SetUser(1, "Admin");

        var response = await Client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<List<UserListItemDto>>();
        list.Should().NotBeNull();
        list!.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAll_AsUser_Returns403()
    {
        SetUser(1, "User");

        var response = await Client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Block_AsAdmin_Returns200()
    {
        SetUser(1, "Admin");

        var response = await Client.PostAsync("/api/users/1/block", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<bool>();
        body.Should().BeTrue();

        var user = await Factory.QueryDbAsync(db =>
            Task.FromResult(db.Users.Find(1)!));
        user.IsBlocked.Should().BeTrue();
    }

    [Fact]
    public async Task Unblock_AsAdmin_Returns200()
    {
        SetUser(1, "Admin");

        var response = await Client.PostAsync("/api/users/1/unblock", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await Factory.QueryDbAsync(db =>
            Task.FromResult(db.Users.Find(1)!));
        user.IsBlocked.Should().BeFalse();
    }

    [Fact]
    public async Task Block_AsUser_Returns403()
    {
        SetUser(1, "User");

        var response = await Client.PostAsync("/api/users/1/block", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetMe_WithoutAuth_Returns401()
    {
        Client.DefaultRequestHeaders.Clear(); 
    }

    [Fact]
    public async Task UpdateMe_WithEmptyBody_PreservesFields()
    {
        SetUser(1);
        await PutJsonAsync("/api/users/me", new OnlineCasino.DTOs.Users.UpdateProfileRequest
        {
            FirstName = "Ivan",
            LastName = "Ivanov"
        });

        var response = await PutJsonAsync("/api/users/me",
            new OnlineCasino.DTOs.Users.UpdateProfileRequest());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<OnlineCasino.DTOs.Users.UserProfileDto>();
        body!.FirstName.Should().Be("Ivan");
        body.LastName.Should().Be("Ivanov");
    }

    [Fact]
    public async Task Block_NonExistingUser_Returns404()
    {
        SetUser(1, "Admin");

        var response = await Client.PostAsync("/api/users/9999/block", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    } 

    [Fact]
    public async Task DeleteMe_ThenGetMe_Returns404()
    {
        SetUser(1);

        await Client.DeleteAsync("/api/users/me");

        var response = await Client.GetAsync("/api/users/me");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_AdminSeesUserDetails()
    {
        SetUser(1, "Admin");

        var response = await Client.GetAsync("/api/users");
        var list = await response.Content.ReadFromJsonAsync<List<OnlineCasino.DTOs.Users.UserListItemDto>>();

        list!.Single().Balance.Should().Be(1000m);
        list.Single().Email.Should().Be("test@test.com");
    }

    [Fact]
    public async Task GetMe_ResponseDoesNotContainPasswordHash()
    {
        SetUser(1);

        var response = await Client.GetAsync("/api/users/me");
        var body = await response.Content.ReadAsStringAsync();

        body.Should().NotContain("passwordHash");
        body.Should().NotContain("PasswordHash");
    }

    [Fact]
    public async Task GetAll_ResponseDoesNotIncludePasswordHash()
    {
        SetUser(1, "Admin");

        var response = await Client.GetAsync("/api/users");
        var body = await response.Content.ReadAsStringAsync();

        body.Should().NotContain("passwordHash");
        body.Should().NotContain("PasswordHash");
    }
}