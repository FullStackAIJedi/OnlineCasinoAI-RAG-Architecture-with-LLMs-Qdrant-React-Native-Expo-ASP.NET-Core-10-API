namespace OnlineCasino.Services
{
    using System.Text.Json;

    using Microsoft.EntityFrameworkCore;

    using OnlineCasino.Data;
    using OnlineCasino.DTOs.Roulette;
    using OnlineCasino.Entities;
    using OnlineCasino.Models;

    /// <summary>
    /// Roulette service – двуфазна игра: залагане → spin.
    /// <para>
    /// Логика:
    /// <list type="number">
    ///   <item>Играчът поставя един или повече залози (<see cref="PlaceBetAsync"/>). Парите <b>още не се удържат</b>.</item>
    ///   <item>Играчът завърта колелото (<see cref="SpinAsync"/>). Тогава се удържа общата сума,
    ///         генерира се печелившото число и се изплащат печалбите.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Payout таблица (коефициенти):
    /// <list type="bullet">
    ///   <item><c>red</c>, <c>black</c>, <c>even</c>, <c>odd</c>, <c>low</c>, <c>high</c> → 2×</item>
    ///   <item><c>dozen</c> → 3×</item>
    ///   <item><c>number</c> → 36×</item>
    /// </list>
    /// </para>
    /// <para>
    /// Колелото е European (single zero) – 37 числа (0–36).
    /// </para>
    /// </summary>
    public interface IRouletteService
    {
        /// <summary>
        /// Връща текущите неизиграни залози на потребителя.
        /// <para>Използва се от клиента за рендериране на вече поставените залози върху масата.</para>
        /// </summary>
        /// <param name="userId">Id на потребителя (от JWT).</param>
        Task<Result<List<CurrentBetDto>>> GetCurrentBetsAsync(int userId);

        /// <summary>
        /// Поставя нов залог. Може да се вика многократно преди spin.
        /// <para>
        /// Валидации:
        /// <list type="bullet">
        ///   <item>Сумата трябва да е положителна.</item>
        ///   <item>Играта трябва да съществува и да е enabled.</item>
        ///   <item>Типът залог трябва да съществува в <c>RouletteBetTypes</c>.</item>
        ///   <item>За <c>number</c> – стойността трябва да е 0–36.</item>
        ///   <item>За <c>dozen</c> – стойността трябва да е 1, 2 или 3.</item>
        ///   <item>Балансът трябва да покрива текущите неизиграни залози + новия залог.</item>
        /// </list>
        /// </para>
        /// <para>
        /// ⚠️ Парите <b>още не се удържат</b> – това става при spin.
        /// </para>
        /// </summary>
        /// <param name="userId">Id на потребителя.</param>
        /// <param name="gameCode">Код на играта (напр. <c>"roulette"</c>).</param>
        /// <param name="req">Заявка с <c>Type</c>, <c>Value</c> (опционално) и <c>Amount</c>.</param>
        Task<Result<CurrentBetDto>> PlaceBetAsync(int userId, string gameCode, PlaceBetRequest req);

        /// <summary>
        /// Завърта колелото – удържа общата сума на всички текущи залози,
        /// генерира печелившото число (0–36) и изплаща печалбите.
        /// <para>
        /// След успешен spin всички текущи залози на потребителя се изтриват.
        /// Записва се <c>GameRound</c> с общия залог, общата печалба и резултата
        /// (<c>Win</c> / <c>Lose</c> / <c>Push</c>).
        /// </para>
        /// <para>
        /// Резултат:
        /// <list type="bullet">
        ///   <item><c>Win</c> – totalWin &gt; totalBet.</item>
        ///   <item><c>Push</c> – totalWin == totalBet (връща се залогът).</item>
        ///   <item><c>Lose</c> – totalWin &lt; totalBet.</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="userId">Id на потребителя.</param>
        /// <param name="gameCode">Код на играта.</param>
        /// <param name="req">Заявка с <c>ClientRoundId</c> за идемпотентност.</param>
        Task<Result<RouletteSpinResponse>> SpinAsync(int userId, string gameCode, SpinRouletteRequest req);
    }

    /// <summary>
    /// Имплементация на <see cref="IRouletteService"/>.
    /// <para>
    /// Използва European wheel (single zero) – 37 числа (0–36).
    /// Червените числа са фиксирани в <see cref="RedNumbers"/>; всички останали (без 0) са черни.
    /// </para>
    /// </summary>
    public class RouletteService : IRouletteService
    {
        private readonly AppDbContext _db;
        private readonly IWalletService _wallet;

        /// <summary>Червените числа на European roulette wheel.</summary>
        private static readonly int[] RedNumbers =
            { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };

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

            if (betType.Code == "number" && (req.Value is null || req.Value < 0 || req.Value > 36))
                return Result<CurrentBetDto>.Failure("Number must be between 0 and 36.", 400);

            if (betType.Code == "dozen" && (req.Value is null || req.Value < 1 || req.Value > 3))
                return Result<CurrentBetDto>.Failure("Dozen must be 1, 2 or 3.", 400);

            // Проверка за достатъчен баланс (без да теглим още)
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

        /// <inheritdoc />
        public async Task<Result<RouletteSpinResponse>> SpinAsync(int userId, string gameCode, SpinRouletteRequest req)
        {
            var game = await _db.Games
                .Where(g => g.Code == gameCode && g.Enabled)
                .Select(g => new { g.Id })
                .FirstOrDefaultAsync();

            if (game is null)
                return Result<RouletteSpinResponse>.Failure("Game not found.", 404);

            var bets = await _db.RouletteBets
                .Where(b => b.UserId == userId)
                .Select(b => new { b.Id, TypeCode = b.BetType.Code, b.NumberValue, b.Amount })
                .ToListAsync();

            if (bets.Count == 0)
                return Result<RouletteSpinResponse>.Failure("No bets placed.", 400);

            decimal totalBet = bets.Sum(b => b.Amount);

            // 1. Debit total bet
            var debit = await _wallet.DebitAsync(userId, totalBet, req.ClientRoundId);
            if (!debit.IsSuccess)
                return Result<RouletteSpinResponse>.Failure(debit.Error!.Message, debit.Error.StatusCode);

            // 2. Generate number
            int winning = Random.Shared.Next(0, 37);
            string color = winning == 0 ? "green" : (RedNumbers.Contains(winning) ? "red" : "black");

            // 3. Calculate win
            decimal totalWin = 0;
            foreach (var b in bets)
            {
                totalWin += CalculatePayout(b.TypeCode, b.NumberValue, winning, color, b.Amount);
            }

            // 4. Credit win
            decimal balance = debit.Data;
            if (totalWin > 0)
            {
                var credit = await _wallet.CreditAsync(userId, totalWin, req.ClientRoundId);
                if (!credit.IsSuccess)
                    return Result<RouletteSpinResponse>.Failure(credit.Error!.Message, credit.Error.StatusCode);
                balance = credit.Data;
            }

            // 5. Save round + clear bets
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
        /// Изчислява печалбата за един залог според типа му и печелившото число.
        /// <para>
        /// Връща <c>0</c> при загуба. Връща <c>amount × коефициент</c> при печалба.
        /// </para>
        /// </summary>
        /// <param name="type">Код на типа залог (<c>red</c>, <c>black</c>, <c>even</c>, ...).</param>
        /// <param name="value">Стойност за <c>number</c> (0–36) или <c>dozen</c> (1–3); иначе <c>null</c>.</param>
        /// <param name="winning">Печелившото число от този spin.</param>
        /// <param name="color">Цвят на печелившото число (<c>red</c>, <c>black</c>, <c>green</c>).</param>
        /// <param name="amount">Сумата на залога.</param>
        /// <returns>Печалбата (включително върнатия залог, ако е печалба).</returns>
        private static decimal CalculatePayout(string type, int? value, int winning, string color, decimal amount)
        {
            return type switch
            {
                "red" => color == "red" ? amount * 2 : 0,
                "black" => color == "black" ? amount * 2 : 0,
                "even" => winning != 0 && winning % 2 == 0 ? amount * 2 : 0,
                "odd" => winning % 2 == 1 ? amount * 2 : 0,
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