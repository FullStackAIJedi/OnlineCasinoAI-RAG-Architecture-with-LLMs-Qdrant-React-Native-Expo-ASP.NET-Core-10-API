namespace OnlineCasino.Services
{
    using System.Text.Json;

    using Microsoft.EntityFrameworkCore;

    using OnlineCasino.Data;
    using OnlineCasino.DTOs.Roulette;
    using OnlineCasino.Entities;
    using OnlineCasino.Models;

    /// <summary>
    /// Roulette service – two-phase game: betting → spin.
    /// <para>
    /// Uses AMERICAN roulette (double zero) – 38 numbers (0, 00, 1–36).
    /// In the backend, <c>37</c> represents <c>00</c> (because <c>00</c> is not a valid int).
    /// </para>
    /// <para>
    /// Payout table (multipliers):
    /// <list type="bullet">
    ///   <item><c>red</c>, <c>black</c>, <c>even</c>, <c>odd</c>, <c>low</c>, <c>high</c> → 2×</item>
    ///   <item><c>dozen</c> → 3×</item>
    ///   <item><c>number</c> → 36×</item>
    /// </list>
    /// </para>
    /// </summary>
    public interface IRouletteService
    {
        /// <summary>
        /// Retrieves all pending (unresolved) bets placed by the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user whose bets are being retrieved.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a list of <see cref="CurrentBetDto"/> on success,
        /// or an error message and status code on failure.
        /// </returns>
        Task<Result<List<CurrentBetDto>>> GetCurrentBetsAsync(int userId);

        /// <summary>
        /// Places a new bet for the specified user without immediately resolving it.
        /// </summary>
        /// <param name="userId">The ID of the user placing the bet.</param>
        /// <param name="gameCode">The unique code identifying the roulette game.</param>
        /// <param name="req">The bet request containing the bet type, optional number value, and amount.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the created <see cref="CurrentBetDto"/> on success,
        /// or an error message and status code on failure.
        /// </returns>
        Task<Result<CurrentBetDto>> PlaceBetAsync(int userId, string gameCode, PlaceBetRequest req);

        /// <summary>
        /// Resolves all pending bets for the specified user by spinning the wheel.
        /// <para>
        /// Debits the total bet amount, generates a winning number (0–37, where 37 = 00),
        /// calculates all payouts, credits any winnings, records the round, and clears pending bets.
        /// </para>
        /// </summary>
        /// <param name="userId">The ID of the user spinning the wheel.</param>
        /// <param name="gameCode">The unique code identifying the roulette game.</param>
        /// <param name="req">The spin request containing the client round ID.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a <see cref="RouletteSpinResponse"/> with the round ID,
        /// winning number, winning color, total bet, total win, new balance, and result name on success;
        /// otherwise, an error message and status code.
        /// </returns>
        Task<Result<RouletteSpinResponse>> SpinAsync(int userId, string gameCode, SpinRouletteRequest req);
    }

    /// <summary>
    /// Implementation of <see cref="IRouletteService"/>.
    /// <para>
    /// American roulette: 0, 00, 1–36 (38 numbers).
    /// In code, <c>37 = 00</c>.
    /// </para>
    /// </summary>
    public class RouletteService : IRouletteService
    {
        private readonly AppDbContext _db;
        private readonly IWalletService _wallet;

        /// <summary>Red numbers (00 is not red, 0 is not red).</summary>
        private static readonly int[] RedNumbers =
            { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };

        /// <summary>
        /// Initializes a new instance of the <see cref="RouletteService"/> class.
        /// </summary>
        /// <param name="db">The application database context.</param>
        /// <param name="wallet">The wallet service used for debiting and crediting user balances.</param>
        public RouletteService(AppDbContext db, IWalletService wallet)
        {
            _db = db;
            _wallet = wallet;
        }

        /// <inheritdoc />
        public async Task<Result<List<CurrentBetDto>>> GetCurrentBetsAsync(int userId)
        {
            var list = await _db.RouletteBets
                .AsNoTracking()
                .Where(b => b.UserId == userId)
                .OrderBy(b => b.CreatedAt)
                .Select(b => new CurrentBetDto
                {
                    Id = b.Id,
                    Type = b.BetType.Code,
                    Value = b.NumberValue,
                    Amount = b.Amount
                })
                .ToListAsync();

            return Result<List<CurrentBetDto>>.Success(list);
        }

        /// <inheritdoc />
        public async Task<Result<CurrentBetDto>> PlaceBetAsync(int userId, string gameCode, PlaceBetRequest req)
        {
            if (req.Amount <= 0)
                return Result<CurrentBetDto>.Failure("Bet amount must be positive.", 400);

            var game = await _db.Games
                .Where(g => g.Code == gameCode && g.Enabled)
                .Select(g => new { g.Id, g.MinBet, g.MaxBet })
                .FirstOrDefaultAsync();

            if (game is null)
                return Result<CurrentBetDto>.Failure("Game not found.", 404);

            var betType = await _db.RouletteBetTypes
                .Where(t => t.Code == req.Type)
                .Select(t => new { t.Id, t.Code })
                .FirstOrDefaultAsync();

            if (betType is null)
                return Result<CurrentBetDto>.Failure("Invalid bet type.", 400);
             
            if (betType.Code == "number" && (req.Value is null || req.Value < 0 || req.Value > 37))
                return Result<CurrentBetDto>.Failure("Number must be between 0 and 37 (37 = 00).", 400);

            if (betType.Code == "dozen" && (req.Value is null || req.Value < 1 || req.Value > 3))
                return Result<CurrentBetDto>.Failure("Dozen must be 1, 2 or 3.", 400);

            var balance = await _db.Wallets
                .Where(w => w.UserId == userId)
                .Select(w => w.Balance)
                .FirstAsync();

            var pendingTotal = await _db.RouletteBets
                .Where(b => b.UserId == userId)
                .SumAsync(b => (decimal?)b.Amount) ?? 0m;

            if (balance < pendingTotal + req.Amount)
                return Result<CurrentBetDto>.Failure("Insufficient balance.", 400);

            var bet = new RouletteBet
            {
                UserId = userId,
                RouletteBetTypeId = betType.Id,
                NumberValue = req.Value,
                Amount = req.Amount,
                CreatedAt = DateTime.UtcNow
            };
            _db.RouletteBets.Add(bet);
            await _db.SaveChangesAsync();

            return Result<CurrentBetDto>.Success(new CurrentBetDto
            {
                Id = bet.Id,
                Type = betType.Code,
                Value = req.Value,
                Amount = req.Amount
            });
        }

        /// <summary>
        /// Resolves all pending bets for the specified user by spinning the roulette wheel.
        /// <para>
        /// The method performs the following steps:
        /// <list type="number">
        ///   <item>Validates that the game exists and is enabled.</item>
        ///   <item>Loads all pending bets for the user; fails if none are found.</item>
        ///   <item>Debits the total bet amount from the user's wallet.</item>
        ///   <item>Generates a winning number (0–37, where 37 represents 00).</item>
        ///   <item>Determines the winning color (green for 0 and 37, otherwise red or black).</item>
        ///   <item>Calculates the total payout across all bets.</item>
        ///   <item>Credits any winnings back to the user's wallet.</item>
        ///   <item>Saves the round result and clears the user's pending bets.</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="userId">The ID of the user spinning the wheel.</param>
        /// <param name="gameCode">The unique code identifying the roulette game.</param>
        /// <param name="req">The spin request containing the client round ID used as an idempotency reference.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a <see cref="RouletteSpinResponse"/> with the round ID,
        /// winning number, winning color, total bet, total win, new balance, and result name on success;
        /// otherwise, an error message and status code on failure.
        /// </returns>
        public async Task<Result<RouletteSpinResponse>> SpinAsync(int userId, string gameCode, SpinRouletteRequest req)
        { 
            var game = await _db.Games
                .Where(g => g.Code == gameCode && g.Enabled)
                .Select(g => new { g.Id })
                .FirstOrDefaultAsync();

            if (game is null)
            { 
                return Result<RouletteSpinResponse>.Failure("Game not found.", 404);
            }

            var bets = await _db.RouletteBets
                .Where(b => b.UserId == userId)
                .Select(b => new { b.Id, TypeCode = b.BetType.Code, b.NumberValue, b.Amount })
                .ToListAsync();

            if (bets.Count == 0)
            { 
                return Result<RouletteSpinResponse>.Failure("No bets placed.", 400);
            }

            decimal totalBet = bets.Sum(b => b.Amount); 
             
            var debit = await _wallet.DebitAsync(userId, totalBet, req.ClientRoundId);
            if (!debit.IsSuccess)
            { 
                return Result<RouletteSpinResponse>.Failure(debit.Error!.Message, debit.Error.StatusCode);
            } 
             
            int winning = Random.Shared.Next(0, 38); 
            string color = (winning == 0 || winning == 37)
                ? "green"
                : (RedNumbers.Contains(winning) ? "red" : "black");
              
            decimal totalWin = 0;
            foreach (var b in bets)
            {
                decimal payout = CalculatePayout(b.TypeCode, b.NumberValue, winning, color, b.Amount);
                totalWin += payout;
            } 
             
            decimal balance = debit.Data;
            if (totalWin > 0)
            {
                var credit = await _wallet.CreditAsync(userId, totalWin, req.ClientRoundId);
                if (!credit.IsSuccess)
                { 
                    return Result<RouletteSpinResponse>.Failure(credit.Error!.Message, credit.Error.StatusCode);
                }
                balance = credit.Data; 
            } 
             
            var resultName = totalWin > totalBet ? "Win" : (totalWin == totalBet ? "Push" : "Lose"); 

            var resultId = await _db.RoundResults
                .Where(r => r.Name == resultName)
                .Select(r => r.Id)
                .FirstAsync();

            var round = new GameRound
            {
                UserId = userId,
                GameId = game.Id,
                RoundResultId = resultId,
                BetAmount = totalBet,
                WinAmount = totalWin,
                DetailsJson = JsonSerializer.Serialize(new { winning, color }),
                CreatedAt = DateTime.UtcNow
            };
            _db.GameRounds.Add(round);

            var toRemove = await _db.RouletteBets.Where(b => b.UserId == userId).ToListAsync();
            _db.RouletteBets.RemoveRange(toRemove);

            await _db.SaveChangesAsync(); 

            return Result<RouletteSpinResponse>.Success(new RouletteSpinResponse
            {
                RoundId = round.PublicId,
                WinningNumber = winning,
                WinningColor = color,
                TotalBet = totalBet,
                TotalWin = totalWin,
                Balance = balance,
                Result = resultName
            });
        }

        /// <summary>
        /// Calculates the payout for a single bet.
        /// <para>
        /// IMPORTANT: <c>37 = 00</c>. If the bet is <c>number</c> with value <c>37</c>,
        /// it only wins if winning == 37.
        /// </para>
        /// </summary>
        /// <param name="type">The bet type code (e.g. "red", "black", "number", "dozen").</param>
        /// <param name="value">The optional numeric value associated with the bet (used for "number" and "dozen").</param>
        /// <param name="winning">The winning number (0–37, where 37 = 00).</param>
        /// <param name="color">The color of the winning number ("red", "black", or "green").</param>
        /// <param name="amount">The amount wagered on this bet.</param>
        /// <returns>The payout amount for this bet (0 if the bet loses).</returns>
        private static decimal CalculatePayout(string type, int? value, int winning, string color, decimal amount)
        {
            return type switch
            {
                "red" => color == "red" ? amount * 2 : 0,
                "black" => color == "black" ? amount * 2 : 0, 
                "even" => winning != 0 && winning != 37 && winning % 2 == 0 ? amount * 2 : 0,
                "odd" => winning != 0 && winning != 37 && winning % 2 == 1 ? amount * 2 : 0, 
                "low" => winning >= 1 && winning <= 18 ? amount * 2 : 0,
                "high" => winning >= 19 && winning <= 36 ? amount * 2 : 0, 
                "dozen" => value switch
                {
                    1 => winning >= 1 && winning <= 12 ? amount * 3 : 0,
                    2 => winning >= 13 && winning <= 24 ? amount * 3 : 0,
                    3 => winning >= 25 && winning <= 36 ? amount * 3 : 0,
                    _ => 0
                },
                 
                "number" => value == winning ? amount * 36 : 0,

                _ => 0
            };
        }
    }
}