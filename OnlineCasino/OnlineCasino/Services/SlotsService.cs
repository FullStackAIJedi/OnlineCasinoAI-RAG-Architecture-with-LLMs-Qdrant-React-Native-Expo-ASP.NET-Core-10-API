//namespace OnlineCasino.Services
//{
//    using System.Text.Json;

//    using Microsoft.EntityFrameworkCore;

//    using OnlineCasino.Data;
//    using OnlineCasino.Entities;

//    using OnlineCasino.DTOs.Slots;
//    using OnlineCasino.Models;

//    /// <summary>
//    /// Slots service – еднофазна игра: spin → резултат.
//    /// <para>
//    /// Логика:
//    /// <list type="number">
//    ///   <item>Удържа залога от wallet-а (<c>TransactionType = Bet</c>).</item>
//    ///   <item>Генерира 3 символа чрез претеглен RNG (<c>SlotSymbols.Weight</c>).</item>
//    ///   <item>Ако трите символа съвпадат → печалба = <c>BetAmount × Payout3</c> на символа.</item>
//    ///   <item>Кредитира печалбата (ако има такава) с <c>TransactionType = Win</c>.</item>
//    ///   <item>Записва <c>GameRound</c> с резултата (<c>Win</c> / <c>Lose</c>).</item>
//    /// </list>
//    /// </para>
//    /// <para>
//    /// Клиентът <b>никога</b> не изпраща резултата от барабаните – сървърът генерира всичко.
//    /// </para>
//    /// </summary>
//    public interface ISlotsService
//    {
//        /// <summary>
//        /// Завърта барабаните.
//        /// <para>
//        /// Валидации:
//        /// <list type="bullet">
//        ///   <item>Залогът трябва да е положителен.</item>
//        ///   <item>Играта трябва да съществува и да е enabled.</item>
//        ///   <item>Залогът трябва да е в min/max границите на играта.</item>
//        ///   <item>Балансът трябва да покрива залога (проверява се в <see cref="IWalletService.DebitAsync"/>).</item>
//        /// </list>
//        /// </para>
//        /// </summary>
//        /// <param name="userId">Id на потребителя (от JWT).</param>
//        /// <param name="gameCode">Код на играта (напр. <c>"slots"</c>).</param>
//        /// <param name="req">
//        /// Заявка с <c>BetAmount</c> и <c>ClientRoundId</c>.
//        /// <c>ClientRoundId</c> се използва като <c>ReferenceId</c> в транзакциите (идемпотентност).
//        /// </param>
//        Task<Result<SpinResponse>> SpinAsync(int userId, string gameCode, SpinRequest req);
//    }

//    /// <summary>
//    /// Имплементация на <see cref="ISlotsService"/>.
//    /// <para>
//    /// RNG-то е претеглено – всеки символ има <c>Weight</c> в <c>SlotSymbols</c> таблицата.
//    /// По-висок weight = по-голяма вероятност, но по-нисък payout.
//    /// </para>
//    /// </summary>
//    public class SlotsService : ISlotsService
//    {
//        private readonly AppDbContext _db;
//        private readonly IWalletService _wallet;

//        public SlotsService(AppDbContext db, IWalletService wallet)
//        {
//            _db = db;
//            _wallet = wallet;
//        }

//        /// <inheritdoc />
//        public async Task<Result<SpinResponse>> SpinAsync(int userId, string gameCode, SpinRequest req)
//        {
//            if (req.BetAmount <= 0)
//                return Result<SpinResponse>.Failure("Bet amount must be positive.", 400);

//            var game = await _db.Games
//                .Where(g => g.Code == gameCode && g.Enabled)
//                .Select(g => new { g.Id, g.MinBet, g.MaxBet })
//                .FirstOrDefaultAsync();

//            if (game is null)
//                return Result<SpinResponse>.Failure("Game not found.", 404);

//            if (req.BetAmount < game.MinBet || req.BetAmount > game.MaxBet)
//                return Result<SpinResponse>.Failure($"Bet must be between {game.MinBet} and {game.MaxBet}.", 400);

//            // 1. Debit bet
//            var debit = await _wallet.DebitAsync(userId, req.BetAmount, req.ClientRoundId);
//            if (!debit.IsSuccess)
//                return Result<SpinResponse>.Failure(debit.Error!.Message, debit.Error.StatusCode);

