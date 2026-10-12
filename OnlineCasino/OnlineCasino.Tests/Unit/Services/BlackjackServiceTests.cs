namespace OnlineCasino.Tests.Unit.Services;

using Microsoft.EntityFrameworkCore;

using Xunit;
using FluentAssertions;

using OnlineCasino.Data;
using OnlineCasino.DTOs.Blackjack;
using OnlineCasino.Services;

using OnlineCasino.Tests.Common.Fixtures;
using OnlineCasino.Tests.Common.TestData;
 
public sealed class BlackjackServiceTests
{
    private static async Task<(AppDbContext db, BlackjackService svc)> ArrangeAsync(decimal balance = 1000m)
    {
        var db = await TestDbFactory.CreateSeededAsync();
        db.Users.Add(TestUser.WithBalance(balance).ToEntity());
        await db.SaveChangesAsync();
        var wallet = new WalletService(db);
        return (db, new BlackjackService(db, wallet));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Start_NonPositiveBet_Returns400(decimal bet)
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.StartAsync(1, TestCodes.BlackjackGame,
            new StartBlackjackRequest { BetAmount = bet });

        result.Error!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Start_UnknownGame_Returns404()
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.StartAsync(1, "unknown",
            new StartBlackjackRequest { BetAmount = 10 });

        result.Error!.StatusCode.Should().Be(404);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(9999)]
    public async Task Start_BetOutsideRange_Returns400(decimal bet)
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.StartAsync(1, TestCodes.BlackjackGame,
            new StartBlackjackRequest { BetAmount = bet });

