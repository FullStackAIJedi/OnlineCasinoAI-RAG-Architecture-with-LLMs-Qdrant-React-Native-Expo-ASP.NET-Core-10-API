namespace OnlineCasino.Tests.Unit.Services;

using Microsoft.EntityFrameworkCore;

using Xunit;
using FluentAssertions;

using OnlineCasino.Data;
using OnlineCasino.DTOs.Roulette;
using OnlineCasino.Services;

using OnlineCasino.Tests.Common.Fixtures;
using OnlineCasino.Tests.Common.TestData;
 
public sealed class RouletteServiceTests
{
    private static async Task<(AppDbContext db, RouletteService svc)> ArrangeAsync(decimal balance = 1000m)
    {
        var db = await TestDbFactory.CreateSeededAsync();
        db.Users.Add(TestUser.WithBalance(balance).ToEntity());
        await db.SaveChangesAsync();
        var wallet = new WalletService(db);
        return (db, new RouletteService(db, wallet));
    }

    [Fact]
    public async Task GetCurrentBets_Empty_ReturnsEmptyList()
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.GetCurrentBetsAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task PlaceBet_NonPositive_Returns400()
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "red", Amount = 0 });

        result.Error!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task PlaceBet_UnknownGame_Returns404()
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.PlaceBetAsync(1, "unknown",
            new PlaceBetRequest { Type = "red", Amount = 10 });

        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task PlaceBet_InvalidBetType_Returns400()
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "purple", Amount = 10 });

        result.Error!.StatusCode.Should().Be(400);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(38)]
    public async Task PlaceBet_NumberOutOfRange_Returns400(int number)
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "number", Value = number, Amount = 10 });

        result.Error!.StatusCode.Should().Be(400);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public async Task PlaceBet_DozenOutOfRange_Returns400(int dozen)
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "dozen", Value = dozen, Amount = 10 });

        result.Error!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task PlaceBet_InsufficientBalance_Returns400()
    {
        var (_, svc) = await ArrangeAsync(5m);

        var result = await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "red", Amount = 100 });

        result.Error!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task PlaceBet_Valid_CreatesBet()
    {
        var (db, svc) = await ArrangeAsync(100m);

        var result = await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "red", Amount = 10 });

        result.IsSuccess.Should().BeTrue();
        result.Data!.Type.Should().Be("red");
        result.Data.Amount.Should().Be(10);

        var stored = await svc.GetCurrentBetsAsync(1);
        stored.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task Spin_NoBets_Returns400()
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.SpinAsync(1, TestCodes.RouletteGame,
            new SpinRouletteRequest { ClientRoundId = "r1" });

        result.Error!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Spin_UnknownGame_Returns404()
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.SpinAsync(1, "unknown",
            new SpinRouletteRequest { ClientRoundId = "r1" });

        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Spin_Valid_DebitsCreditsAndClearsBets()
    {
        var (db, svc) = await ArrangeAsync(100m);
        await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "red", Amount = 10 });
        await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "black", Amount = 5 });

        var result = await svc.SpinAsync(1, TestCodes.RouletteGame,
            new SpinRouletteRequest { ClientRoundId = "round-1" });

        result.IsSuccess.Should().BeTrue();
        result.Data!.TotalBet.Should().Be(15m);
        result.Data.WinningNumber.Should().BeInRange(0, 37);
        result.Data.WinningColor.Should().BeOneOf("red", "black", "green");

        (await svc.GetCurrentBetsAsync(1)).Data.Should().BeEmpty();
        db.GameRounds.Count().Should().Be(1);
    }

    [Fact]
    public async Task GetCurrentBets_ReturnsOrderedByCreatedAt()
    {
        var (db, svc) = await ArrangeAsync();

        var red = TestRouletteBet.Red with
        {
            Id = null,
            UserId = 1,
            CreatedAt = TestClock.FixedNow.AddMinutes(-2)
        };
        var black = TestRouletteBet.Black with
        {
            Id = null,
            UserId = 1,
            CreatedAt = TestClock.FixedNow
        };

        db.RouletteBets.AddRange(red.ToEntity(), black.ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.GetCurrentBetsAsync(1);

        result.Data.Should().HaveCount(2);
        result.Data[0].Type.Should().Be("red");
    }

    [Fact]
    public async Task GetCurrentBets_OnlyOwnBetsReturned()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add((TestUser.Valid with { Id = 2, Email = "u2@t.com", NickName = "u2" }).ToEntity());
        await db.SaveChangesAsync();

        db.RouletteBets.AddRange(
            (TestRouletteBet.Red with { Id = null, UserId = 1 }).ToEntity(),
            (TestRouletteBet.Red with { Id = null, UserId = 2 }).ToEntity());
        await db.SaveChangesAsync();

        var result = await svc.GetCurrentBetsAsync(1);

        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task PlaceBet_MultipleTypes_AllStored()
    {
        var (db, svc) = await ArrangeAsync(1000m);

        await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "red", Amount = 10 });
        await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "black", Amount = 20 });
        await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "even", Amount = 5 });

        (await svc.GetCurrentBetsAsync(1)).Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task PlaceBet_TotalPendingCannotExceedBalance()
    {
        var (_, svc) = await ArrangeAsync(50m);

        await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "red", Amount = 40 });

        var result = await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "black", Amount = 20 });

        result.Error!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task PlaceBet_NumberValue0_Succeeds()
    {
        var (_, svc) = await ArrangeAsync(100m);

        var result = await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "number", Value = 0, Amount = 10 });

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task PlaceBet_NumberValue37_Succeeds()
    {
        var (_, svc) = await ArrangeAsync(100m);

        var result = await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "number", Value = 37, Amount = 10 });

        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task PlaceBet_ValidDozen_Succeeds(int dozen)
    {
        var (_, svc) = await ArrangeAsync(100m);

        var result = await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "dozen", Value = dozen, Amount = 10 });

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Spin_ClearsAllBetsAfterResolve()
    {
        var (db, svc) = await ArrangeAsync(1000m);
        await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "red", Amount = 5 });
        await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "black", Amount = 5 });

        await svc.SpinAsync(1, TestCodes.RouletteGame,
            new SpinRouletteRequest { ClientRoundId = "r" });

        (await db.RouletteBets.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Spin_WritesGameRound()
    {
        var (db, svc) = await ArrangeAsync(100m);
        await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "red", Amount = 10 });

        var result = await svc.SpinAsync(1, TestCodes.RouletteGame,
            new SpinRouletteRequest { ClientRoundId = "r" });

        var round = db.GameRounds.Single();
        round.PublicId.Should().Be(result.Data!.RoundId);
        round.BetAmount.Should().Be(10m);
        round.WinAmount.Should().Be(result.Data.TotalWin);
    }

    [Fact]
    public async Task Spin_ResultIsWinLoseOrPush()
    {
        var (_, svc) = await ArrangeAsync(100m);
        await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "red", Amount = 10 });

        var result = await svc.SpinAsync(1, TestCodes.RouletteGame,
            new SpinRouletteRequest { ClientRoundId = "r" });

        result.Data!.Result.Should().BeOneOf("Win", "Lose", "Push");
    }

    [Fact]
    public async Task Spin_GreenNumber_0Or37()
    {
        var (_, svc) = await ArrangeAsync(100m);
        await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "red", Amount = 10 });

        var result = await svc.SpinAsync(1, TestCodes.RouletteGame,
            new SpinRouletteRequest { ClientRoundId = "r" });

        if (result.Data!.WinningNumber == 0 || result.Data.WinningNumber == 37)
            result.Data.WinningColor.Should().Be("green");
    }

    [Fact]
    public async Task Spin_TotalWinGreaterOrEqualZero()
    {
        var (_, svc) = await ArrangeAsync(100m);
        await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "red", Amount = 10 });

        var result = await svc.SpinAsync(1, TestCodes.RouletteGame,
            new SpinRouletteRequest { ClientRoundId = "r" });

        result.Data!.TotalWin.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task Spin_ZeroWinning_DoesNotPayEvenOddLowHighBets()
    {
        var (db, svc) = await ArrangeAsync(1000m);
        await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "even", Amount = 10 });
        await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "low", Amount = 10 });

        RouletteSpinResponse? result = null;
        for (int i = 0; i < 200; i++)
        {
            await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
                new PlaceBetRequest { Type = "even", Amount = 10 });
            await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
                new PlaceBetRequest { Type = "low", Amount = 10 });
            result = (await svc.SpinAsync(1, TestCodes.RouletteGame,
                new SpinRouletteRequest { ClientRoundId = Guid.NewGuid().ToString("N") })).Data;
            if (result!.WinningNumber == 0) break;
        }

        if (result!.WinningNumber == 0)
        {
            result.TotalWin.Should().Be(0);
        }
    }

    [Fact]
    public async Task Spin_AfterSpin_RouletteBetsTableEmpty()
    {
        var (db, svc) = await ArrangeAsync(1000m);
        await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "red", Amount = 10 });
        await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "black", Amount = 20 });

        await svc.SpinAsync(1, TestCodes.RouletteGame,
            new SpinRouletteRequest { ClientRoundId = "r" });

        (await db.RouletteBets.CountAsync()).Should().Be(0);
    }
     
    [Fact]
    public async Task Spin_BetTransactionAmount_EqualsSumOfPendingBets()
    {
        var (db, svc) = await ArrangeAsync(1000m);
        await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "red", Amount = 10 });
        await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "black", Amount = 20 });
        await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "even", Amount = 5 });

        await svc.SpinAsync(1, TestCodes.RouletteGame,
            new SpinRouletteRequest { ClientRoundId = "r" });

        var betTx = await db.Transactions
            .SingleAsync(t => t.TransactionTypeId == TestIds.BetTypeId);

        betTx.Amount.Should().Be(-35m);
    }

    [Fact]
    public async Task Spin_GameRoundDetailsJson_ContainsWinningNumberAndColor()
    {
        var (db, svc) = await ArrangeAsync(100m);
        await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "red", Amount = 10 });

        var result = await svc.SpinAsync(1, TestCodes.RouletteGame,
            new SpinRouletteRequest { ClientRoundId = "r" });

        var round = await db.GameRounds.SingleAsync();
        round.DetailsJson.Should().Contain(result.Data!.WinningNumber.ToString());
        round.DetailsJson.Should().Contain(result.Data.WinningColor);
    }

    [Fact]
    public async Task Spin_GameRoundBetAmount_EqualsTotalBet()
    {
        var (db, svc) = await ArrangeAsync(100m);
        await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "red", Amount = 15 });

        var result = await svc.SpinAsync(1, TestCodes.RouletteGame,
            new SpinRouletteRequest { ClientRoundId = "r" });

        var round = await db.GameRounds.SingleAsync();
        round.BetAmount.Should().Be(result.Data!.TotalBet);
        round.WinAmount.Should().Be(result.Data.TotalWin);
    }

    [Fact]
    public async Task PlaceBet_Number_OnlyWinsOnExactNumber()
    {
        var (_, svc) = await ArrangeAsync(10_000m);

        for (int i = 0; i < 300; i++)
        {
            await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
                new PlaceBetRequest { Type = "number", Value = 7, Amount = 10 });

            var result = await svc.SpinAsync(1, TestCodes.RouletteGame,
                new SpinRouletteRequest { ClientRoundId = $"r{i}" });

            if (result.Data!.WinningNumber == 7)
                result.Data.TotalWin.Should().Be(360m);
            else
                result.Data.TotalWin.Should().Be(0m);
        }
    }

    [Fact]
    public async Task PlaceBet_Dozen_OnlyWinsOnItsRange()
    {
        var (_, svc) = await ArrangeAsync(10_000m);

        for (int i = 0; i < 300; i++)
        {
            await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
                new PlaceBetRequest { Type = "dozen", Value = 1, Amount = 10 });

            var result = await svc.SpinAsync(1, TestCodes.RouletteGame,
                new SpinRouletteRequest { ClientRoundId = $"r{i}" });

            var n = result.Data!.WinningNumber;
            if (n >= 1 && n <= 12)
                result.Data.TotalWin.Should().Be(30m);
            else
                result.Data.TotalWin.Should().Be(0m);
        }
    }

    [Fact]
    public async Task PlaceBet_Number37_IsDoubleZero_OnlyWinsOn37()
    {
        var (_, svc) = await ArrangeAsync(10_000m);

        for (int i = 0; i < 300; i++)
        {
            await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
                new PlaceBetRequest { Type = "number", Value = 37, Amount = 10 });

            var result = await svc.SpinAsync(1, TestCodes.RouletteGame,
                new SpinRouletteRequest { ClientRoundId = $"r{i}" });

            if (result.Data!.WinningNumber == 37)
                result.Data.TotalWin.Should().Be(360m);
            else
                result.Data.TotalWin.Should().Be(0m);
        }
    }

    [Fact]
    public async Task Spin_ResultLabel_MatchesTotalWinVsBet()
    {
        var (_, svc) = await ArrangeAsync(10_000m);

        for (int i = 0; i < 50; i++)
        {
            await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
                new PlaceBetRequest { Type = "red", Amount = 10 });

            var d = (await svc.SpinAsync(1, TestCodes.RouletteGame,
                new SpinRouletteRequest { ClientRoundId = $"r{i}" })).Data!;

            if (d.TotalWin > d.TotalBet) d.Result.Should().Be("Win");
            else if (d.TotalWin == d.TotalBet) d.Result.Should().Be("Push");
            else d.Result.Should().Be("Lose");
        }
    }

    [Fact]
    public async Task Spin_PendingBetsOfOtherUsers_NotCleared()
    {
        var (db, svc) = await ArrangeAsync(1000m);
        db.Users.Add((TestUser.Valid with { Id = 2, Email = "u2@t.com", NickName = "u2" }).ToEntity());
        await db.SaveChangesAsync();

        await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "red", Amount = 10 });
        await svc.PlaceBetAsync(2, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "black", Amount = 20 });

        await svc.SpinAsync(1, TestCodes.RouletteGame,
            new SpinRouletteRequest { ClientRoundId = "r" });

        var remaining = await db.RouletteBets.ToListAsync();
        remaining.Should().HaveCount(1);
        remaining[0].UserId.Should().Be(2);
    }

    [Fact]
    public async Task PlaceBet_SameUserMultipleBets_AllVisibleInGetCurrentBets()
    {
        var (_, svc) = await ArrangeAsync(1000m);

        await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "red", Amount = 10 });
        await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "black", Amount = 20 });
        await svc.PlaceBetAsync(1, TestCodes.RouletteGame,
            new PlaceBetRequest { Type = "even", Amount = 30 });

        var result = await svc.GetCurrentBetsAsync(1);

        result.Data.Should().HaveCount(3);
        result.Data.Sum(b => b.Amount).Should().Be(60m);
    }
}