//            // 2. Generate reels (3 символа чрез претеглен RNG)
//            var symbols = await _db.SlotSymbols.AsNoTracking().ToListAsync();
//            var reels = new List<string>();
//            var rng = Random.Shared;

//            for (int i = 0; i < 3; i++)
//            {
//                var pick = PickWeighted(symbols, rng);
//                reels.Add(pick.Code);
//            }

//            // 3. Calculate win (само при три еднакви символа)
//            decimal winAmount = 0;
//            if (reels[0] == reels[1] && reels[1] == reels[2])
//            {
//                var sym = symbols.First(s => s.Code == reels[0]);
//                winAmount = req.BetAmount * sym.Payout3;
//            }

//            // 4. Credit win
//            decimal balance = debit.Data;
//            if (winAmount > 0)
//            {
//                var credit = await _wallet.CreditAsync(userId, winAmount, req.ClientRoundId);
//                if (!credit.IsSuccess)
//                    return Result<SpinResponse>.Failure(credit.Error!.Message, credit.Error.StatusCode);
//                balance = credit.Data;
//            }

//            // 5. Save round
//            var resultName = winAmount > 0 ? "Win" : "Lose";
//            var resultId = await _db.RoundResults
//                .Where(r => r.Name == resultName)
//                .Select(r => r.Id)
//                .FirstAsync();

//            var round = new GameRound
//            {
//                UserId = userId,
//                GameId = game.Id,
//                RoundResultId = resultId,
//                BetAmount = req.BetAmount,
//                WinAmount = winAmount,
//                DetailsJson = JsonSerializer.Serialize(new { reels }),
//                CreatedAt = DateTime.UtcNow
//            };
//            _db.GameRounds.Add(round);
//            await _db.SaveChangesAsync();

//            return Result<SpinResponse>.Success(new SpinResponse
//            {
//                RoundId = round.PublicId,
//                Reels = reels,
//                BetAmount = req.BetAmount,
//                WinAmount = winAmount,
//                Balance = balance,
//                Result = resultName
//            });
//        }

//        /// <summary>
//        /// Избира символ чрез претеглен random избор.
//        /// <para>
//        /// Вероятността за всеки символ е <c>Weight / TotalWeight</c>.
//        /// По-висок weight = по-често се пада, но payout-ът (<c>Payout3</c>) е по-нисък.
//        /// </para>
//        /// </summary>
//        /// <param name="symbols">Списък с наличните символи (от <c>SlotSymbols</c> таблицата).</param>
//        /// <param name="rng">Random инстанция (<see cref="Random.Shared"/> за thread-safety).</param>
//        /// <returns>Избраният символ.</returns>
//        private static SlotSymbol PickWeighted(List<SlotSymbol> symbols, Random rng)
//        {
//            var total = symbols.Sum(s => s.Weight);
//            var roll = (decimal)rng.NextDouble() * total;
//            decimal acc = 0;

//            foreach (var s in symbols)
//            {
//                acc += s.Weight;
//                if (roll <= acc) return s;
//            }

//            // Fallback (не трябва да се стига при коректни данни)
//            return symbols[^1];
//        }
//    }
//}

namespace OnlineCasino.Services
{
    using System.Text.Json;

    using Microsoft.EntityFrameworkCore;

    using OnlineCasino.Data;
    using OnlineCasino.Entities;
    using OnlineCasino.DTOs.Slots;
    using OnlineCasino.Models;

    /// <summary>
    /// Slots service – работи с 3, 4 или повече барабана.
    /// <para>
    /// Броят барабани се чете от <c>Game.ReelsCount</c> в базата.
    /// Payout логика:
    /// <list type="bullet">
    ///   <item>Всички барабани еднакви → <c>BetAmount × Payout{N}</c> (където N = ReelsCount).</item>
    ///   <item>N-1 еднакви (напр. 3 от 4) → <c>BetAmount × Payout{N-1}</c>.</item>
    ///   <item>По-малко → 0.</item>
    /// </list>
    /// </para>
    /// </summary>
    public interface ISlotsService
    {
        Task<Result<SpinResponse>> SpinAsync(int userId, string gameCode, SpinRequest req);
    }

    /// <summary>
    /// Имплементация на <see cref="ISlotsService"/>. Универсална за всички slot игри.
    /// </summary>
    public class SlotsService : ISlotsService
    {
        private readonly AppDbContext _db;
        private readonly IWalletService _wallet;

