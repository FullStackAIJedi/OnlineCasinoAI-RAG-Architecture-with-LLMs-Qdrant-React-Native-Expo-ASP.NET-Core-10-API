namespace OnlineCasino.Services
{
    using System.Text.Json;

    using Microsoft.EntityFrameworkCore;

    using OnlineCasino.Data;
    using OnlineCasino.Entities;

    using OnlineCasino.DTOs.Slots;
    using OnlineCasino.Models;

    /// <summary>
    /// Slots service – еднофазна игра: spin → резултат.
    /// <para>
    /// Логика:
    /// <list type="number">
    ///   <item>Удържа залога от wallet-а (<c>TransactionType = Bet</c>).</item>
    ///   <item>Генерира 3 символа чрез претеглен RNG (<c>SlotSymbols.Weight</c>).</item>
    ///   <item>Ако трите символа съвпадат → печалба = <c>BetAmount × Payout3</c> на символа.</item>
    ///   <item>Кредитира печалбата (ако има такава) с <c>TransactionType = Win</c>.</item>
    ///   <item>Записва <c>GameRound</c> с резултата (<c>Win</c> / <c>Lose</c>).</item>
    /// </list>
    /// </para>
    /// <para>
    /// Клиентът <b>никога</b> не изпраща резултата от барабаните – сървърът генерира всичко.
    /// </para>
    /// </summary>
    public interface ISlotsService
    {
        /// <summary>
        /// Завърта барабаните.
        /// <para>
        /// Валидации:
        /// <list type="bullet">
        ///   <item>Залогът трябва да е положителен.</item>
        ///   <item>Играта трябва да съществува и да е enabled.</item>
        ///   <item>Залогът трябва да е в min/max границите на играта.</item>
        ///   <item>Балансът трябва да покрива залога (проверява се в <see cref="IWalletService.DebitAsync"/>).</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="userId">Id на потребителя (от JWT).</param>
        /// <param name="gameCode">Код на играта (напр. <c>"slots"</c>).</param>
        /// <param name="req">
        /// Заявка с <c>BetAmount</c> и <c>ClientRoundId</c>.
        /// <c>ClientRoundId</c> се използва като <c>ReferenceId</c> в транзакциите (идемпотентност).
        /// </param>
        Task<Result<SpinResponse>> SpinAsync(int userId, string gameCode, SpinRequest req);
    }

    /// <summary>
    /// Имплементация на <see cref="ISlotsService"/>.
    /// <para>
    /// RNG-то е претеглено – всеки символ има <c>Weight</c> в <c>SlotSymbols</c> таблицата.
    /// По-висок weight = по-голяма вероятност, но по-нисък payout.
    /// </para>
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
        public async Task<Result<SpinResponse>> SpinAsync(int userId, string gameCode, SpinRequest req)
        {
            if (req.BetAmount <= 0)
                return Result<SpinResponse>.Failure("Bet amount must be positive.", 400);

            var game = await _db.Games
                .Where(g => g.Code == gameCode && g.Enabled)
                .Select(g => new { g.Id, g.MinBet, g.MaxBet })
                .FirstOrDefaultAsync();

            if (game is null)
                return Result<SpinResponse>.Failure("Game not found.", 404);

            if (req.BetAmount < game.MinBet || req.BetAmount > game.MaxBet)
                return Result<SpinResponse>.Failure($"Bet must be between {game.MinBet} and {game.MaxBet}.", 400);

            // 1. Debit bet
            var debit = await _wallet.DebitAsync(userId, req.BetAmount, req.ClientRoundId);
            if (!debit.IsSuccess)
                return Result<SpinResponse>.Failure(debit.Error!.Message, debit.Error.StatusCode);

            // 2. Generate reels (3 символа чрез претеглен RNG)
            var symbols = await _db.SlotSymbols.AsNoTracking().ToListAsync();
            var reels = new List<string>();
            var rng = Random.Shared;

            for (int i = 0; i < 3; i++)
            {
                var pick = PickWeighted(symbols, rng);
                reels.Add(pick.Code);
            }

            // 3. Calculate win (само при три еднакви символа)
            decimal winAmount = 0;
            if (reels[0] == reels[1] && reels[1] == reels[2])
            {
                var sym = symbols.First(s => s.Code == reels[0]);
                winAmount = req.BetAmount * sym.Payout3;
            }

            // 4. Credit win
            decimal balance = debit.Data;
            if (winAmount > 0)
            {
                var credit = await _wallet.CreditAsync(userId, winAmount, req.ClientRoundId);
                if (!credit.IsSuccess)
                    return Result<SpinResponse>.Failure(credit.Error!.Message, credit.Error.StatusCode);
                balance = credit.Data;
            }

            // 5. Save round
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
                DetailsJson = JsonSerializer.Serialize(new { reels }),
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
                Result = resultName
            });
        }

        /// <summary>
        /// Избира символ чрез претеглен random избор.
        /// <para>
        /// Вероятността за всеки символ е <c>Weight / TotalWeight</c>.
        /// По-висок weight = по-често се пада, но payout-ът (<c>Payout3</c>) е по-нисък.
        /// </para>
        /// </summary>
        /// <param name="symbols">Списък с наличните символи (от <c>SlotSymbols</c> таблицата).</param>
        /// <param name="rng">Random инстанция (<see cref="Random.Shared"/> за thread-safety).</param>
        /// <returns>Избраният символ.</returns>
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

            // Fallback (не трябва да се стига при коректни данни)
            return symbols[^1];
        }
    }
}