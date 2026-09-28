namespace OnlineCasino.Services
{
    using System.Text.Json;

    using Microsoft.EntityFrameworkCore;

    using OnlineCasino.Data;
    using OnlineCasino.DTOs.Blackjack;
    using OnlineCasino.Entities;
    using OnlineCasino.Models;

    /// <summary>
    /// Blackjack service – stateful игра с теглене на карти, hit / stand / double.
    /// <para>
    /// Сървърът пази ръцете на играча и дилъра в <c>BlackjackSessions</c>.
    /// Клиентът <b>никога</b> не изпраща карти – само действия.
    /// </para>
    /// <para>
    /// Всички карти се генерират чрез <see cref="Random.Shared"/> (за демо).
    /// В production трябва да е cryptographic RNG.
    /// </para>
    /// <para>
    /// Payout правила:
    /// <list type="bullet">
    ///   <item>Blackjack (21 на първите 2 карти) → 2.5× залога.</item>
    ///   <item>Обикновена победа → 2× залога.</item>
    ///   <item>Push (равенство) → връща се залогът (1×).</item>
    ///   <item>Загуба / Bust → 0.</item>
    /// </list>
    /// </para>
    /// </summary>
    public interface IBlackjackService
    {
        /// <summary>
        /// Стартира нова Blackjack сесия – удържа залога, тегли по 2 карти на играча и дилъра
        /// и връща началното състояние.
        /// <para>Ако играчът веднага направи 21 (blackjack) – рундът приключва веднага с печалба 2.5×.</para>
        /// </summary>
        /// <param name="userId">Id на потребителя (от JWT).</param>
        /// <param name="gameCode">Код на играта (напр. <c>"blackjack"</c>).</param>
        /// <param name="req">Заявка с <c>BetAmount</c>.</param>
        Task<Result<BlackjackStateDto>> StartAsync(int userId, string gameCode, StartBlackjackRequest req);

        /// <summary>
        /// Играчът тегли още една карта.
        /// <para>
        /// Ако резултатът надхвърли 21 → Bust (загуба), сесията приключва.
        /// Ако стане точно 21 → дилърът играе и сесията приключва.
        /// Иначе играта продължава с <c>PlayerTurn</c>.
        /// </para>
        /// </summary>
        /// <param name="userId">Id на потребителя.</param>
        /// <param name="sessionId">Публичен идентификатор на сесията.</param>
        Task<Result<BlackjackStateDto>> HitAsync(int userId, string sessionId);

        /// <summary>
        /// Играчът спира да тегли. Дилърът тегли карти, докато достигне поне 17,
        /// след което се определя победителят и се сетълва залога.
        /// </summary>
        Task<Result<BlackjackStateDto>> StandAsync(int userId, string sessionId);

        /// <summary>
        /// Удвоява залога, тегли точно една карта и автоматично приключва рунда.
        /// <para>Позволено само на първите две карти на играча.</para>
        /// </summary>
        Task<Result<BlackjackStateDto>> DoubleAsync(int userId, string sessionId);

        /// <summary>
        /// Връща текущото състояние на сесията без да променя нищо.
        /// <para>Полезно при reconnect от React Native.</para>
        /// <para>Втората карта на дилъра се показва като <c>"?"</c>, докато рундът не приключи.</para>
        /// </summary>
        Task<Result<BlackjackStateDto>> GetStateAsync(int userId, string sessionId);
    }

    /// <summary>
    /// Имплементация на <see cref="IBlackjackService"/>.
    /// </summary>
    public class BlackjackService : IBlackjackService
    {
        private readonly AppDbContext _db;
        private readonly IWalletService _wallet;

        private static readonly string[] Suits = { "♠", "♥", "♦", "♣" };
        private static readonly string[] Ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

        public BlackjackService(AppDbContext db, IWalletService wallet)
        {
            _db = db;
            _wallet = wallet;
        }

        /// <inheritdoc />
        public async Task<Result<BlackjackStateDto>> StartAsync(int userId, string gameCode, StartBlackjackRequest req)
        {
            if (req.BetAmount <= 0)
                return Result<BlackjackStateDto>.Failure("Bet amount must be positive.", 400);

            var game = await _db.Games
                .Where(g => g.Code == gameCode && g.Enabled)
                .Select(g => new { g.Id, g.MinBet, g.MaxBet })
                .FirstOrDefaultAsync();

            if (game is null) return Result<BlackjackStateDto>.Failure("Game not found.", 404);

            if (req.BetAmount < game.MinBet || req.BetAmount > game.MaxBet)
                return Result<BlackjackStateDto>.Failure($"Bet must be between {game.MinBet} and {game.MaxBet}.", 400);

            var debit = await _wallet.DebitAsync(userId, req.BetAmount, Guid.NewGuid().ToString("N"));
            if (!debit.IsSuccess)
                return Result<BlackjackStateDto>.Failure(debit.Error!.Message, debit.Error.StatusCode);

            var player = new List<string> { DrawCard(), DrawCard() };
            var dealer = new List<string> { DrawCard(), DrawCard() };

            var playerScore = CalculateScore(player);
            var dealerScore = CalculateScore(dealer);

            var statusCode = "PlayerTurn";
            string? result = null;
            decimal winAmount = 0;

            if (playerScore == 21)
            {
                statusCode = "Finished";
                result = "Win";
                winAmount = req.BetAmount * 2.5m;  
            }

            var statusId = await _db.BlackjackStatuses
                .Where(s => s.Code == statusCode)
                .Select(s => s.Id)
                .FirstAsync();

            var session = new BlackjackSession
            {
                UserId = userId,
                GameId = game.Id,           
                BetAmount = req.BetAmount,
                PlayerHand = string.Join(",", player),
                DealerHand = string.Join(",", dealer),
                PlayerScore = playerScore,
                DealerScore = dealerScore,
                StatusId = statusId,
                CreatedAt = DateTime.UtcNow
            };
            _db.BlackjackSessions.Add(session);
            await _db.SaveChangesAsync();

            decimal balance = debit.Data;

            if (winAmount > 0)
            {
                var credit = await _wallet.CreditAsync(userId, winAmount, session.PublicId);
                if (credit.IsSuccess) balance = credit.Data;
                session.FinishedAt = DateTime.UtcNow;
                await SaveRoundAsync(userId, game.Id, req.BetAmount, winAmount, session, result);
                await _db.SaveChangesAsync();
            }

            return Result<BlackjackStateDto>.Success(BuildState(session, balance, result, winAmount));
        }

        /// <inheritdoc />
        public async Task<Result<BlackjackStateDto>> HitAsync(int userId, string sessionId)
        {
            var session = await LoadSessionAsync(userId, sessionId);
            if (!session.IsSuccess)
                return Result<BlackjackStateDto>.Failure(session.Error!.Message, session.Error.StatusCode);

            var s = session.Data!;
            var status = await _db.BlackjackStatuses
                .Where(x => x.Id == s.StatusId)
                .Select(x => x.Code)
                .FirstAsync();

            if (status != "PlayerTurn")
                return Result<BlackjackStateDto>.Failure("Session not in player turn.", 400);

            var player = s.PlayerHand.Split(',').ToList();
            player.Add(DrawCard());
            s.PlayerHand = string.Join(",", player);
            s.PlayerScore = CalculateScore(player);

            decimal balance = await GetBalanceAsync(userId);
            string? result = null;
            decimal winAmount = 0;

            if (s.PlayerScore > 21)
            {
                s.StatusId = await StatusIdAsync("Bust");
                s.FinishedAt = DateTime.UtcNow;
                result = "Lose";
                await SaveRoundAsync(userId, s.GameId, s.BetAmount, 0, s, result);
            }
            else if (s.PlayerScore == 21)
            {
                await PlayDealerAsync(s);
                result = ResolveResult(s);
                winAmount = result == "Win" ? s.BetAmount * 2 : (result == "Push" ? s.BetAmount : 0);
                if (winAmount > 0)
                {
                    var credit = await _wallet.CreditAsync(userId, winAmount, s.PublicId);
                    if (credit.IsSuccess) balance = credit.Data;
                }
                s.StatusId = await StatusIdAsync("Finished");
                s.FinishedAt = DateTime.UtcNow;
                await SaveRoundAsync(userId, s.GameId, s.BetAmount, winAmount, s, result);
            }

            await _db.SaveChangesAsync();
            return Result<BlackjackStateDto>.Success(BuildState(s, balance, result, winAmount));
        }

        /// <inheritdoc />
        public async Task<Result<BlackjackStateDto>> StandAsync(int userId, string sessionId)
        {
            var session = await LoadSessionAsync(userId, sessionId);
            if (!session.IsSuccess)
                return Result<BlackjackStateDto>.Failure(session.Error!.Message, session.Error.StatusCode);

            var s = session.Data!;
            var status = await _db.BlackjackStatuses
                .Where(x => x.Id == s.StatusId)
                .Select(x => x.Code)
                .FirstAsync();

            if (status != "PlayerTurn")
                return Result<BlackjackStateDto>.Failure("Session not in player turn.", 400);

            await PlayDealerAsync(s);
            var result = ResolveResult(s);
            decimal winAmount = result == "Win" ? s.BetAmount * 2 : (result == "Push" ? s.BetAmount : 0);

            decimal balance = await GetBalanceAsync(userId);
            if (winAmount > 0)
            {
                var credit = await _wallet.CreditAsync(userId, winAmount, s.PublicId);
                if (credit.IsSuccess) balance = credit.Data;
            }

            s.StatusId = await StatusIdAsync("Finished");
            s.FinishedAt = DateTime.UtcNow;
            await SaveRoundAsync(userId, s.GameId, s.BetAmount, winAmount, s, result);
            await _db.SaveChangesAsync();

            return Result<BlackjackStateDto>.Success(BuildState(s, balance, result, winAmount));
        }

        /// <inheritdoc />
        public async Task<Result<BlackjackStateDto>> DoubleAsync(int userId, string sessionId)
        {
            var session = await LoadSessionAsync(userId, sessionId);
            if (!session.IsSuccess)
                return Result<BlackjackStateDto>.Failure(session.Error!.Message, session.Error.StatusCode);

            var s = session.Data!;
            var status = await _db.BlackjackStatuses
                .Where(x => x.Id == s.StatusId)
                .Select(x => x.Code)
                .FirstAsync();

            if (status != "PlayerTurn")
                return Result<BlackjackStateDto>.Failure("Session not in player turn.", 400);

            if (s.PlayerHand.Split(',').Length != 2)
                return Result<BlackjackStateDto>.Failure("Double only allowed on first two cards.", 400);

            var extraDebit = await _wallet.DebitAsync(userId, s.BetAmount, s.PublicId);
            if (!extraDebit.IsSuccess)
                return Result<BlackjackStateDto>.Failure(extraDebit.Error!.Message, extraDebit.Error.StatusCode);

            s.BetAmount *= 2;

            var player = s.PlayerHand.Split(',').ToList();
            player.Add(DrawCard());
            s.PlayerHand = string.Join(",", player);
            s.PlayerScore = CalculateScore(player);

            await PlayDealerAsync(s);
            var result = ResolveResult(s);
            decimal winAmount = result == "Win" ? s.BetAmount * 2 : (result == "Push" ? s.BetAmount : 0);

            decimal balance = extraDebit.Data;
            if (winAmount > 0)
            {
                var credit = await _wallet.CreditAsync(userId, winAmount, s.PublicId);
                if (credit.IsSuccess) balance = credit.Data;
            }

            s.StatusId = await StatusIdAsync("Finished");
            s.FinishedAt = DateTime.UtcNow;
            await SaveRoundAsync(userId, s.GameId, s.BetAmount, winAmount, s, result);
            await _db.SaveChangesAsync();

            return Result<BlackjackStateDto>.Success(BuildState(s, balance, result, winAmount));
        }

        /// <inheritdoc />
        public async Task<Result<BlackjackStateDto>> GetStateAsync(int userId, string sessionId)
        {
            var session = await LoadSessionAsync(userId, sessionId);
            if (!session.IsSuccess)
                return Result<BlackjackStateDto>.Failure(session.Error!.Message, session.Error.StatusCode);

            var balance = await GetBalanceAsync(userId);
            return Result<BlackjackStateDto>.Success(BuildState(session.Data!, balance, null, 0));
        }

        // --- helpers ---

        /// <summary>
        /// Зарежда сесия по <c>PublicId</c> и <c>userId</c> (ownership check).
        /// <para>Връща 404 при липса – не 403, за да не изтича информация.</para>
        /// </summary>
        private async Task<Result<BlackjackSession>> LoadSessionAsync(int userId, string publicId)
        {
            var s = await _db.BlackjackSessions
                .Where(x => x.PublicId == publicId && x.UserId == userId)
                .Select(x => new BlackjackSession
                {
                    Id = x.Id,
                    PublicId = x.PublicId,
                    UserId = x.UserId,
                    GameId = x.GameId,           
                    BetAmount = x.BetAmount,
                    PlayerHand = x.PlayerHand,
                    DealerHand = x.DealerHand,
                    PlayerScore = x.PlayerScore,
                    DealerScore = x.DealerScore,
                    StatusId = x.StatusId,
                    CreatedAt = x.CreatedAt,
                    FinishedAt = x.FinishedAt
                })
                .FirstOrDefaultAsync();

            if (s is null) return Result<BlackjackSession>.Failure("Session not found.", 404);
            return Result<BlackjackSession>.Success(s);
        }

        /// <summary>
        /// Дилърът тегли карти, докато достигне 17 или повече (stand на soft 17).
        /// </summary>
        private async Task PlayDealerAsync(BlackjackSession s)
        {
            var dealer = s.DealerHand.Split(',').ToList();
            while (CalculateScore(dealer) < 17)
            {
                dealer.Add(DrawCard());
            }
            s.DealerHand = string.Join(",", dealer);
            s.DealerScore = CalculateScore(dealer);
            await Task.CompletedTask;  // методът е async за консистентност
        }

        /// <summary>
        /// Определя резултата от рунда: <c>Win</c> / <c>Lose</c> / <c>Push</c>.
        /// </summary>
        private static string ResolveResult(BlackjackSession s)
        {
            if (s.PlayerScore > 21) return "Lose";
            if (s.DealerScore > 21) return "Win";
            if (s.PlayerScore > s.DealerScore) return "Win";
            if (s.PlayerScore < s.DealerScore) return "Lose";
            return "Push";
        }

        /// <summary>
        /// Записва <c>GameRound</c> в историята с детайли за ръцете на играча и дилъра.
        /// </summary>
        private async Task SaveRoundAsync(int userId, int gameId, decimal bet, decimal win, BlackjackSession s, string? result)
        {
            var resultId = await _db.RoundResults
                .Where(r => r.Name == result)
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            _db.GameRounds.Add(new GameRound
            {
                UserId = userId,
                GameId = gameId,
                RoundResultId = resultId == 0 ? null : resultId,
                BetAmount = bet,
                WinAmount = win,
                DetailsJson = JsonSerializer.Serialize(new
                {
                    player = s.PlayerHand.Split(','),
                    dealer = s.DealerHand.Split(','),
                    playerScore = s.PlayerScore,
                    dealerScore = s.DealerScore
                }),
                CreatedAt = DateTime.UtcNow
            });
        }

        /// <summary>Връща Id на статус по код (напр. <c>"PlayerTurn"</c>).</summary>
        private async Task<int> StatusIdAsync(string code)
            => await _db.BlackjackStatuses.Where(s => s.Code == code).Select(s => s.Id).FirstAsync();

        /// <summary>Връща текущия баланс на потребителя.</summary>
        private async Task<decimal> GetBalanceAsync(int userId)
            => await _db.Wallets.Where(w => w.UserId == userId).Select(w => w.Balance).FirstAsync();

        /// <summary>Тегли случайна карта (ранг + боя), напр. <c>"A♠"</c>.</summary>
        private static string DrawCard()
        {
            var rank = Ranks[Random.Shared.Next(Ranks.Length)];
            var suit = Suits[Random.Shared.Next(Suits.Length)];
            return rank + suit;
        }

        /// <summary>
        /// Изчислява стойността на ръка. Ace се брои като 11, освен ако не води до bust → 1.
        /// </summary>
        private static int CalculateScore(List<string> hand)
        {
            int total = 0;
            int aces = 0;
            foreach (var card in hand)
            {
                var rank = card[..^1];
                if (rank == "A") { aces++; total += 11; }
                else if (rank is "J" or "Q" or "K") total += 10;
                else total += int.Parse(rank);
            }
            while (total > 21 && aces > 0) { total -= 10; aces--; }
            return total;
        }

        /// <summary>
        /// Сглобява <see cref="BlackjackStateDto"/> за връщане към клиента.
        /// <para>Скрива втората карта на дилъра с <c>"?"</c>, ако рундът не е приключил.</para>
        /// </summary>
        private static BlackjackStateDto BuildState(BlackjackSession s, decimal balance, string? result, decimal winAmount)
        {
            var player = s.PlayerHand.Split(',').ToList();
            var dealer = s.DealerHand.Split(',').ToList();

            var dealerVisible = s.FinishedAt is null && result is null
                ? new List<string> { dealer[0], "?" }
                : dealer;

            return new BlackjackStateDto
            {
                SessionId = s.PublicId,
                PlayerHand = player,
                DealerHand = dealerVisible,
                PlayerScore = s.PlayerScore,
                DealerScore = s.FinishedAt is null ? 0 : s.DealerScore,
                Status = s.FinishedAt is null ? "PlayerTurn" : "Finished",
                BetAmount = s.BetAmount,
                WinAmount = winAmount,
                Result = result,
                Balance = balance
            };
        }
    }
}
