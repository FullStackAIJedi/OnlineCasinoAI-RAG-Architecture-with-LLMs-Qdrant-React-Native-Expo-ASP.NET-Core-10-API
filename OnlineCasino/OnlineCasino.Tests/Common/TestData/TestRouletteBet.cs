namespace OnlineCasino.Tests.Common.TestData;
using OnlineCasino.Entities;

public sealed record TestRouletteBet
{
    public int? Id { get; init; }
    public int UserId { get; init; } = 1;
    public int BetTypeId { get; init; } = TestIds.RouletteRedId;
    public string BetTypeCode { get; init; } = "red";
    public int? NumberValue { get; init; }
    public decimal Amount { get; init; } = 10m;
    public DateTime CreatedAt { get; init; } = TestClock.FixedNow;

    public static TestRouletteBet Red => new();

    public static TestRouletteBet Black => new()
    {
        BetTypeId = TestIds.RouletteBlackId,
        BetTypeCode = "black"
    };

    public static TestRouletteBet Even => new()
    {
        BetTypeId = TestIds.RouletteEvenId,
        BetTypeCode = "even"
    };

    public static TestRouletteBet Odd => new()
    {
        BetTypeId = TestIds.RouletteOddId,
        BetTypeCode = "odd"
    };

    public static TestRouletteBet Low => new()
    {
        BetTypeId = TestIds.RouletteLowId,
        BetTypeCode = "low"
    };

    public static TestRouletteBet High => new()
    {
        BetTypeId = TestIds.RouletteHighId,
        BetTypeCode = "high"
    };

    public static TestRouletteBet Dozen(int dozen) => new()
    {
        BetTypeId = TestIds.RouletteDozenId,
        BetTypeCode = "dozen",
        NumberValue = dozen
    };

    public static TestRouletteBet Number(int number) => new()
    {
        BetTypeId = TestIds.RouletteNumberId,
        BetTypeCode = "number",
        NumberValue = number
    };

    public RouletteBet ToEntity() => new()
    {
        Id = Id ?? 0,
        UserId = UserId,
        RouletteBetTypeId = BetTypeId,
        NumberValue = NumberValue,
        Amount = Amount,
        CreatedAt = CreatedAt
    };
}