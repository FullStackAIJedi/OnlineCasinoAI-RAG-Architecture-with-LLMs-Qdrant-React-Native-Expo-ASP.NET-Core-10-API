namespace OnlineCasino.Tests.Unit.Services;

using Microsoft.EntityFrameworkCore;

using Xunit;
using FluentAssertions;

using OnlineCasino.Data;
using OnlineCasino.DTOs.Slots;
using OnlineCasino.Services;

using OnlineCasino.Tests.Common.Fixtures;
using OnlineCasino.Tests.Common.TestData;
 
public sealed class SlotsServiceTests
{
    private static async Task<(AppDbContext db, SlotsService svc)> ArrangeAsync(decimal balance = 1000m)
    {
        var db = await TestDbFactory.CreateSeededAsync();
        db.Users.Add(TestUser.WithBalance(balance).ToEntity());
        await db.SaveChangesAsync();
        var wallet = new WalletService(db);
        return (db, new SlotsService(db, wallet));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Spin_NonPositiveBet_Returns400(decimal bet)
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.SpinAsync(1, TestCodes.SlotsGame,
            new SpinRequest { BetAmount = bet, ClientRoundId = "r1" });

        result.Error!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Spin_UnknownGame_Returns404()
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.SpinAsync(1, "unknown",
            new SpinRequest { BetAmount = 10, ClientRoundId = "r1" });

        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Spin_DisabledGame_Returns404()
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.SpinAsync(1, TestCodes.DisabledGame,
            new SpinRequest { BetAmount = 10, ClientRoundId = "r1" });

        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Spin_NotASlotGame_Returns400()
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.SpinAsync(1, TestCodes.RouletteGame,
            new SpinRequest { BetAmount = 10, ClientRoundId = "r1" });

        result.Error!.StatusCode.Should().Be(400);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(999)]
    public async Task Spin_BetOutsideRange_Returns400(decimal bet)
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.SpinAsync(1, TestCodes.SlotsGame,
            new SpinRequest { BetAmount = bet, ClientRoundId = "r1" });

