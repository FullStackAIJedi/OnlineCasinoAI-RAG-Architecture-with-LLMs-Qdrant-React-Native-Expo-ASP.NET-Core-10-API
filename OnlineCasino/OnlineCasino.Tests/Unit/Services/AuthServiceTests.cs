namespace OnlineCasino.Tests.Unit.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

using Xunit;
using FluentAssertions;

using AutoMapper;

using OnlineCasino.Data;
using OnlineCasino.DTOs.Auth;
using OnlineCasino.Mappings;
using OnlineCasino.Services;

using OnlineCasino.Tests.Common.Fixtures;
using OnlineCasino.Tests.Common.TestData;

public sealed class AuthServiceTests
{
    private static IConfiguration BuildConfig() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "super-secret-key-for-demo-min-32-chars!!",
            ["Jwt:Issuer"] = "test-issuer",
            ["Jwt:Audience"] = "test-audience",
            ["Jwt:ExpireMinutes"] = "60"
        }).Build();

    private static IMapper BuildMapper()
    {
        var loggerFactory = NullLoggerFactory.Instance;

        var config = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingProfile>(),
            loggerFactory);

        return config.CreateMapper();
    }

    private static async Task<(AppDbContext db, AuthService svc)> ArrangeAsync()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        return (db, new AuthService(db, BuildConfig(), BuildMapper()));
    }

    [Fact]
    public async Task SignUp_Valid_CreatesUserWithWalletAndReturnsToken()
    {
        var (db, svc) = await ArrangeAsync();

        var result = await svc.SignUpAsync(new SignUpRequest
        {
            Email = "new@test.com",
            NickName = "newbie",
            Password = "Pass123!"
        });

        result.IsSuccess.Should().BeTrue();
        result.Data.Token.Should().NotBeNullOrWhiteSpace();
        result.Data.Expires.Should().BeAfter(DateTime.UtcNow);

        var user = await db.Users.Include(u => u.Wallet).SingleAsync();
        user.Email.Should().Be("new@test.com");
        user.RoleId.Should().Be(TestIds.UserRoleId);
        user.Wallet.Balance.Should().Be(1000m);
        user.Wallet.Currency.Should().Be("EUR");
    }

    [Fact]
    public async Task SignUp_DuplicateEmail_Returns409()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.SignUpAsync(new SignUpRequest
        {
            Email = TestUser.Valid.Email,
            NickName = "other",
            Password = "x"
        });

        result.IsSuccess.Should().BeFalse();
        result.Error!.StatusCode.Should().Be(409);
        result.Error.Message.Should().Contain("Email");
    }

    [Fact]
    public async Task SignUp_DuplicateNickname_Returns409()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.SignUpAsync(new SignUpRequest
        {
            Email = "other@test.com",
            NickName = TestUser.Valid.NickName,
            Password = "x"
        });

        result.Error!.StatusCode.Should().Be(409);
        result.Error.Message.Should().Contain("Nickname");
    }

    [Fact]
    public async Task SignIn_Valid_ReturnsToken()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.SignInAsync(new SignInRequest
        {
            Email = TestUser.Valid.Email,
            Password = TestUser.Valid.Password
        });

        result.IsSuccess.Should().BeTrue();
        result.Data.Token.Should().NotBeNullOrWhiteSpace();
        result.Data.Expires.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task SignIn_UnknownUser_Returns401()
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.SignInAsync(new SignInRequest
        {
            Email = "nope@x.com",
            Password = "x"
        });

        result.Error!.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task SignIn_WrongPassword_Returns401()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.SignInAsync(new SignInRequest
        {
            Email = TestUser.Valid.Email,
            Password = "wrong-password"
        });

        result.Error!.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task SignIn_BlockedAccount_Returns403()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.BlockedAccount.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.SignInAsync(new SignInRequest
        {
            Email = TestUser.Valid.Email,
            Password = TestUser.Valid.Password
        });

        result.Error!.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task ChangePassword_UnknownUser_Returns404()
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.ChangePasswordAsync(999,
            new ChangePasswordRequest { CurrentPassword = "x", NewPassword = "y" });

        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task ChangePassword_WrongCurrent_Returns400()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.ChangePasswordAsync(1,
            new ChangePasswordRequest { CurrentPassword = "wrong", NewPassword = "new" });

        result.Error!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task ChangePassword_Valid_UpdatesHash()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.ChangePasswordAsync(1,
            new ChangePasswordRequest
            {
                CurrentPassword = TestUser.Valid.Password,
                NewPassword = "NewPass123!"
            });

        result.IsSuccess.Should().BeTrue();

        var user = await db.Users.FindAsync(1);
        BCrypt.Net.BCrypt.Verify("NewPass123!", user!.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task SignUp_ReturnsTokenWithCorrectIssuerAndAudience()
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.SignUpAsync(new SignUpRequest
        {
            Email = "jwt@test.com",
            NickName = "jwtuser",
            Password = "Pass123!"
        });

        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Data.Token);

        token.Issuer.Should().Be("test-issuer");
        token.Audiences.Should().Contain("test-audience");
    }

    [Fact]
    public async Task SignUp_TokenContainsExpectedClaims()
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.SignUpAsync(new SignUpRequest
        {
            Email = "claims@test.com",
            NickName = "claimsuser",
            Password = "Pass123!"
        });

        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Data.Token);

        token.Claims.Should().Contain(c => c.Type == "nickname" && c.Value == "claimsuser");
        token.Claims.Should().Contain(c => c.Type == "email" && c.Value == "claims@test.com");
        token.Claims.Should().Contain(c =>
            c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == "User");
    }

    [Fact]
    public async Task SignIn_AdminUser_TokenHasAdminRole()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Admin.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.SignInAsync(new SignInRequest
        {
            Email = TestUser.Admin.Email,
            Password = TestUser.Admin.Password
        });

        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Data.Token);

        token.Claims.Should().Contain(c =>
            c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public async Task SignUp_BothEmailAndNicknameTaken_Returns409ForEmail()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.SignUpAsync(new SignUpRequest
        {
            Email = TestUser.Valid.Email,
            NickName = TestUser.Valid.NickName,
            Password = "x"
        });

        result.Error!.StatusCode.Should().Be(409);
        result.Error.Message.Should().Contain("Email");
    }

    [Fact]
    public async Task ChangePassword_SameAsCurrent_StillSucceeds()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.ChangePasswordAsync(1, new ChangePasswordRequest
        {
            CurrentPassword = TestUser.Valid.Password,
            NewPassword = TestUser.Valid.Password
        });

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ChangePassword_TokenStillIssuedAfterChange()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        await svc.ChangePasswordAsync(1, new ChangePasswordRequest
        {
            CurrentPassword = TestUser.Valid.Password,
            NewPassword = "NewPass123!"
        });

        var signIn = await svc.SignInAsync(new SignInRequest
        {
            Email = TestUser.Valid.Email,
            Password = "NewPass123!"
        });

        signIn.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SignIn_OldPasswordRejected_AfterChange()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        await svc.ChangePasswordAsync(1, new ChangePasswordRequest
        {
            CurrentPassword = TestUser.Valid.Password,
            NewPassword = "NewPass123!"
        });

        var signIn = await svc.SignInAsync(new SignInRequest
        {
            Email = TestUser.Valid.Email,
            Password = TestUser.Valid.Password
        });

        signIn.Error!.StatusCode.Should().Be(401);
    } 

    [Fact]
    public async Task SignIn_ProducesToken_WithNameIdentifierClaimForGetUserId()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.SignInAsync(new SignInRequest
        {
            Email = TestUser.Valid.Email,
            Password = TestUser.Valid.Password
        });

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
            .ReadJwtToken(result.Data.Token);

        token.Claims.Should().Contain(c =>
            c.Type == System.Security.Claims.ClaimTypes.NameIdentifier && c.Value == "1");
    }

    [Fact]
    public async Task SignIn_AdminUser_TokenContainsAdminRoleClaim()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Admin.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.SignInAsync(new SignInRequest
        {
            Email = TestUser.Admin.Email,
            Password = TestUser.Admin.Password
        });

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
            .ReadJwtToken(result.Data.Token);

        token.Claims.Should().Contain(c =>
            c.Type == System.Security.Claims.ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public async Task SignIn_TokenIsSigned_WithConfiguredIssuerAndAudience()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.SignInAsync(new SignInRequest
        {
            Email = TestUser.Valid.Email,
            Password = TestUser.Valid.Password
        });

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
            .ReadJwtToken(result.Data.Token);

        token.Issuer.Should().Be("test-issuer");
        token.Audiences.Should().Contain("test-audience");
    }

    [Fact]
    public async Task ChangePassword_OldHashNoLongerValidatesAgainstOldPassword()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        var oldHash = (await db.Users.FindAsync(1))!.PasswordHash;

        await svc.ChangePasswordAsync(1, new ChangePasswordRequest
        {
            CurrentPassword = TestUser.Valid.Password,
            NewPassword = "NewPass123!"
        });

        var newHash = (await db.Users.FindAsync(1))!.PasswordHash;

        newHash.Should().NotBe(oldHash);
        BCrypt.Net.BCrypt.Verify("NewPass123!", newHash).Should().BeTrue();
        BCrypt.Net.BCrypt.Verify(TestUser.Valid.Password, newHash).Should().BeFalse();
    }

    [Fact]
    public async Task SignUp_PersistsHashedPasswordNotPlaintext()
    {
        var (db, svc) = await ArrangeAsync();

        await svc.SignUpAsync(new SignUpRequest
        {
            Email = "hash@test.com",
            NickName = "hash",
            Password = "Plain123!"
        });

        var user = await db.Users.SingleAsync();
        user.PasswordHash.Should().NotBe("Plain123!");
        BCrypt.Net.BCrypt.Verify("Plain123!", user.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task SignUp_TokenIsImmediatelyUsable_ForSignIn()
    {
        var (db, svc) = await ArrangeAsync();

        var signUp = await svc.SignUpAsync(new SignUpRequest
        {
            Email = "flow@test.com",
            NickName = "flow",
            Password = "Pass123!"
        });

        var signIn = await svc.SignInAsync(new SignInRequest
        {
            Email = "flow@test.com",
            Password = "Pass123!"
        });

        signUp.IsSuccess.Should().BeTrue();
        signIn.IsSuccess.Should().BeTrue();
        signIn.Data.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task SignIn_TokenContainsSameNicknameThatWasRegistered()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.SignInAsync(new SignInRequest
        {
            Email = TestUser.Valid.Email,
            Password = TestUser.Valid.Password
        });

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
            .ReadJwtToken(result.Data.Token);

        token.Claims
            .Single(c => c.Type == "nickname")
            .Value.Should().Be(TestUser.Valid.NickName);
    }

    [Fact]
    public async Task SignIn_TokenHasExpirationInConfiguredWindow()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.SignInAsync(new SignInRequest
        {
            Email = TestUser.Valid.Email,
            Password = TestUser.Valid.Password
        });

        result.Data.Expires.Should().BeCloseTo(
            DateTime.UtcNow.AddMinutes(60),
            TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task SignUp_NewUser_HasUserRoleNotAdmin()
    {
        var (db, svc) = await ArrangeAsync();

        await svc.SignUpAsync(new SignUpRequest
        {
            Email = "reg@test.com",
            NickName = "reg",
            Password = "Pass123!"
        });

        var user = await db.Users.SingleAsync();
        user.RoleId.Should().Be(TestIds.UserRoleId);
    }

    [Fact]
    public async Task ChangePassword_OldPasswordNoLongerAuthenticates()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        await svc.ChangePasswordAsync(1, new ChangePasswordRequest
        {
            CurrentPassword = TestUser.Valid.Password,
            NewPassword = "NewPass123!"
        });

        var signIn = await svc.SignInAsync(new SignInRequest
        {
            Email = TestUser.Valid.Email,
            Password = TestUser.Valid.Password
        });

        signIn.Error!.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task SignIn_EmailCaseSensitive_RejectsUpperCase()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.SignInAsync(new SignInRequest
        {
            Email = TestUser.Valid.Email.ToUpperInvariant(),
            Password = TestUser.Valid.Password
        });

        result.Error!.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task ChangePassword_ToIdenticalPassword_StillSucceeds()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.ChangePasswordAsync(1, new ChangePasswordRequest
        {
            CurrentPassword = TestUser.Valid.Password,
            NewPassword = TestUser.Valid.Password
        });

        result.IsSuccess.Should().BeTrue();
    }
}