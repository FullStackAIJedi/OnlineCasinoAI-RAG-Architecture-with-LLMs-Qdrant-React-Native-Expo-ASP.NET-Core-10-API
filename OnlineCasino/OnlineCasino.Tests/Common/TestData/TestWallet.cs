namespace OnlineCasino.Tests.Common.TestData;
using OnlineCasino.Entities;

public sealed record TestWallet
{
    public int? Id { get; init; }
    public decimal Balance { get; init; } = 1000m;
    public string Currency { get; init; } = "EUR";

    public static TestWallet Standard => new();
    public static TestWallet EmptyBalance => new() { Balance = 0m };
    public static TestWallet HighBalance => new() { Balance = 100_000m };

    public Wallet ToEntity(int userId) => new()
    {
        Id = Id ?? 0,
        UserId = userId,
        Balance = Balance,
        Currency = Currency,
        UpdatedAt = TestClock.FixedNow
    };
}