        public SlotsService(AppDbContext db, IWalletService wallet)
        {
            _db = db;
            _wallet = wallet;
        }

        /// <inheritdoc />
        //public async Task<Result<SpinResponse>> SpinAsync(int userId, string gameCode, SpinRequest req)
        //{
        //    if (req.BetAmount <= 0)
        //        return Result<SpinResponse>.Failure("Bet amount must be positive.", 400);

        //    var game = await _db.Games
        //        .Where(g => g.Code == gameCode && g.Enabled)
        //        .Select(g => new { g.Id, g.MinBet, g.MaxBet, g.ReelsCount })
        //        .FirstOrDefaultAsync();

        //    if (game is null)
        //        return Result<SpinResponse>.Failure("Game not found.", 404);

        //    if (game.ReelsCount < 3)
        //        return Result<SpinResponse>.Failure("This game is not a slot game.", 400);

        //    if (req.BetAmount < game.MinBet || req.BetAmount > game.MaxBet)
        //        return Result<SpinResponse>.Failure($"Bet must be between {game.MinBet} and {game.MaxBet}.", 400);

        //    // 1. Debit bet
        //    var debit = await _wallet.DebitAsync(userId, req.BetAmount, req.ClientRoundId);
        //    if (!debit.IsSuccess)
        //        return Result<SpinResponse>.Failure(debit.Error!.Message, debit.Error.StatusCode);

        //    // 2. Generate N reels (N = ReelsCount)
        //    var symbols = await _db.SlotSymbols.AsNoTracking().ToListAsync();
        //    var reels = new List<string>();
        //    var rng = Random.Shared;

        //    for (int i = 0; i < game.ReelsCount; i++)
        //    {
        //        reels.Add(PickWeighted(symbols, rng).Code);
        //    }

        //    // 3. Find best match count
        //    var grouped = reels
        //        .GroupBy(r => r)
        //        .Select(g => new { Code = g.Key, Count = g.Count() })
        //        .OrderByDescending(x => x.Count)
        //        .First();

        //    int matchCount = grouped.Count;
        //    var sym = symbols.First(s => s.Code == grouped.Code);

        //    // 4. Calculate win – само ако matchCount >= ReelsCount - 1
        //    decimal winAmount = 0;
        //    if (matchCount == game.ReelsCount)
        //    {
        //        winAmount = req.BetAmount * GetPayout(sym, game.ReelsCount);
        //    }
        //    else if (matchCount == game.ReelsCount - 1)
        //    {
        //        winAmount = req.BetAmount * GetPayout(sym, game.ReelsCount - 1);
        //    }

        //    // 5. Credit win
        //    decimal balance = debit.Data;
        //    if (winAmount > 0)
        //    {
        //        var credit = await _wallet.CreditAsync(userId, winAmount, req.ClientRoundId);
        //        if (!credit.IsSuccess)
        //            return Result<SpinResponse>.Failure(credit.Error!.Message, credit.Error.StatusCode);
        //        balance = credit.Data;
        //    }

        //    // 6. Save round
        //    var resultName = winAmount > 0 ? "Win" : "Lose";
        //    var resultId = await _db.RoundResults
        //        .Where(r => r.Name == resultName)
        //        .Select(r => r.Id)
        //        .FirstAsync();

        //    var round = new GameRound
        //    {
        //        UserId = userId,
        //        GameId = game.Id,
        //        RoundResultId = resultId,
        //        BetAmount = req.BetAmount,
        //        WinAmount = winAmount,
        //        DetailsJson = JsonSerializer.Serialize(new { reels, matchCount }),
        //        CreatedAt = DateTime.UtcNow
        //    };
        //    _db.GameRounds.Add(round);
        //    await _db.SaveChangesAsync();