        result.Error!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Start_InsufficientBalance_Returns400()
    {
        var (_, svc) = await ArrangeAsync(1m);

        var result = await svc.StartAsync(1, TestCodes.BlackjackGame,
            new StartBlackjackRequest { BetAmount = 50 });

        result.Error!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Start_Valid_ReturnsTwoCardsForEach()
    {
        var (db, svc) = await ArrangeAsync(100m);

        var result = await svc.StartAsync(1, TestCodes.BlackjackGame,
            new StartBlackjackRequest { BetAmount = 10 });

        result.IsSuccess.Should().BeTrue();
        result.Data!.PlayerHand.Should().HaveCount(2);
        result.Data.DealerHand.Should().HaveCount(2);

        if (result.Data.Status == "PlayerTurn")
            result.Data.DealerHand[1].Should().Be("?");
        else
            result.Data.DealerHand[1].Should().NotBe("?");

        db.BlackjackSessions.Count().Should().Be(1);
    }

    [Fact]
    public async Task Hit_UnknownSession_Returns404()
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.HitAsync(1, "missing");

        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Stand_UnknownSession_Returns404()
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.StandAsync(1, "missing");

        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Double_UnknownSession_Returns404()
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.DoubleAsync(1, "missing");

        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetState_UnknownSession_Returns404()
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.GetStateAsync(1, "missing");

        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetState_ExistingSession_ReturnsState()
    {
        var (_, svc) = await ArrangeAsync(100m);
        var start = await svc.StartAsync(1, TestCodes.BlackjackGame,
            new StartBlackjackRequest { BetAmount = 10 });

        var result = await svc.GetStateAsync(1, start.Data!.SessionId);

        result.IsSuccess.Should().BeTrue();
        result.Data!.SessionId.Should().Be(start.Data.SessionId);
    }

    [Fact]
    public async Task Hit_PlayerTurn_DrawsCard()
    {
        var (_, svc) = await ArrangeAsync(100m);
        var start = await svc.StartAsync(1, TestCodes.BlackjackGame,
            new StartBlackjackRequest { BetAmount = 10 });

        if (start.Data!.Status != "PlayerTurn") return;

        var result = await svc.HitAsync(1, start.Data.SessionId);

        result.IsSuccess.Should().BeTrue();
        result.Data!.PlayerHand.Count.Should().BeGreaterThan(2);
    }

    [Fact]
    public async Task Stand_PlayerTurn_Resolves()
    {
        var (_, svc) = await ArrangeAsync(100m);
        var start = await svc.StartAsync(1, TestCodes.BlackjackGame,
            new StartBlackjackRequest { BetAmount = 10 });

        if (start.Data!.Status != "PlayerTurn") return;

        var result = await svc.StandAsync(1, start.Data.SessionId);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Status.Should().Be("Finished");
        result.Data.Result.Should().BeOneOf("Win", "Lose", "Push");
        result.Data.DealerScore.Should().BeGreaterOrEqualTo(17);
    } 
     

    [Fact]
    public async Task Start_SetsPlayerHandWithTwoCards()
    {
        var (_, svc) = await ArrangeAsync(100m);

        var result = await svc.StartAsync(1, TestCodes.BlackjackGame,
            new StartBlackjackRequest { BetAmount = 10 });

        result.Data!.PlayerHand.Should().HaveCount(2);
        result.Data.PlayerScore.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Start_HidesSecondDealerCard()
    {
        var (_, svc) = await ArrangeAsync(100m);

        var result = await svc.StartAsync(1, TestCodes.BlackjackGame,
            new StartBlackjackRequest { BetAmount = 10 });

        result.Data!.DealerHand[1].Should().Be("?");
    } 

    [Fact]
    public async Task Start_WritesSession()
    {
        var (db, svc) = await ArrangeAsync(100m);

        await svc.StartAsync(1, TestCodes.BlackjackGame,
            new StartBlackjackRequest { BetAmount = 10 });

        (await db.BlackjackSessions.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Start_StatusIsPlayerTurnOrFinished()
    {
        var (_, svc) = await ArrangeAsync(100m);

        var result = await svc.StartAsync(1, TestCodes.BlackjackGame,
            new StartBlackjackRequest { BetAmount = 10 });

        result.Data!.Status.Should().BeOneOf("PlayerTurn", "Finished");
    }

    [Fact]
    public async Task Start_ExactMinBet_Succeeds()
    {
        var (_, svc) = await ArrangeAsync(100m);

        var result = await svc.StartAsync(1, TestCodes.BlackjackGame,
            new StartBlackjackRequest { BetAmount = 5m });

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Start_ExactMaxBet_Succeeds()
    {
        var (_, svc) = await ArrangeAsync(1000m);

        var result = await svc.StartAsync(1, TestCodes.BlackjackGame,
            new StartBlackjackRequest { BetAmount = 500m });

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Stand_DealerDrawsUntilAtLeast17()
    {
        var (_, svc) = await ArrangeAsync(100m);
        var start = await svc.StartAsync(1, TestCodes.BlackjackGame,
            new StartBlackjackRequest { BetAmount = 10 });

        if (start.Data!.Status != "PlayerTurn") return;

        var result = await svc.StandAsync(1, start.Data.SessionId);

        result.Data!.DealerScore.Should().BeGreaterOrEqualTo(17);
    }

    [Fact]
    public async Task Stand_RevealsDealerSecondCard()
    {
        var (_, svc) = await ArrangeAsync(100m);
        var start = await svc.StartAsync(1, TestCodes.BlackjackGame,
            new StartBlackjackRequest { BetAmount = 10 });

        if (start.Data!.Status != "PlayerTurn") return;

        var result = await svc.StandAsync(1, start.Data.SessionId);

        result.Data!.DealerHand[1].Should().NotBe("?");
    }

    [Fact]
    public async Task Stand_WritesGameRound()
    {
        var (db, svc) = await ArrangeAsync(100m);
        var start = await svc.StartAsync(1, TestCodes.BlackjackGame,
            new StartBlackjackRequest { BetAmount = 10 });

        if (start.Data!.Status != "PlayerTurn") return;

        await svc.StandAsync(1, start.Data.SessionId);

        (await db.GameRounds.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Stand_BalanceUpdatedCorrectly()
    {
        var (_, svc) = await ArrangeAsync(100m);
        var start = await svc.StartAsync(1, TestCodes.BlackjackGame,
            new StartBlackjackRequest { BetAmount = 10 });

        if (start.Data!.Status != "PlayerTurn") return;

        var result = await svc.StandAsync(1, start.Data.SessionId);

        var expected = 90m + result.Data!.WinAmount;
        result.Data.Balance.Should().Be(expected);
    }

    [Fact]
    public async Task Hit_AfterHitThenHitAgainUntilBustOr21()
    {
        var (_, svc) = await ArrangeAsync(1000m);
        var start = await svc.StartAsync(1, TestCodes.BlackjackGame,
            new StartBlackjackRequest { BetAmount = 10 });

        if (start.Data!.Status != "PlayerTurn") return;

        var result = await svc.HitAsync(1, start.Data.SessionId);

        result.Data!.PlayerHand.Count.Should().BeGreaterThan(2);
    }
       
    [Fact]
    public async Task GetState_ExistingSession_ReturnsCurrentState()
    {
        var (_, svc) = await ArrangeAsync(100m);
        var start = await svc.StartAsync(1, TestCodes.BlackjackGame,
            new StartBlackjackRequest { BetAmount = 10 });

        var result = await svc.GetStateAsync(1, start.Data!.SessionId);

        result.Data!.BetAmount.Should().Be(10m);
        result.Data.PlayerHand.Count.Should().Be(2);
    }

    [Fact]
    public async Task GetState_OtherUsersSession_Returns404()
    {
        var (db, svc) = await ArrangeAsync(100m);
        db.Users.Add((TestUser.Valid with { Id = 2, Email = "u2@t.com", NickName = "u2" }).ToEntity());
        await db.SaveChangesAsync();

        var start = await svc.StartAsync(1, TestCodes.BlackjackGame,
            new StartBlackjackRequest { BetAmount = 10 });

        var result = await svc.GetStateAsync(2, start.Data!.SessionId);

        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Hit_OtherUsersSession_Returns404()
    {
        var (db, svc) = await ArrangeAsync(100m);
        db.Users.Add((TestUser.Valid with { Id = 2, Email = "u2@t.com", NickName = "u2" }).ToEntity());
        await db.SaveChangesAsync();

        var start = await svc.StartAsync(1, TestCodes.BlackjackGame,
            new StartBlackjackRequest { BetAmount = 10 });

        var result = await svc.HitAsync(2, start.Data!.SessionId);

        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Start_InitialHandScore_NeverExceeds21_UnlessBust()
    {
        var (_, svc) = await ArrangeAsync(1000m);

        for (int i = 0; i < 50; i++)
        {
            var result = await svc.StartAsync(1, TestCodes.BlackjackGame,
                new StartBlackjackRequest { BetAmount = 10 });

            if (result.Data!.Status == "PlayerTurn")
            {
                result.Data.PlayerScore.Should().BeLessOrEqualTo(21);
            }
        }
    }

    [Fact]
    public async Task Start_BlackjackOnFirstTwoCards_Pays2Point5xBet()
    {
        var (_, svc) = await ArrangeAsync(10_000m);

        for (int i = 0; i < 300; i++)
        {
            var result = await svc.StartAsync(1, TestCodes.BlackjackGame,
                new StartBlackjackRequest { BetAmount = 10 });

            if (result.Data!.Status == "Finished" &&
                result.Data.PlayerScore == 21 &&
                result.Data.Result == "Win")
            {
                result.Data.WinAmount.Should().Be(25m);
                return;
            }
        }
    }

    [Fact]
    public async Task Stand_DealerBust_PlayerWins()
    {
        var (_, svc) = await ArrangeAsync(10_000m);

        for (int i = 0; i < 500; i++)
        {
            var start = await svc.StartAsync(1, TestCodes.BlackjackGame,
                new StartBlackjackRequest { BetAmount = 10 });

            if (start.Data!.Status != "PlayerTurn") continue;

            var result = await svc.StandAsync(1, start.Data.SessionId);
            var d = result.Data!;

            if (d.DealerScore > 21 && d.PlayerScore <= 21)
            {
                d.Result.Should().Be("Win");
                d.WinAmount.Should().Be(20m);
                return;
            }
        }
    }

    [Fact]
    public async Task Stand_PlayerBust_ImpossibleAfterStandBecauseHitWouldBeNeeded()
    {
        var (_, svc) = await ArrangeAsync(10_000m);

        for (int i = 0; i < 200; i++)
        {
            var start = await svc.StartAsync(1, TestCodes.BlackjackGame,
                new StartBlackjackRequest { BetAmount = 10 });

            if (start.Data!.Status != "PlayerTurn") continue;

            var result = await svc.StandAsync(1, start.Data.SessionId);
            var d = result.Data!;

            d.PlayerScore.Should().BeLessOrEqualTo(21);
        }
    }

    [Fact]
    public async Task Hit_WhenBust_EndsSessionWithLose()
    {
        var (_, svc) = await ArrangeAsync(10_000m);

        for (int i = 0; i < 500; i++)
        {
            var start = await svc.StartAsync(1, TestCodes.BlackjackGame,
                new StartBlackjackRequest { BetAmount = 10 });

            if (start.Data!.Status != "PlayerTurn") continue;

            var result = await svc.HitAsync(1, start.Data.SessionId);
            var d = result.Data!;

            if (d.PlayerScore > 21)
            {
                d.Status.Should().Be("Finished");
                d.Result.Should().Be("Lose");
                d.WinAmount.Should().Be(0m);
                return;
            }
        }
    }
      
    [Fact]
    public async Task EverySessionPublicId_IsUnique()
    {
        var (db, svc) = await ArrangeAsync(10_000m);

        for (int i = 0; i < 20; i++)
        {
            await svc.StartAsync(1, TestCodes.BlackjackGame,
                new StartBlackjackRequest { BetAmount = 10 });
        }

        var ids = await db.BlackjackSessions.Select(s => s.PublicId).ToListAsync();
        ids.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task WinAmount_WhenPlayerWins_IsExactlyTwiceTheBet()
    {
        var (_, svc) = await ArrangeAsync(10_000m);

        for (int i = 0; i < 500; i++)
        {
            var start = await svc.StartAsync(1, TestCodes.BlackjackGame,
                new StartBlackjackRequest { BetAmount = 10 });

            if (start.Data!.Status != "PlayerTurn") continue;

            var result = await svc.StandAsync(1, start.Data.SessionId);
            var d = result.Data!;

            if (d.Result == "Win")
            {
                d.WinAmount.Should().Be(20m);
                return;
            }
        }
    }

    [Fact]
    public async Task Push_ReturnsExactBet_NotMoreNotLess()
    {
        var (_, svc) = await ArrangeAsync(10_000m);

        for (int i = 0; i < 1000; i++)
        {
            var start = await svc.StartAsync(1, TestCodes.BlackjackGame,
                new StartBlackjackRequest { BetAmount = 10 });

            if (start.Data!.Status != "PlayerTurn") continue;

            var result = await svc.StandAsync(1, start.Data.SessionId);
            var d = result.Data!;

            if (d.Result == "Push")
            {
                d.WinAmount.Should().Be(10m);
                return;
            }
        }
    }

    [Fact]
    public async Task GameRound_DetailsJson_ContainsBothHands()
    {
        var (db, svc) = await ArrangeAsync(100m);
        var start = await svc.StartAsync(1, TestCodes.BlackjackGame,
            new StartBlackjackRequest { BetAmount = 10 });

        if (start.Data!.Status != "PlayerTurn") return;

        await svc.StandAsync(1, start.Data.SessionId);

        var round = await db.GameRounds.SingleAsync();
        round.DetailsJson.Should().Contain("player");
        round.DetailsJson.Should().Contain("dealer");
        round.DetailsJson.Should().Contain("playerScore");
        round.DetailsJson.Should().Contain("dealerScore");
    }

    [Fact]
    public async Task WalletNeverGoesNegative_AcrossManySessions()
    {
        var (db, svc) = await ArrangeAsync(100m);

        for (int i = 0; i < 100; i++)
        {
            var start = await svc.StartAsync(1, TestCodes.BlackjackGame,
                new StartBlackjackRequest { BetAmount = 5 });

            if (start.IsSuccess)
            {
                if (start.Data!.Status == "PlayerTurn")
                    await svc.StandAsync(1, start.Data.SessionId);
            }

            (await db.Wallets.FindAsync(1))!.Balance.Should().BeGreaterOrEqualTo(0m);
        }
    }
      
    [Fact]
    public async Task EverySession_HasDifferentPublicId_NotReusedOnBust()
    {
        var (db, svc) = await ArrangeAsync(10_000m);

        for (int i = 0; i < 20; i++)
            await svc.StartAsync(1, TestCodes.BlackjackGame,
                new StartBlackjackRequest { BetAmount = 10 });

        var ids = await db.BlackjackSessions.Select(s => s.PublicId).ToListAsync();
        ids.Should().OnlyHaveUniqueItems();
        ids.Should().OnlyContain(id => id.Length == 32);
    }

    [Fact]
    public async Task Hit_ThenStandOnSameSession_Works()
    {
        var (_, svc) = await ArrangeAsync(1000m);

        for (int i = 0; i < 50; i++)
        {
            var start = await svc.StartAsync(1, TestCodes.BlackjackGame,
                new StartBlackjackRequest { BetAmount = 10 });

            if (start.Data!.Status != "PlayerTurn") continue;

            var hit = await svc.HitAsync(1, start.Data.SessionId);
            if (hit.Data!.Status != "PlayerTurn") continue;

            var stand = await svc.StandAsync(1, start.Data.SessionId);

            stand.IsSuccess.Should().BeTrue();
            stand.Data!.Status.Should().Be("Finished");
            return;
        }
    }
}