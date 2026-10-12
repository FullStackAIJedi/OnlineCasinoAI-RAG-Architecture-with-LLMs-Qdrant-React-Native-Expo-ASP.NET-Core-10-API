namespace OnlineCasino.Tests.Unit.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

using Xunit;
using AutoMapper;
using FluentAssertions;

using OnlineCasino.Data;
using OnlineCasino.DTOs.Users;
using OnlineCasino.Mappings;
using OnlineCasino.Services;

using OnlineCasino.Tests.Common.Fixtures;
using OnlineCasino.Tests.Common.TestData;

public sealed class UserServiceTests
{
    private static IMapper BuildMapper()
    {
        var loggerFactory = NullLoggerFactory.Instance;
        var cfg = new MapperConfiguration(
            c => c.AddProfile<MappingProfile>(),
            loggerFactory);
        return cfg.CreateMapper();
    }

    private static async Task<(AppDbContext db, UserService svc)> ArrangeAsync()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        return (db, new UserService(db, BuildMapper()));
    }

    [Fact]
    public async Task GetProfile_Existing_ReturnsProjectedProfile()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.GetProfileAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Email.Should().Be(TestUser.Valid.Email);
        result.Data.NickName.Should().Be(TestUser.Valid.NickName);
        result.Data.Role.Should().Be("User");
    }

    [Fact]
    public async Task GetProfile_Missing_Returns404()
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.GetProfileAsync(999);

        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateProfile_OnlyFirstName_PreservesOthers()
    {
        var (db, svc) = await ArrangeAsync();
        var user = TestUser.Valid.ToEntity();
        user.FirstName = "Old";
        user.LastName = "Name";
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var result = await svc.UpdateProfileAsync(1,
            new UpdateProfileRequest { FirstName = "New" });

        result.IsSuccess.Should().BeTrue();
        result.Data!.FirstName.Should().Be("New");
        result.Data.LastName.Should().Be("Name");
    }

    [Fact]
    public async Task UpdateProfile_AllNull_PreservesAll()
    {
        var (db, svc) = await ArrangeAsync();
        var user = TestUser.Valid with { FirstName = "F", LastName = "L" };
        db.Users.Add(user.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.UpdateProfileAsync(1, new UpdateProfileRequest());

        result.Data!.FirstName.Should().Be("F");
        result.Data.LastName.Should().Be("L");
    }

    [Fact]
    public async Task UpdateProfile_Missing_Returns404()
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.UpdateProfileAsync(999,
            new UpdateProfileRequest { FirstName = "x" });

        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteAccount_RemovesUser()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.DeleteAccountAsync(1);

        result.IsSuccess.Should().BeTrue();
        (await db.Users.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task DeleteAccount_Missing_Returns404()
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.DeleteAccountAsync(999);

        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetAll_ReturnsProjectedList()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        db.Users.Add((TestUser.Admin with { Id = null }).ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.GetAllAsync();

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data.Should().ContainSingle(u => u.Role == "Admin");
    }

    [Fact]
    public async Task SetBlocked_True_UpdatesFlag()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.SetBlockedAsync(1, true);

        result.IsSuccess.Should().BeTrue();
        var user = await db.Users.FindAsync(1);
        user!.IsBlocked.Should().BeTrue();
    }

    [Fact]
    public async Task SetBlocked_False_UpdatesFlag()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.BlockedAccount.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.SetBlockedAsync(1, false);

        result.IsSuccess.Should().BeTrue();
        var user = await db.Users.FindAsync(1);
        user!.IsBlocked.Should().BeFalse();
    }

    [Fact]
    public async Task SetBlocked_Missing_Returns404()
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.SetBlockedAsync(999, true);

        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateProfile_OnlyLastName_PreservesFirstName()
    {
        var (db, svc) = await ArrangeAsync();
        var user = TestUser.Valid with { FirstName = "Old" };
        db.Users.Add(user.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.UpdateProfileAsync(1,
            new UpdateProfileRequest { LastName = "New" });

        result.Data!.FirstName.Should().Be("Old");
        result.Data.LastName.Should().Be("New");
    }

    [Fact]
    public async Task UpdateProfile_ProfileImageOnly_Updates()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.UpdateProfileAsync(1,
            new UpdateProfileRequest { PofileImage = "avatar.png" });

        result.Data!.PofileImage.Should().Be("avatar.png");
    }

    [Fact]
    public async Task UpdateProfile_EmptyStrings_SetsEmpty()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.UpdateProfileAsync(1,
            new UpdateProfileRequest { FirstName = "", LastName = "" });

        result.Data!.FirstName.Should().Be("");
        result.Data.LastName.Should().Be("");
    }

    [Fact]
    public async Task DeleteAccount_RemovesWalletToo()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        await svc.DeleteAccountAsync(1);

        (await db.Users.CountAsync()).Should().Be(0);
        (await db.Wallets.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task GetAll_IncludesBlockedAndBalance()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        db.Users.Add((TestUser.BlockedAccount with { Id = null }).ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.GetAllAsync();

        result.Data.Should().HaveCount(2);
        result.Data.Should().Contain(u => u.IsBlocked);
        result.Data.Should().OnlyContain(u => u.Balance == 1000m);
    }

    [Fact]
    public async Task GetAll_EmptyDatabase_ReturnsEmptyList()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.RemoveRange(db.Users);
        await db.SaveChangesAsync();

        var result = await svc.GetAllAsync();

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_IncludesAdminRole()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add((TestUser.Admin with { Id = null }).ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.GetAllAsync();

        result.Data.Should().ContainSingle(u => u.Role == "Admin");
    }

    [Fact]
    public async Task GetProfile_WithAdminRole_ReturnsAdminRole()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add((TestUser.Admin with { Id = null }).ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.GetProfileAsync(1);

        result.Data!.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task GetProfile_ExposesCreatedAt()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.GetProfileAsync(1);

        result.Data!.CreatedAt.Should().Be(TestClock.FixedNow);
    }

    [Fact]
    public async Task GetProfile_DoesNotExposePasswordHash()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.GetProfileAsync(1);

        typeof(OnlineCasino.DTOs.Users.UserProfileDto)
            .GetProperty("PasswordHash")
            .Should().BeNull();
    }

    [Fact]
    public async Task SetBlocked_Idempotent_WhenAlreadyBlocked()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.BlockedAccount.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.SetBlockedAsync(1, true);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SetBlocked_ThenUnblock_Succeeds()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        await svc.SetBlockedAsync(1, true);
        await svc.SetBlockedAsync(1, false);

        var user = await db.Users.FindAsync(1);
        user!.IsBlocked.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAccount_RemovesWalletAndRounds_NotJustUser()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        db.GameRounds.Add((TestGameRound.WinningRound with { Id = 0, UserId = 1 }).ToEntity());
        await db.SaveChangesAsync();

        await svc.DeleteAccountAsync(1);

        (await db.Users.CountAsync()).Should().Be(0);
        (await db.Wallets.CountAsync()).Should().Be(0);
        (await db.GameRounds.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task DeleteAccount_CascadesToWalletAndRounds()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        db.GameRounds.Add((TestGameRound.WinningRound with { Id = 0, UserId = 1 }).ToEntity());
        await db.SaveChangesAsync();

        await svc.DeleteAccountAsync(1);

        (await db.Users.CountAsync()).Should().Be(0);
        (await db.Wallets.CountAsync()).Should().Be(0);
        (await db.GameRounds.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task UpdateProfile_DoesNotAllowEmailChange()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        var originalEmail = TestUser.Valid.Email;

        await svc.UpdateProfileAsync(1, new UpdateProfileRequest
        {
            FirstName = "Changed"
        });

        var user = await db.Users.FindAsync(1);
        user!.Email.Should().Be(originalEmail);
    }

    [Fact]
    public async Task UpdateProfile_DoesNotAllowNicknameChange()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        var originalNick = TestUser.Valid.NickName;

        await svc.UpdateProfileAsync(1, new UpdateProfileRequest
        {
            FirstName = "Changed"
        });

        var user = await db.Users.FindAsync(1);
        user!.NickName.Should().Be(originalNick);
    }

    [Fact]
    public async Task SetBlocked_BlockedUserCannotBeFoundAsUnblocked()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        await svc.SetBlockedAsync(1, true);
        var user = await db.Users.FindAsync(1);

        user!.IsBlocked.Should().BeTrue();
    } 

    [Fact]
    public async Task UpdateProfile_LeavesEmailUntouched()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        await svc.UpdateProfileAsync(1, new UpdateProfileRequest { FirstName = "New" });

        var user = await db.Users.FindAsync(1);
        user!.Email.Should().Be(TestUser.Valid.Email);
    }

    [Fact]
    public async Task UpdateProfile_LeavesNicknameUntouched()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        await svc.UpdateProfileAsync(1, new UpdateProfileRequest { FirstName = "New" });

        var user = await db.Users.FindAsync(1);
        user!.NickName.Should().Be(TestUser.Valid.NickName);
    }

    [Fact]
    public async Task GetProfile_DoesNotIncludeWalletData()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.GetProfileAsync(1);

        typeof(OnlineCasino.DTOs.Users.UserProfileDto)
            .GetProperty("Balance")
            .Should().BeNull();
    }

    [Fact]
    public async Task GetAll_ReflectsUpdatedBalanceAfterWalletMutation()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        await db.SaveChangesAsync();

        var wallet = new WalletService(db);
        await wallet.DepositAsync(1, 500m);

        var result = await svc.GetAllAsync();

        result.Data.Single().Balance.Should().Be(1500m);
    }
}
