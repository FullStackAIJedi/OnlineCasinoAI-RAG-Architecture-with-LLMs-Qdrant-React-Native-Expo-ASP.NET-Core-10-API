namespace OnlineCasino.Tests.Common.TestData;
using OnlineCasino.Entities;

public sealed record TestBlackjackSession
{
    public int? Id { get; init; }
    public string PublicId { get; init; } = Guid.NewGuid().ToString("N");
    public int UserId { get; init; } = 1;
    public int GameId { get; init; } = TestIds.BlackjackGameId;
    public decimal BetAmount { get; init; } = 10m;
    public string PlayerHand { get; init; } = "10♠,7♥";
    public string DealerHand { get; init; } = "9♦,6♣";
    public int PlayerScore { get; init; } = 17;
    public int DealerScore { get; init; } = 15;
    public int StatusId { get; init; } = TestIds.PlayerTurnStatusId;
    public DateTime CreatedAt { get; init; } = TestClock.FixedNow;
    public DateTime? FinishedAt { get; init; }

    public static TestBlackjackSession PlayerTurn => new();

    public static TestBlackjackSession FinishedWin => new()
    {
        StatusId = TestIds.FinishedStatusId,
        PlayerHand = "10♠,9♥",
        PlayerScore = 19,
        DealerHand = "9♦,6♣,2♥",
        DealerScore = 17,
        FinishedAt = TestClock.FixedNow
    };

    public static TestBlackjackSession BustSession => new()
    {
        StatusId = TestIds.BustStatusId,
        PlayerHand = "10♠,9♥,5♣",
        PlayerScore = 24,
        FinishedAt = TestClock.FixedNow
    };

    public BlackjackSession ToEntity() => new()
    {
        Id = Id ?? 0,
        PublicId = PublicId,
        UserId = UserId,
        GameId = GameId,
        BetAmount = BetAmount,
        PlayerHand = PlayerHand,
        DealerHand = DealerHand,
        PlayerScore = PlayerScore,
        DealerScore = DealerScore,
        StatusId = StatusId,
        CreatedAt = CreatedAt,
        FinishedAt = FinishedAt
    };
}