        //    return Result<SpinResponse>.Success(new SpinResponse
        //    {
        //        RoundId = round.PublicId,
        //        Reels = reels,
        //        BetAmount = req.BetAmount,
        //        WinAmount = winAmount,
        //        Balance = balance,
        //        Result = resultName,
        //        MatchCount = matchCount
        //    });
        //}
        public async Task<Result<SpinResponse>> SpinAsync(int userId, string gameCode, SpinRequest req)
        {
            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine($"[SLOTS] START: userId={userId}, gameCode={gameCode}, bet={req.BetAmount}");

            if (req.BetAmount <= 0)
                return Result<SpinResponse>.Failure("Bet amount must be positive.", 400);

            var game = await _db.Games
                .Where(g => g.Code == gameCode && g.Enabled)
                .Select(g => new { g.Id, g.MinBet, g.MaxBet, g.ReelsCount })
                .FirstOrDefaultAsync();

            if (game is null)
            {
                Console.WriteLine($"[SLOTS] FAIL: Game '{gameCode}' not found");
                return Result<SpinResponse>.Failure("Game not found.", 404);
            }

            Console.WriteLine($"[SLOTS] Game found: Id={game.Id}, ReelsCount={game.ReelsCount}");

            if (game.ReelsCount < 3)
            {
                Console.WriteLine($"[SLOTS] FAIL: ReelsCount={game.ReelsCount} < 3");
                return Result<SpinResponse>.Failure("This game is not a slot game.", 400);
            }

            if (req.BetAmount < game.MinBet || req.BetAmount > game.MaxBet)
                return Result<SpinResponse>.Failure($"Bet must be between {game.MinBet} and {game.MaxBet}.", 400);

            // ─── 1. Debit ───
            var debit = await _wallet.DebitAsync(userId, req.BetAmount, req.ClientRoundId);
            if (!debit.IsSuccess)
                return Result<SpinResponse>.Failure(debit.Error!.Message, debit.Error.StatusCode);

            Console.WriteLine($"[SLOTS] DEBIT OK: balance={debit.Data}");

            // ─── 2. Generate reels ───
            var symbols = await _db.SlotSymbols.AsNoTracking().ToListAsync();
            Console.WriteLine($"[SLOTS] Loaded {symbols.Count} symbols");

            var reels = new List<string>();
            var rng = Random.Shared;

            for (int i = 0; i < game.ReelsCount; i++)
            {
                reels.Add(PickWeighted(symbols, rng).Code);
            }

            Console.WriteLine($"[SLOTS] Generated reels: [{string.Join(", ", reels)}]");

            // ─── 3. Find best match count ───
            var grouped = reels
                .GroupBy(r => r)
                .Select(g => new { Code = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .First();

            int matchCount = grouped.Count;
            var sym = symbols.First(s => s.Code == grouped.Code);

            Console.WriteLine($"[SLOTS] Best symbol: '{grouped.Code}', matchCount={matchCount}, ReelsCount={game.ReelsCount}");

            // ─── 4. Calculate win ───
            decimal winAmount = 0;
            if (matchCount == game.ReelsCount)
            {
                var multiplier = GetPayout(sym, game.ReelsCount);
                winAmount = req.BetAmount * multiplier;
                Console.WriteLine($"[SLOTS] FULL MATCH! multiplier={multiplier}, winAmount={winAmount}");
            }
            else if (matchCount == game.ReelsCount - 1)
            {
                var multiplier = GetPayout(sym, game.ReelsCount - 1);
                winAmount = req.BetAmount * multiplier;
                Console.WriteLine($"[SLOTS] PARTIAL MATCH! multiplier={multiplier}, winAmount={winAmount}");
            }
            else
            {
                Console.WriteLine($"[SLOTS] NO WIN: matchCount={matchCount} < required={game.ReelsCount - 1}");
            }

            Console.WriteLine($"[SLOTS] Total win: {winAmount}");

            // ─── 5. Credit win ───
            decimal balance = debit.Data;
            if (winAmount > 0)
            {
                var credit = await _wallet.CreditAsync(userId, winAmount, req.ClientRoundId);
                if (!credit.IsSuccess)
                    return Result<SpinResponse>.Failure(credit.Error!.Message, credit.Error.StatusCode);
                balance = credit.Data;
                Console.WriteLine($"[SLOTS] CREDIT OK: balance={balance}");
            }

            // ─── 6. Save round ───
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

            Console.WriteLine($"[SLOTS] END: result={resultName}, win={winAmount}, balance={balance}");
            Console.WriteLine("═══════════════════════════════════════");

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
        /// Връща payout множителя за даден символ според броя съвпадения.
        /// <para>
        /// Поддържа 3 и 4 барабана. За 5+ добави <c>Payout5</c> в <see cref="SlotSymbol"/>.
        /// </para>
        /// </summary>
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
        /// Претеглен random избор на символ.
        /// </summary>
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
