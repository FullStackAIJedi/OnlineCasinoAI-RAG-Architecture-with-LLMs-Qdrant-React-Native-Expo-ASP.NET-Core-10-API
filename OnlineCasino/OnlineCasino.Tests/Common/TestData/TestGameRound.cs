namespace OnlineCasino.Tests.Common.TestData;
using OnlineCasino.Entities;

public sealed record TestGameRound
{
    public int? Id { get; init; }
    public string PublicId { get; init; } = Guid.NewGuid().ToString("N");
    public int UserId { get; init; } = 1;
    public int GameId { get; init; } = TestIds.SlotsGameId;
    public int? ResultId { get; init; } = TestIds.WinResultId;
    public decimal BetAmount { get; init; } = 10m;
    public decimal WinAmount { get; init; }
    public DateTime CreatedAt { get; init; } = TestClock.FixedNow;

    public static TestGameRound WinningRound => new()
    {
        ResultId = TestIds.WinResultId,
        WinAmount = 20m
    };

    public static TestGameRound LosingRound => new()
    {
        ResultId = TestIds.LoseResultId
    };

    public static TestGameRound PushRound => new()
    {
        ResultId = TestIds.PushResultId,
        WinAmount = 10m
    };

    public static TestGameRound UnresolvedRound => new() { ResultId = null };

    public GameRound ToEntity() => new()
    {
        Id = Id ?? 0,
        PublicId = PublicId,
        UserId = UserId,
        GameId = GameId,
        RoundResultId = ResultId,
        BetAmount = BetAmount,
        WinAmount = WinAmount,
        CreatedAt = CreatedAt
    };
}
