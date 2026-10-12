namespace OnlineCasino.Tests.Common.Fixtures;

using Microsoft.EntityFrameworkCore;

using OnlineCasino.Data;
using OnlineCasino.Entities;
using OnlineCasino.Tests.Common.TestData;

public static class TestDbFactory
{
    public static AppDbContext CreateInMemory(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(
                Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AppDbContext(options);
    }

    public static async Task<AppDbContext> CreateSeededAsync(string? dbName = null)
    {
        var db = CreateInMemory(dbName);
        await SeedLookupsAsync(db);
        return db;
    }

    public static async Task SeedLookupsAsync(AppDbContext db)
    {
        if (!await db.Roles.AnyAsync())
        {
            db.Roles.AddRange(
                new Role { Id = TestIds.UserRoleId, Name = "User" },
                new Role { Id = TestIds.AdminRoleId, Name = "Admin" });
        }

        if (!await db.TransactionTypes.AnyAsync())
        {
            db.TransactionTypes.AddRange(
                new TransactionType { Id = TestIds.DepositTypeId, Name = "Deposit" },
                new TransactionType { Id = TestIds.WithdrawTypeId, Name = "Withdraw" },
                new TransactionType { Id = TestIds.BetTypeId, Name = "Bet" },
                new TransactionType { Id = TestIds.WinTypeId, Name = "Win" });
        }

        if (!await db.GameTypes.AnyAsync())
        {
            db.GameTypes.AddRange(
                new GameType { Id = TestIds.SlotsTypeId, Name = "Slots" },
                new GameType { Id = TestIds.RouletteTypeId, Name = "Roulette" },
                new GameType { Id = TestIds.BlackjackTypeId, Name = "Blackjack" });
        }

        if (!await db.RoundResults.AnyAsync())
        {
            db.RoundResults.AddRange(
                new RoundResult { Id = TestIds.WinResultId, Name = "Win" },
                new RoundResult { Id = TestIds.LoseResultId, Name = "Lose" },
                new RoundResult { Id = TestIds.PushResultId, Name = "Push" });
        }

        if (!await db.RouletteBetTypes.AnyAsync())
        {
            db.RouletteBetTypes.AddRange(
                new RouletteBetType { Id = 1, Code = "red", Name = "Red" },
                new RouletteBetType { Id = 2, Code = "black", Name = "Black" },
                new RouletteBetType { Id = 3, Code = "even", Name = "Even" },
                new RouletteBetType { Id = 4, Code = "odd", Name = "Odd" },
                new RouletteBetType { Id = 5, Code = "low", Name = "Low" },
                new RouletteBetType { Id = 6, Code = "high", Name = "High" },
                new RouletteBetType { Id = 7, Code = "dozen", Name = "Dozen" },
                new RouletteBetType { Id = 8, Code = "number", Name = "Number" });
        }

        if (!await db.BlackjackStatuses.AnyAsync())
        {
            db.BlackjackStatuses.AddRange(
                new BlackjackStatus { Id = 1, Code = "PlayerTurn", Name = "Player Turn" },
                new BlackjackStatus { Id = 2, Code = "DealerTurn", Name = "Dealer Turn" },
                new BlackjackStatus { Id = 3, Code = "Finished", Name = "Finished" },
                new BlackjackStatus { Id = 4, Code = "Bust", Name = "Bust" });
        }

        if (!await db.SlotSymbols.AnyAsync())
        {
            db.SlotSymbols.AddRange(
                new SlotSymbol { Id = 1, Code = "cherry", Name = "Cherry", Payout3 = 5m, Payout4 = 25m, Weight = 30 },
                new SlotSymbol { Id = 2, Code = "lemon", Name = "Lemon", Payout3 = 8m, Payout4 = 40m, Weight = 25 },
                new SlotSymbol { Id = 3, Code = "bell", Name = "Bell", Payout3 = 15m, Payout4 = 75m, Weight = 20 },
                new SlotSymbol { Id = 4, Code = "bar", Name = "BAR", Payout3 = 30m, Payout4 = 150m, Weight = 15 },
                new SlotSymbol { Id = 5, Code = "seven", Name = "Seven", Payout3 = 100m, Payout4 = 500m, Weight = 10 });
        }

        if (!await db.Games.AnyAsync())
        {
            db.Games.AddRange(
                new Game { Id = 1, Code = "slots", Name = "Lucky Slots", GameTypeId = 1, MinBet = 1, MaxBet = 100, ReelsCount = 3, Enabled = true },
                new Game { Id = 2, Code = "slots4", Name = "Mega Slots", GameTypeId = 1, MinBet = 1, MaxBet = 100, ReelsCount = 4, Enabled = true },
                new Game { Id = 3, Code = "roulette", Name = "Roulette", GameTypeId = 2, MinBet = 1, MaxBet = 500, ReelsCount = 0, Enabled = true },
                new Game { Id = 4, Code = "blackjack", Name = "Blackjack", GameTypeId = 3, MinBet = 5, MaxBet = 500, ReelsCount = 0, Enabled = true },
                new Game { Id = 5, Code = "disabled", Name = "Disabled", GameTypeId = 1, MinBet = 1, MaxBet = 100, ReelsCount = 3, Enabled = false });
        }

        await db.SaveChangesAsync();
    }
}
