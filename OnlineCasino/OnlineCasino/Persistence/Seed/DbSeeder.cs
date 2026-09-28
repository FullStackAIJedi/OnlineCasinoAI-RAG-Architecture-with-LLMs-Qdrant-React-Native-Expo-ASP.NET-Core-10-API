namespace OnlineCasino.Data.Seed
{
    using Microsoft.EntityFrameworkCore;

    using OnlineCasino.Entities;

    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            await db.Database.MigrateAsync();

            if (!await db.Roles.AnyAsync())
            {
                db.Roles.AddRange(
                    new Role { Name = "User" },
                    new Role { Name = "Admin" });
            }

            if (!await db.TransactionTypes.AnyAsync())
            {
                db.TransactionTypes.AddRange(
                    new TransactionType { Name = "Deposit" },
                    new TransactionType { Name = "Withdraw" },
                    new TransactionType { Name = "Bet" },
                    new TransactionType { Name = "Win" });
            }

            if (!await db.GameTypes.AnyAsync())
            {
                db.GameTypes.AddRange(
                    new GameType { Name = "Slots" },
                    new GameType { Name = "Roulette" },
                    new GameType { Name = "Blackjack" });
            }

            if (!await db.RoundResults.AnyAsync())
            {
                db.RoundResults.AddRange(
                    new RoundResult { Name = "Win" },
                    new RoundResult { Name = "Lose" },
                    new RoundResult { Name = "Push" });
            }

            if (!await db.RouletteBetTypes.AnyAsync())
            {
                db.RouletteBetTypes.AddRange(
                    new RouletteBetType { Code = "red", Name = "Red" },
                    new RouletteBetType { Code = "black", Name = "Black" },
                    new RouletteBetType { Code = "even", Name = "Even" },
                    new RouletteBetType { Code = "odd", Name = "Odd" },
                    new RouletteBetType { Code = "low", Name = "Low (1-18)" },
                    new RouletteBetType { Code = "high", Name = "High (19-36)" },
                    new RouletteBetType { Code = "dozen", Name = "Dozen" },
                    new RouletteBetType { Code = "number", Name = "Single Number" });
            }

            if (!await db.BlackjackStatuses.AnyAsync())
            {
                db.BlackjackStatuses.AddRange(
                    new BlackjackStatus { Code = "PlayerTurn", Name = "Player Turn" },
                    new BlackjackStatus { Code = "DealerTurn", Name = "Dealer Turn" },
                    new BlackjackStatus { Code = "Finished", Name = "Finished" },
                    new BlackjackStatus { Code = "Bust", Name = "Bust" });
            }

            if (!await db.SlotSymbols.AnyAsync())
            {
                db.SlotSymbols.AddRange(
                    new SlotSymbol { Code = "cherry", Name = "Cherry", Payout3 = 5m, Weight = 30 },
                    new SlotSymbol { Code = "lemon", Name = "Lemon", Payout3 = 8m, Weight = 25 },
                    new SlotSymbol { Code = "bell", Name = "Bell", Payout3 = 15m, Weight = 20 },
                    new SlotSymbol { Code = "bar", Name = "BAR", Payout3 = 30m, Weight = 15 },
                    new SlotSymbol { Code = "seven", Name = "Seven", Payout3 = 100m, Weight = 10 });
            }

            await db.SaveChangesAsync();

            if (!await db.Games.AnyAsync())
            {
                var slotsType = await db.GameTypes.FirstAsync(t => t.Name == "Slots");
                var rouletteType = await db.GameTypes.FirstAsync(t => t.Name == "Roulette");
                var blackjackType = await db.GameTypes.FirstAsync(t => t.Name == "Blackjack");

                db.Games.AddRange(
                    new Game { Code = "slots", Name = "Lucky Slots", GameTypeId = slotsType.Id, MinBet = 1, MaxBet = 100 },
                    new Game { Code = "roulette", Name = "European Roulette", GameTypeId = rouletteType.Id, MinBet = 1, MaxBet = 500 },
                    new Game { Code = "blackjack", Name = "Classic Blackjack", GameTypeId = blackjackType.Id, MinBet = 5, MaxBet = 500 });
            }

            await db.SaveChangesAsync();
        }
    }
}
