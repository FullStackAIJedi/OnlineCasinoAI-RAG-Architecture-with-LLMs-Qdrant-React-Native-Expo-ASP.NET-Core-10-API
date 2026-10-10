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
             
             /* Slot symbols 
             Payout rule:
             Payout4 = Payout3 × 5    (the chance of 4 matching is ~5× lower than 3 matching)
             Payout5 = Payout3 × 25   (for future 5-reel games)
             This keeps the RTP ~95% consistent across all slot games.*/
            if (!await db.SlotSymbols.AnyAsync())
            {
                db.SlotSymbols.AddRange(
                    // 3-reel symbols (but with Payout4 for slots4)
                    new SlotSymbol { Code = "cherry", Name = "Cherry", Payout3 = 5m, Payout4 = 25m, Weight = 30 },
                    new SlotSymbol { Code = "lemon", Name = "Lemon", Payout3 = 8m, Payout4 = 40m, Weight = 25 },
                    new SlotSymbol { Code = "bell", Name = "Bell", Payout3 = 15m, Payout4 = 75m, Weight = 20 },
                    new SlotSymbol { Code = "bar", Name = "BAR", Payout3 = 30m, Payout4 = 150m, Weight = 15 },
                    new SlotSymbol { Code = "seven", Name = "Seven", Payout3 = 100m, Payout4 = 500m, Weight = 10 },

                    // 4-reel symbols (rarer, higher payouts)
                    new SlotSymbol { Code = "diamond", Name = "Diamond", Payout3 = 50m, Payout4 = 250m, Weight = 12 },
                    new SlotSymbol { Code = "clover", Name = "Clover", Payout3 = 12m, Payout4 = 60m, Weight = 22 },
                    new SlotSymbol { Code = "star", Name = "Star", Payout3 = 25m, Payout4 = 125m, Weight = 18 }
                );
            }

            await db.SaveChangesAsync();

            // Games 
            // ReelsCount:
            // 3  → slots (3 reels)
            // 4  → slots4 (4 reels)
            // 0  → non-slot games (roulette, blackjack)
            if (!await db.Games.AnyAsync())
            {
                var slotsType = await db.GameTypes.FirstAsync(t => t.Name == "Slots");
                var rouletteType = await db.GameTypes.FirstAsync(t => t.Name == "Roulette");
                var blackjackType = await db.GameTypes.FirstAsync(t => t.Name == "Blackjack");

                db.Games.AddRange(
                    new Game { Code = "slots", Name = "Lucky Slots", GameTypeId = slotsType.Id, MinBet = 1, MaxBet = 100, ReelsCount = 3 },
                    new Game { Code = "slots4", Name = "Mega Slots 4x", GameTypeId = slotsType.Id, MinBet = 1, MaxBet = 100, ReelsCount = 4 },
                    new Game { Code = "roulette", Name = "European Roulette", GameTypeId = rouletteType.Id, MinBet = 1, MaxBet = 500, ReelsCount = 0 },
                    new Game { Code = "blackjack", Name = "Classic Blackjack", GameTypeId = blackjackType.Id, MinBet = 5, MaxBet = 500, ReelsCount = 0 });
            }

            await db.SaveChangesAsync();
        }
    }
}