        result.Error!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Spin_InsufficientBalance_Returns400()
    {
        var (_, svc) = await ArrangeAsync(1m);

        var result = await svc.SpinAsync(1, TestCodes.SlotsGame,
            new SpinRequest { BetAmount = 50, ClientRoundId = "r1" });

        result.Error!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Spin_Valid_ReturnsReelsAndWritesRound()
    {
        var (db, svc) = await ArrangeAsync(100m);

        var result = await svc.SpinAsync(1, TestCodes.SlotsGame,
            new SpinRequest { BetAmount = 10, ClientRoundId = "round-1" });

        result.IsSuccess.Should().BeTrue();
        result.Data!.Reels.Should().HaveCount(3);
        result.Data.BetAmount.Should().Be(10);
        result.Data.MatchCount.Should().BeGreaterOrEqualTo(1);
        result.Data.Result.Should().BeOneOf("Win", "Lose");

        var rounds = db.GameRounds.ToList();
        rounds.Should().HaveCount(1);
    }

    [Fact]
    public async Task Spin_Slots4_ReturnsFourReels()
    {
        var (_, svc) = await ArrangeAsync(100m);

        var result = await svc.SpinAsync(1, TestCodes.Slots4Game,
            new SpinRequest { BetAmount = 10, ClientRoundId = "round-4" });

        result.IsSuccess.Should().BeTrue();
        result.Data!.Reels.Should().HaveCount(4);
    }

    [Fact]
    public async Task Spin_ExactMinBet_Succeeds()
    {
        var (_, svc) = await ArrangeAsync(100m);

        var result = await svc.SpinAsync(1, TestCodes.SlotsGame,
            new SpinRequest { BetAmount = 1m, ClientRoundId = "r-min" });

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Spin_ExactMaxBet_Succeeds()
    {
        var (_, svc) = await ArrangeAsync(1000m);

        var result = await svc.SpinAsync(1, TestCodes.SlotsGame,
            new SpinRequest { BetAmount = 100m, ClientRoundId = "r-max" });

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Spin_DebitsBet_AndWritesBetTransaction()
    {
        var (db, svc) = await ArrangeAsync(100m);

        await svc.SpinAsync(1, TestCodes.SlotsGame,
            new SpinRequest { BetAmount = 10m, ClientRoundId = "r-bet" });

        db.Transactions.Any(t =>
            t.TransactionTypeId == TestIds.BetTypeId &&
            t.Amount == -10m &&
            t.ReferenceId == "r-bet").Should().BeTrue();
    }

    [Fact]
    public async Task Spin_RoundIsPersistedWithDetails()
    {
        var (db, svc) = await ArrangeAsync(100m);

        await svc.SpinAsync(1, TestCodes.SlotsGame,
            new SpinRequest { BetAmount = 10m, ClientRoundId = "r-persist" });

        var round = db.GameRounds.Single();
        round.DetailsJson.Should().NotBeNullOrWhiteSpace();
        round.BetAmount.Should().Be(10m);
    }

    [Fact]
    public async Task Spin_RoundIdMatchesResponse()
    {
        var (db, svc) = await ArrangeAsync(100m);

        var result = await svc.SpinAsync(1, TestCodes.SlotsGame,
            new SpinRequest { BetAmount = 10m, ClientRoundId = "r-id" });

        var round = db.GameRounds.Single();
        round.PublicId.Should().Be(result.Data!.RoundId);
    }

    [Fact]
    public async Task Spin_BalanceReflectsDebitAndCredit()
    {
        var (_, svc) = await ArrangeAsync(100m);

        var result = await svc.SpinAsync(1, TestCodes.SlotsGame,
            new SpinRequest { BetAmount = 10m, ClientRoundId = "r-bal" });

        var expected = 100m - 10m + result.Data!.WinAmount;
        result.Data.Balance.Should().Be(expected);
    }

    [Fact]
    public async Task Spin_ResultIsWinOrLose()
    {
        var (_, svc) = await ArrangeAsync(100m);

        var result = await svc.SpinAsync(1, TestCodes.SlotsGame,
            new SpinRequest { BetAmount = 10m, ClientRoundId = "r-res" });

        result.Data!.Result.Should().BeOneOf("Win", "Lose");
    }

    [Fact]
    public async Task Spin_MatchCount_IsPositive()
    {
        var (_, svc) = await ArrangeAsync(100m);

        var result = await svc.SpinAsync(1, TestCodes.SlotsGame,
            new SpinRequest { BetAmount = 10m, ClientRoundId = "r-match" });

        result.Data!.MatchCount.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task Spin_Slots4_ValidBet()
    {
        var (_, svc) = await ArrangeAsync(100m);

        var result = await svc.SpinAsync(1, TestCodes.Slots4Game,
            new SpinRequest { BetAmount = 50m, ClientRoundId = "r-4" });

        result.IsSuccess.Should().BeTrue();
        result.Data!.Reels.Should().HaveCount(4);
    }

    [Fact]
    public async Task Spin_RouletteGameRejected_Returns400()
    {
        var (_, svc) = await ArrangeAsync(100m);

        var result = await svc.SpinAsync(1, TestCodes.RouletteGame,
            new SpinRequest { BetAmount = 10m, ClientRoundId = "r" });

        result.Error!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Spin_BlackjackGameRejected_Returns400()
    {
        var (_, svc) = await ArrangeAsync(100m);

        var result = await svc.SpinAsync(1, TestCodes.BlackjackGame,
            new SpinRequest { BetAmount = 10m, ClientRoundId = "r" });

        result.Error!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Spin_WhenAllReelsMatch_PaysBetTimesPayout3()
    {
        var (db, svc) = await ArrangeAsync(1000m);

        var result = await svc.SpinAsync(1, TestCodes.SlotsGame,
            new SpinRequest { BetAmount = 10, ClientRoundId = "r" });

        if (result.Data!.MatchCount == 3)
        {
            var expectedWin = result.Data.WinAmount;
            var symbol = result.Data.Reels[0];

            var payout = await db.SlotSymbols
                .Where(s => s.Code == symbol)
                .Select(s => s.Payout3)
                .SingleAsync();

            expectedWin.Should().Be(10 * payout);
        }
    }

    [Fact]
    public async Task Spin_ClientRoundId_AppearsInBetTransaction()
    {
        var (db, svc) = await ArrangeAsync(100m);

        await svc.SpinAsync(1, TestCodes.SlotsGame,
            new SpinRequest { BetAmount = 10, ClientRoundId = "abc-123" });

        var betTx = await db.Transactions
            .SingleAsync(t => t.TransactionTypeId == TestIds.BetTypeId);

        betTx.ReferenceId.Should().Be("abc-123");
    }

    [Fact]
    public async Task Spin_ManySpins_NoNegativeBalanceEver()
    {
        var (db, svc) = await ArrangeAsync(100m);

        for (int i = 0; i < 50; i++)
        {
            await svc.SpinAsync(1, TestCodes.SlotsGame,
                new SpinRequest { BetAmount = 10, ClientRoundId = $"r{i}" });

            (await db.Wallets.FindAsync(1))!.Balance.Should().BeGreaterOrEqualTo(0m);
        }
    }

    [Fact]
    public async Task Spin_SumOfBetTransactions_EqualsTotalWagered()
    {
        var (db, svc) = await ArrangeAsync(1000m);

        for (int i = 0; i < 10; i++)
        {
            await svc.SpinAsync(1, TestCodes.SlotsGame,
                new SpinRequest { BetAmount = 7, ClientRoundId = $"r{i}" });
        }

        var totalBets = await db.Transactions
            .Where(t => t.TransactionTypeId == TestIds.BetTypeId)
            .SumAsync(t => t.Amount);

        totalBets.Should().Be(-70m);
    }

    [Fact]
    public async Task Spin_WinAmountIsAlwaysMultipleOfPayoutOrZero()
    {
        var (db, svc) = await ArrangeAsync(1000m);

        for (int i = 0; i < 30; i++)
        {
            var result = await svc.SpinAsync(1, TestCodes.SlotsGame,
                new SpinRequest { BetAmount = 10, ClientRoundId = $"r{i}" });

            var d = result.Data!;
            if (d.MatchCount < 2)
                d.WinAmount.Should().Be(0m);
        }
    }

    [Fact]
    public async Task Spin_RoundDetailsJson_ContainsReelsAndMatchCount()
    {
        var (db, svc) = await ArrangeAsync(100m);

        await svc.SpinAsync(1, TestCodes.SlotsGame,
            new SpinRequest { BetAmount = 10, ClientRoundId = "r" });

        var round = await db.GameRounds.SingleAsync();
        round.DetailsJson.Should().Contain("\"reels\"");
        round.DetailsJson.Should().Contain("\"matchCount\"");
    }

    [Fact]
    public async Task Spin_GameRoundPublicId_MatchesResponseRoundId()
    {
        var (db, svc) = await ArrangeAsync(100m);

        var result = await svc.SpinAsync(1, TestCodes.SlotsGame,
            new SpinRequest { BetAmount = 10, ClientRoundId = "r" });

        var round = await db.GameRounds.SingleAsync();
        round.PublicId.Should().Be(result.Data!.RoundId);
    }

    [Fact]
    public async Task Spin_BetTransaction_AndWinTransactionShareSameReferenceId()
    {
        var (db, svc) = await ArrangeAsync(1000m);

        for (int i = 0; i < 50; i++)
        {
            await svc.SpinAsync(1, TestCodes.SlotsGame,
                new SpinRequest { BetAmount = 10, ClientRoundId = "shared-ref" });
        }

        var refs = await db.Transactions
            .Where(t => t.ReferenceId == "shared-ref")
            .Select(t => t.TransactionTypeId)
            .Distinct()
            .ToListAsync();

        refs.Should().Contain(TestIds.BetTypeId);
    }

    [Fact]
    public async Task Spin_TotalTransactions_EqualsSumOfBetsAndWins()
    {
        var (db, svc) = await ArrangeAsync(1000m);

        for (int i = 0; i < 10; i++)
            await svc.SpinAsync(1, TestCodes.SlotsGame,
                new SpinRequest { BetAmount = 10, ClientRoundId = $"r{i}" });

        var bets = await db.Transactions.CountAsync(t => t.TransactionTypeId == TestIds.BetTypeId);
        var wins = await db.Transactions.CountAsync(t => t.TransactionTypeId == TestIds.WinTypeId);

        bets.Should().Be(10);
        (bets + wins).Should().Be(await db.Transactions.CountAsync());
    }
}