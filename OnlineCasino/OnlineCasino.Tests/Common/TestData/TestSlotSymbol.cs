namespace OnlineCasino.Tests.Common.TestData;
using OnlineCasino.Entities;

public sealed record TestSlotSymbol
{
    public int? Id { get; init; }
    public string Code { get; init; } = "cherry";
    public string Name { get; init; } = "Cherry";
    public decimal Payout3 { get; init; } = 5m;
    public decimal Payout4 { get; init; } = 25m;
    public decimal Payout5 { get; init; }
    public decimal Weight { get; init; } = 1m;

    public static TestSlotSymbol Cherry => new();

    public static TestSlotSymbol Seven => new()
    {
        Code = "seven",
        Name = "Seven",
        Payout3 = 100m,
        Payout4 = 500m
    };

    public SlotSymbol ToEntity() => new()
    {
        Id = Id ?? 0,
        Code = Code,
        Name = Name,
        Payout3 = Payout3,
        Payout4 = Payout4,
        Payout5 = Payout5,
        Weight = Weight
    };
}
