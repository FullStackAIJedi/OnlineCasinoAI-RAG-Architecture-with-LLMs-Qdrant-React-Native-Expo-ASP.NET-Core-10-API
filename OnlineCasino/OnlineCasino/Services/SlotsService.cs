namespace OnlineCasino.Services
{
    using System.Text.Json;

    using Microsoft.EntityFrameworkCore;

    using OnlineCasino.Data;
    using OnlineCasino.Entities;
    using OnlineCasino.DTOs.Slots;
    using OnlineCasino.Models;

    /// <summary>
    /// Slots service – supports 3, 4, or more reels.
    /// <para>
    /// The number of reels is read from <c>Game.ReelsCount</c> in the database.
    /// Payout logic:
    /// <list type="bullet">
    ///   <item>All reels match → <c>BetAmount × Payout{N}</c> (where N = ReelsCount).</item>
    ///   <item>N-1 reels match (e.g. 3 out of 4) → <c>BetAmount × Payout{N-1}</c>.</item>
    ///   <item>Fewer matches → 0.</item>
    /// </list>
    /// </para>
    /// </summary> 
    public interface ISlotsService
    {
         /// <summary>
         /// Spins the reels for a given slot game on behalf of the specified user.
         /// </summary>
         /// <param name="userId">The ID of the user performing the spin.</param>
         /// <param name="gameCode">The unique code identifying the slot game.</param>
         /// <param name="req">The spin request containing the bet amount and client round ID.</param>
         /// <returns>
         /// A <see cref="Result{T}"/> containing a <see cref="SpinResponse"/> on success,
         /// or an error message and status code on failure.
         /// </returns>
        Task<Result<SpinResponse>> SpinAsync(int userId, string gameCode, SpinRequest req);
    }

    /// <summary>
    /// Implementation of <see cref="ISlotsService"/>. Generic for all slot games.
    /// </summary>
    public class SlotsService : ISlotsService
    {
        private readonly AppDbContext _db;
        private readonly IWalletService _wallet;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlotsService"/> class.
        /// </summary>
        /// <param name="db">The application database context.</param>
        /// <param name="wallet">The wallet service used for debiting and crediting user balances.</param>
        public SlotsService(AppDbContext db, IWalletService wallet)
        {
            _db = db;
            _wallet = wallet;
        }

        /// <summary>
        /// Spins the reels for a given slot game on behalf of the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user performing the spin.</param>
        /// <param name="gameCode">The unique code identifying the slot game.</param>
        /// <param name="req">The spin request containing the bet amount and client round ID.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a <see cref="SpinResponse"/> on success,
        /// or an error message and status code on failure.
        /// </returns>
        public async Task<Result<SpinResponse>> SpinAsync(int userId, string gameCode, SpinRequest req)
        {  
            if (req.BetAmount <= 0)
                return Result<SpinResponse>.Failure("Bet amount must be positive.", 400);

            var game = await _db.Games
                .Where(g => g.Code == gameCode && g.Enabled)
                .Select(g => new { g.Id, g.MinBet, g.MaxBet, g.ReelsCount })
                .FirstOrDefaultAsync();

            if (game is null)
            { 
                return Result<SpinResponse>.Failure("Game not found.", 404);
            } 

            if (game.ReelsCount < 3)
            { 
                return Result<SpinResponse>.Failure("This game is not a slot game.", 400);
            }

            if (req.BetAmount < game.MinBet || req.BetAmount > game.MaxBet)
                return Result<SpinResponse>.Failure($"Bet must be between {game.MinBet} and {game.MaxBet}.", 400);
          
            var debit = await _wallet.DebitAsync(userId, req.BetAmount, req.ClientRoundId);
            if (!debit.IsSuccess)
                return Result<SpinResponse>.Failure(debit.Error!.Message, debit.Error.StatusCode);

             
            var symbols = await _db.SlotSymbols.AsNoTracking().ToListAsync(); 

            var reels = new List<string>();
            var rng = Random.Shared;

            for (int i = 0; i < game.ReelsCount; i++)
            {
                reels.Add(PickWeighted(symbols, rng).Code);
            }
              
            var grouped = reels
                .GroupBy(r => r)
                .Select(g => new { Code = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .First();

            int matchCount = grouped.Count;
            var sym = symbols.First(s => s.Code == grouped.Code);
             
            decimal winAmount = 0;
            if (matchCount == game.ReelsCount)
            {
                var multiplier = GetPayout(sym, game.ReelsCount);
                winAmount = req.BetAmount * multiplier; 
            }
            else if (matchCount == game.ReelsCount - 1)
            {
                var multiplier = GetPayout(sym, game.ReelsCount - 1);
                winAmount = req.BetAmount * multiplier; 
            }   

            decimal balance = debit.Data;
            if (winAmount > 0)
            {
                var credit = await _wallet.CreditAsync(userId, winAmount, req.ClientRoundId);
                if (!credit.IsSuccess)
                    return Result<SpinResponse>.Failure(credit.Error!.Message, credit.Error.StatusCode);
                balance = credit.Data; 
            }
             
            var resultName = winAmount > 0 ? "Win" : "Lose";
            var resultId = await _db.RoundResults
                .Where(r => r.Name == resultName)
                .Select(r => r.Id)
                .FirstAsync();

            var round = new GameRound
            {
                UserId = userId,
                GameId = game.Id,
                RoundResultId = resultId,
                BetAmount = req.BetAmount,
                WinAmount = winAmount,
                DetailsJson = JsonSerializer.Serialize(new { reels, matchCount }),
                CreatedAt = DateTime.UtcNow
            };
            _db.GameRounds.Add(round);
            await _db.SaveChangesAsync();
             

            return Result<SpinResponse>.Success(new SpinResponse
            {
                RoundId = round.PublicId,
                Reels = reels,
                BetAmount = req.BetAmount,
                WinAmount = winAmount,
                Balance = balance,
                Result = resultName,
                MatchCount = matchCount
            });
        }

        /// <summary>
        /// Returns the payout multiplier for a given symbol based on the number of matches.
        /// <para>
        /// Supports 3 and 4 reels. For 5+ reels, add <c>Payout5</c> to <see cref="SlotSymbol"/>.
        /// </para>
        /// </summary>
        /// <param name="sym">The slot symbol to get the payout for.</param>
        /// <param name="matches">The number of matching reels.</param>
        /// <returns>The payout multiplier, or 0 if the match count is not supported.</returns>
        private static decimal GetPayout(SlotSymbol sym, int matches)
        {
            return matches switch
            {
                3 => sym.Payout3,
                4 => sym.Payout4,
                _ => 0m
            };
        }

        /// <summary>
        /// Performs a weighted random selection of a symbol.
        /// </summary>
        /// <param name="symbols">The list of available slot symbols with their weights.</param>
        /// <param name="rng">The random number generator instance.</param>
        /// <returns>The selected <see cref="SlotSymbol"/>.</returns>
        private static SlotSymbol PickWeighted(List<SlotSymbol> symbols, Random rng)
        {
            var total = symbols.Sum(s => s.Weight);
            var roll = (decimal)rng.NextDouble() * total;
            decimal acc = 0;

            foreach (var s in symbols)
            {
                acc += s.Weight;
                if (roll <= acc) return s;
            }

            return symbols[^1];
        }
    }
}
