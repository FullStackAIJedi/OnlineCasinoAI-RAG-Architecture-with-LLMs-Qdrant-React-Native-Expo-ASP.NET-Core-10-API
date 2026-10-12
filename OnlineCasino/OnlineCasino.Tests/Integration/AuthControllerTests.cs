namespace OnlineCasino.Tests.Integration;

using System.Net;
using System.Net.Http.Json;

using Xunit;
using FluentAssertions;

using OnlineCasino.DTOs.Auth;
using OnlineCasino.Tests.Common.Fixtures;
using OnlineCasino.Tests.Common.TestData;

public sealed class AuthControllerTests : IntegrationTestBase
{

    [Fact]
    public async Task Register_ValidUser_Returns200AndSetsCookie()
    {
        var response = await PostJsonAsync("/api/auth/register", new SignUpRequest
        {
            Email = "new@test.com",
            NickName = "newbie",
            Password = "Pass123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("Set-Cookie");
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var response = await PostJsonAsync("/api/auth/register", new SignUpRequest
        {
            Email = "test@test.com",
            NickName = "different",
            Password = "Pass123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200AndCookie()
    {
        var response = await PostJsonAsync("/api/auth/login", new SignInRequest
        {
            Email = "test@test.com",
            Password = "Password123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("Set-Cookie");
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var response = await PostJsonAsync("/api/auth/login", new SignInRequest
        {
            Email = "test@test.com",
            Password = "wrong"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_CookieHasSameSiteStrict()
    {
        var response = await PostJsonAsync("/api/auth/login", new SignInRequest
        {
            Email = "test@test.com",
            Password = "Password123!"
        });

        var cookie = response.Headers.GetValues("Set-Cookie").Single();
        cookie.ToLowerInvariant().Should().Contain("samesite=strict");
    }
    [Fact]
    public async Task Login_TwoSessions_BothTokensWork()
    {
        var r1 = await PostJsonAsync("/api/auth/login", new SignInRequest
        {
            Email = "test@test.com",
            Password = "Password123!"
        });
        var cookie1 = r1.Headers.GetValues("Set-Cookie").Single().Split(';')[0];

        var r2 = await PostJsonAsync("/api/auth/login", new SignInRequest
        {
            Email = "test@test.com",
            Password = "Password123!"
        });
        var cookie2 = r2.Headers.GetValues("Set-Cookie").Single().Split(';')[0];

        using var client1 = Factory.CreateClient();
        client1.DefaultRequestHeaders.Add("Cookie", cookie1);
        var me1 = await client1.GetAsync("/api/auth/me");

        using var client2 = Factory.CreateClient();
        client2.DefaultRequestHeaders.Add("Cookie", cookie2);
        var me2 = await client2.GetAsync("/api/auth/me");

        me1.StatusCode.Should().Be(HttpStatusCode.OK);
        me2.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_TwoSessions_BothTokensValid()
    {
        var r1 = await PostJsonAsync("/api/auth/login", new SignInRequest
        {
            Email = "test@test.com",
            Password = "Password123!"
        });
        var cookie1 = r1.Headers.GetValues("Set-Cookie").Single()
            .Split(';')[0];

        await Task.Delay(1100);

        var r2 = await PostJsonAsync("/api/auth/login", new SignInRequest
        {
            Email = "test@test.com",
            Password = "Password123!"
        });
        var cookie2 = r2.Headers.GetValues("Set-Cookie").Single()
            .Split(';')[0];

        cookie1.Should().NotBe(cookie2);

        using var client2 = Factory.CreateClient();
        client2.DefaultRequestHeaders.Add("Cookie", cookie1);
        var meWithOld = await client2.GetAsync("/api/auth/me");

        meWithOld.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Me_Authenticated_ReturnsClaims()
    {
        SetUser(1, "User", "test@test.com", "tester");

        var response = await Client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<MeResponse>();
        body!.Id.Should().Be(1);
        body.Email.Should().Be("test@test.com");
        body.Role.Should().Be("User");
    }

    [Fact]
    public async Task Logout_Authenticated_ClearsCookie()
    {
        SetUser(1);

        var response = await Client.PostAsync("/api/auth/logout", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangePassword_Valid_Returns200()
    {
        SetUser(1);

        var response = await PostJsonAsync("/api/auth/change-password", new ChangePasswordRequest
        {
            CurrentPassword = "Password123!",
            NewPassword = "NewPass123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangePassword_WrongCurrent_Returns400()
    {
        SetUser(1);

        var response = await PostJsonAsync("/api/auth/change-password", new ChangePasswordRequest
        {
            CurrentPassword = "wrong",
            NewPassword = "NewPass123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_AndThenLogin_Works()
    {
        await PostJsonAsync("/api/auth/register", new OnlineCasino.DTOs.Auth.SignUpRequest
        {
            Email = "fresh@test.com",
            NickName = "fresh",
            Password = "Pass123!"
        });

        var login = await PostJsonAsync("/api/auth/login", new OnlineCasino.DTOs.Auth.SignInRequest
        {
            Email = "fresh@test.com",
            Password = "Pass123!"
        });

        login.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_DuplicateNickname_Returns409()
    {
        var response = await PostJsonAsync("/api/auth/register", new OnlineCasino.DTOs.Auth.SignUpRequest
        {
            Email = "unique@test.com",
            NickName = "tester",
            Password = "Pass123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_BlockedUser_Returns403()
    {
        var db = await Factory.GetSeededDbAsync();
        db.Users.Add((TestUser.BlockedAccount with { Email = "blocked@t.com", NickName = "blocked" }).ToEntity());
        await db.SaveChangesAsync();

        var response = await PostJsonAsync("/api/auth/login", new OnlineCasino.DTOs.Auth.SignInRequest
        {
            Email = "blocked@t.com",
            Password = TestUser.Valid.Password
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }  

    [Fact]
    public async Task ChangePassword_ThenLoginWithNewPassword()
    {
        var userId = await Factory.QueryDbAsync(db =>
            Task.FromResult(db.Users.First().Id));

        SqliteWebApplicationFactory.SetUser(userId);

        await PostJsonAsync("/api/auth/change-password", new ChangePasswordRequest
        {
            CurrentPassword = "Password123!",
            NewPassword = "NewPass123!"
        });

        SqliteWebApplicationFactory.SetUser(userId);

        var response = await PostJsonAsync("/api/auth/login", new SignInRequest
        {
            Email = "test@test.com",
            Password = "NewPass123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    } 
     

    [Fact]
    public async Task Register_TokenNotLeakedInResponseBody()
    {
        var response = await PostJsonAsync("/api/auth/register", new SignUpRequest
        {
            Email = "leak@test.com",
            NickName = "leak",
            Password = "Pass123!"
        });

        var body = await response.Content.ReadAsStringAsync();

        body.Should().NotContain("eyJ");
    }

    [Fact]
    public async Task Login_SecondLoginAfterFirst_ProducesDifferentCookie()
    {
        var r1 = await PostJsonAsync("/api/auth/login", new SignInRequest
        {
            Email = "test@test.com",
            Password = "Password123!"
        });
        var c1 = r1.Headers.GetValues("Set-Cookie").Single();

        await Task.Delay(1100);

        var r2 = await PostJsonAsync("/api/auth/login", new SignInRequest
        {
            Email = "test@test.com",
            Password = "Password123!"
        });
        var c2 = r2.Headers.GetValues("Set-Cookie").Single();

        c1.Should().NotBe(c2);
    }

    [Fact]
    public async Task Login_CookieHasHttpOnlyFlag()
    {
        var response = await PostJsonAsync("/api/auth/login", new SignInRequest
        {
            Email = "test@test.com",
            Password = "Password123!"
        });

        var cookie = response.Headers.GetValues("Set-Cookie").Single();
        cookie.ToLowerInvariant().Should().Contain("httponly");
    }

    [Fact]
    public async Task Logout_ClearsAuthCookie()
    {
        SetUser(1);

        var response = await Client.PostAsync("/api/auth/logout", null);
        var cookie = response.Headers.GetValues("Set-Cookie").Single();

        cookie.Should().Contain("access_token=");
        cookie.Should().Contain("expires=Thu, 01 Jan 1970");
    }

    private sealed record MeResponse(int Id, string? Email, string? Nickname, string? Role);
}