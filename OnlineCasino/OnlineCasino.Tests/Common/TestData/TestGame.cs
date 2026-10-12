namespace OnlineCasino.Tests.Common.TestData;
using OnlineCasino.Entities;

public sealed record TestGame
{
    public int? Id { get; init; }
    public string Code { get; init; } = TestCodes.SlotsGame;
    public string Name { get; init; } = "Lucky Slots";
    public int GameTypeId { get; init; } = TestIds.SlotsTypeId;
    public string GameTypeName { get; init; } = "Slots";
    public bool Enabled { get; init; } = true;
    public decimal MinBet { get; init; } = 1m;
    public decimal MaxBet { get; init; } = 100m;
    public int ReelsCount { get; init; } = 3;

    public static TestGame Slots => new();

    public static TestGame Slots4 => new()
    {
        Code = TestCodes.Slots4Game,
        Name = "Mega Slots",
        ReelsCount = 4
    };

    public static TestGame Roulette => new()
    {
        Code = TestCodes.RouletteGame,
        Name = "Roulette",
        GameTypeId = TestIds.RouletteTypeId,
        GameTypeName = "Roulette",
        ReelsCount = 0,
        MaxBet = 500m
    };

    public static TestGame Blackjack => new()
    {
        Code = TestCodes.BlackjackGame,
        Name = "Blackjack",
        GameTypeId = TestIds.BlackjackTypeId,
        GameTypeName = "Blackjack",
        ReelsCount = 0,
        MinBet = 5m,
        MaxBet = 500m
    };

    public static TestGame DisabledSlots => new()
    {
        Code = TestCodes.DisabledGame,
        Name = "Disabled Slots",
        Enabled = false
    };

    public Game ToEntity() => new()
    {
        Id = Id ?? 0,
        Code = Code,
        Name = Name,
        GameTypeId = GameTypeId,
        GameType = new GameType { Id = GameTypeId, Name = GameTypeName },
        Enabled = Enabled,
        MinBet = MinBet,
        MaxBet = MaxBet,
        ReelsCount = ReelsCount
    };
}