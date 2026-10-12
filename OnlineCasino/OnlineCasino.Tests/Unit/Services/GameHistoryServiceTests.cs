namespace OnlineCasino.Tests.Unit.Services;

using Xunit;
using FluentAssertions;

using OnlineCasino.Services; 
using OnlineCasino.Tests.Common.Fixtures;
using OnlineCasino.Tests.Common.TestData;
 
public sealed class GameHistoryServiceTests
{
    private static async Task<OnlineCasino.Data.AppDbContext> ArrangeWithRoundsAsync()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        db.Users.Add(TestUser.Valid.ToEntity());

        var round1 = TestGameRound.WinningRound with
        {
            Id = 0,
            UserId = 1,
            CreatedAt = TestClock.FixedNow.AddMinutes(-10)
        };
        var round2 = TestGameRound.LosingRound with
        {
            Id = 0,
            UserId = 1,
            CreatedAt = TestClock.FixedNow.AddMinutes(-5)
        };
        var round3 = TestGameRound.PushRound with
        {
            Id = 0,
            UserId = 1,
            CreatedAt = TestClock.FixedNow
        };

        db.GameRounds.AddRange(
            round1.ToEntity(),
            round2.ToEntity(),
            round3.ToEntity());

        await db.SaveChangesAsync();
        return db;
    }

    [Fact]
    public async Task Get_ReturnsPagedDescending()
    {
        var db = await ArrangeWithRoundsAsync();
        var svc = new GameHistoryService(db);

        var result = await svc.GetAsync(1, null, 1, 20);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(3);
        result.Data.Items[0].Result.Should().Be("Push");
        result.Data.Total.Should().Be(3);
    }

    [Fact]
    public async Task Get_FilteredByGameCode_ReturnsOnlyMatches()
    {
        var db = await ArrangeWithRoundsAsync();
        var svc = new GameHistoryService(db);

        var result = await svc.GetAsync(1, "slots", 1, 20);

        result.Data!.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task Get_FilteredByUnknownGameCode_ReturnsEmpty()
    {
        var db = await ArrangeWithRoundsAsync();
        var svc = new GameHistoryService(db);

        var result = await svc.GetAsync(1, "nonexistent", 1, 20);

        result.Data!.Items.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0, 0, 1, 20)]
    [InlineData(-1, 500, 1, 20)]
    public async Task Get_NormalizesPaging(int page, int size, int expPage, int expSize)
    {
        var db = await ArrangeWithRoundsAsync();
        var svc = new GameHistoryService(db);

        var result = await svc.GetAsync(1, null, page, size);

        result.Data!.Page.Should().Be(expPage);
        result.Data.PageSize.Should().Be(expSize);
    }

    [Fact]
    public async Task GetByRoundId_OwnedRound_ReturnsDto()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        var round = TestGameRound.WinningRound with { Id = 0, UserId = 1 };
        db.GameRounds.Add(round.ToEntity());
        await db.SaveChangesAsync();
        var svc = new GameHistoryService(db);

        var result = await svc.GetByRoundIdAsync(1, round.PublicId);

        result.IsSuccess.Should().BeTrue();
        result.Data!.RoundId.Should().Be(round.PublicId);
    }

    [Fact]
    public async Task GetByRoundId_OtherUsersRound_Returns404()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        var round = TestGameRound.WinningRound with { Id = 0, UserId = 1 };
        db.GameRounds.Add(round.ToEntity());
        await db.SaveChangesAsync();
        var svc = new GameHistoryService(db);

        var result = await svc.GetByRoundIdAsync(999, round.PublicId);

        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetByRoundId_Missing_Returns404()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        var svc = new GameHistoryService(db);

        var result = await svc.GetByRoundIdAsync(1, "missing");

        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Get_OnlyOwnRoundsReturned()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        db.Users.Add((TestUser.Valid with { Id = 2, Email = "u2@t.com", NickName = "u2" }).ToEntity());
        await db.SaveChangesAsync();

        db.GameRounds.AddRange(
            (TestGameRound.WinningRound with { Id = 0, UserId = 1 }).ToEntity(),
            (TestGameRound.WinningRound with { Id = 0, UserId = 2 }).ToEntity());
        await db.SaveChangesAsync();

        var svc = new GameHistoryService(db);
        var result = await svc.GetAsync(1, null, 1, 20);

        result.Data!.Items.Should().HaveCount(1);
        result.Data.Total.Should().Be(1);
    }

    [Fact]
    public async Task Get_RoundWithoutResult_ReturnsNullResult()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        db.GameRounds.Add((TestGameRound.UnresolvedRound with { Id = 0, UserId = 1 }).ToEntity());
        await db.SaveChangesAsync();

        var svc = new GameHistoryService(db);
        var result = await svc.GetAsync(1, null, 1, 20);

        result.Data!.Items[0].Result.Should().BeNull();
    }

    [Fact]
    public async Task Get_SlotFilter_OnlySlots()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        db.GameRounds.AddRange(
            (TestGameRound.WinningRound with { Id = 0, UserId = 1, GameId = TestIds.SlotsGameId }).ToEntity(),
            (TestGameRound.WinningRound with { Id = 0, UserId = 1, GameId = TestIds.RouletteGameId }).ToEntity());
        await db.SaveChangesAsync();

        var svc = new GameHistoryService(db);
        var result = await svc.GetAsync(1, "slots", 1, 20);

        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items[0].Game.Should().Be("slots");
    }

    [Fact]
    public async Task Get_Pagination_ReturnsCorrectPage()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        for (int i = 0; i < 10; i++)
        {
            db.GameRounds.Add((TestGameRound.WinningRound with
            {
                Id = 0,
                UserId = 1,
                CreatedAt = TestClock.FixedNow.AddMinutes(-i)
            }).ToEntity());
        }
        await db.SaveChangesAsync();

        var svc = new GameHistoryService(db);
        var result = await svc.GetAsync(1, null, 2, 3);

        result.Data!.Items.Should().HaveCount(3);
        result.Data.Page.Should().Be(2);
        result.Data.PageSize.Should().Be(3);
        result.Data.Total.Should().Be(10);
    }

    [Fact]
    public async Task GetByRoundId_ReturnsAllFields()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        var round = TestGameRound.WinningRound with { Id = 0, UserId = 1 };
        db.GameRounds.Add(round.ToEntity());
        await db.SaveChangesAsync();

        var svc = new GameHistoryService(db);
        var result = await svc.GetByRoundIdAsync(1, round.PublicId);

        result.Data!.Game.Should().Be("slots");
        result.Data.BetAmount.Should().Be(10m);
        result.Data.WinAmount.Should().Be(20m);
        result.Data.Result.Should().Be("Win");
    }

    [Fact]
    public async Task GetByRoundId_CaseSensitive()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        var round = TestGameRound.WinningRound with { Id = 0, UserId = 1 };
        db.GameRounds.Add(round.ToEntity());
        await db.SaveChangesAsync();

        var svc = new GameHistoryService(db);
        var result = await svc.GetByRoundIdAsync(1, round.PublicId.ToUpperInvariant());

        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Get_Page3Size4_ReturnsCorrectSlice()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        for (int i = 0; i < 10; i++)
        {
            db.GameRounds.Add((TestGameRound.WinningRound with
            {
                Id = 0,
                UserId = 1,
                CreatedAt = TestClock.FixedNow.AddMinutes(-i)
            }).ToEntity());
        }
        await db.SaveChangesAsync();
        var svc = new GameHistoryService(db);

        var result = await svc.GetAsync(1, null, 3, 4);

        result.Data!.Items.Should().HaveCount(2);
        result.Data.Total.Should().Be(10);
        result.Data.Page.Should().Be(3);
    }

    [Fact]
    public async Task Get_Page3_Size4_ReturnsCorrectSlice()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        for (int i = 0; i < 10; i++)
        {
            db.GameRounds.Add((TestGameRound.WinningRound with
            {
                Id = 0,
                UserId = 1,
                CreatedAt = TestClock.FixedNow.AddMinutes(-i)
            }).ToEntity());
        }
        await db.SaveChangesAsync();
        var svc = new GameHistoryService(db);

        var result = await svc.GetAsync(1, null, 3, 4);

        result.Data!.Items.Should().HaveCount(2);
        result.Data.Total.Should().Be(10);
        result.Data.Page.Should().Be(3);
    }

    [Fact]
    public async Task Get_RoundsAreOrderedByCreatedAtDescending()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        db.GameRounds.AddRange(
            (TestGameRound.WinningRound with { Id = 0, UserId = 1, CreatedAt = TestClock.FixedNow.AddMinutes(-10) }).ToEntity(),
            (TestGameRound.WinningRound with { Id = 0, UserId = 1, CreatedAt = TestClock.FixedNow.AddMinutes(-5) }).ToEntity(),
            (TestGameRound.WinningRound with { Id = 0, UserId = 1, CreatedAt = TestClock.FixedNow }).ToEntity());
        await db.SaveChangesAsync();
        var svc = new GameHistoryService(db);

        var result = await svc.GetAsync(1, null, 1, 20);

        result.Data!.Items.Select(i => i.CreatedAt).Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task GetByRoundId_ReturnsCorrectAmountsNotSwapped()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        var round = TestGameRound.WinningRound with
        {
            Id = 0,
            UserId = 1,
            BetAmount = 33m,
            WinAmount = 77m
        };
        db.GameRounds.Add(round.ToEntity());
        await db.SaveChangesAsync();
        var svc = new GameHistoryService(db);

        var result = await svc.GetByRoundIdAsync(1, round.PublicId);

        result.Data!.BetAmount.Should().Be(33m);
        result.Data.WinAmount.Should().Be(77m);
    }

    [Fact]
    public async Task Get_TotalEqualsFullFilteredSet_NotPageSize()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        db.Users.Add(TestUser.Valid.ToEntity());
        for (int i = 0; i < 25; i++)
        {
            db.GameRounds.Add((TestGameRound.WinningRound with { Id = 0, UserId = 1 }).ToEntity());
        }
        await db.SaveChangesAsync();
        var svc = new GameHistoryService(db);

        var result = await svc.GetAsync(1, null, 1, 10);

        result.Data!.Total.Should().Be(25);
        result.Data.Items.Should().HaveCount(10);
    }
}
