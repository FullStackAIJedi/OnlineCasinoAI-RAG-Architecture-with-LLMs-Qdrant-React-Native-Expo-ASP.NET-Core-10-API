namespace OnlineCasino.Services
{
    using System.Text.Json;

    using Microsoft.EntityFrameworkCore;

    using OnlineCasino.Data;
    using OnlineCasino.DTOs.Blackjack;
    using OnlineCasino.Entities;
    using OnlineCasino.Models;

    /// <summary>
    /// Blackjack service – a stateful game with card drawing, hit / stand / double.
    /// <para>
    /// The server keeps the player's and dealer's hands in <c>BlackjackSessions</c>.
    /// The client <b>never</b> sends cards – only actions.
    /// </para>
    /// <para>    /// All cards are generated via <see cref="Random.Shared"/> (for demo purposes).
    /// In production, a cryptographic RNG should be used.
    /// </para>
    /// <para>
    /// Payout rules:
    /// <list type="bullet">
    ///   <item>Blackjack (21 on the first 2 cards) → 2.5× the bet.</item>
    ///   <item>Regular win → 2× the bet.</item>
    ///   <item>Push (tie) → the bet is returned (1×).</item>
    ///   <item>Loss / Bust → 0.</item>
    /// </list>
    /// </para>
    /// </summary>
    public interface IBlackjackService
    {
        /// <summary>
        /// Starts a new Blackjack session – deducts the bet, draws 2 cards each for the player
        /// and the dealer, and returns the initial state.
        /// <para>If the player immediately hits 21 (blackjack) – the round ends immediately with a 2.5× payout.</para>
        /// </summary>
        /// <param name="userId">User ID (from JWT).</param>
        /// <param name="gameCode">Game code (e.g. <c>"blackjack"</c>).</param>
        /// <param name="req">Request containing <c>BetAmount</c>.</param>
        Task<Result<BlackjackStateDto>> StartAsync(int userId, string gameCode, StartBlackjackRequest req);

        /// <summary>
        /// The player draws another card.
        /// <para>
        /// If the total exceeds 21 → Bust (loss), the session ends.
        /// If it becomes exactly 21 → the dealer plays and the session ends.
        /// Otherwise, the game continues with <c>PlayerTurn</c>.
        /// </para>
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="sessionId">Public identifier of the session.</param>
        Task<Result<BlackjackStateDto>> HitAsync(int userId, string sessionId);

        /// <summary>
        /// The player stops drawing. The dealer draws cards until reaching at least 17,
        /// after which the winner is determined and the bet is settled.
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="sessionId">Public identifier of the session.</param>
        Task<Result<BlackjackStateDto>> StandAsync(int userId, string sessionId);

        /// <summary>
        /// Doubles the bet, draws exactly one card, and automatically ends the round.
        /// <para>Only allowed on the player's first two cards.</para>
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="sessionId">Public identifier of the session.</param>
        Task<Result<BlackjackStateDto>> DoubleAsync(int userId, string sessionId);

        /// <summary>
        /// Returns the current session state without changing anything.
        /// <para>Useful for reconnect from React Native.</para>
        /// <para>The dealer's second card is shown as <c>"?"</c> until the round is finished.</para>
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="sessionId">Public identifier of the session.</param>
        Task<Result<BlackjackStateDto>> GetStateAsync(int userId, string sessionId);
    }

    /// <summary>
    /// Implementation of <see cref="IBlackjackService"/>.
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

        /// <summary>
        /// Starts a new Blackjack session – deducts the bet, draws 2 cards each for the player
        /// and the dealer, and returns the initial state.
        /// <para>If the player immediately hits 21 (blackjack) – the round ends immediately with a 2.5× payout.</para>
        /// </summary>
        /// <param name="userId">User ID (from JWT).</param>
        /// <param name="gameCode">Game code (e.g. <c>"blackjack"</c>).</param>
        /// <param name="req">Request containing <c>BetAmount</c>.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the initial <see cref="BlackjackStateDto"/> on success,
        /// or an error message and status code on failure.
        /// </returns>
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

        /// <summary>
        /// The player draws another card.
        /// <para>
        /// If the total exceeds 21 → Bust (loss), the session ends.
        /// If it becomes exactly 21 → the dealer plays and the session ends.
        /// Otherwise, the game continues with <c>PlayerTurn</c>.
        /// </para>
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="sessionId">Public identifier of the session.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the updated <see cref="BlackjackStateDto"/> on success,
        /// or an error message and status code on failure.
        /// </returns>
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

        /// <summary>
        /// The player stops drawing. The dealer draws cards until reaching at least 17,
        /// after which the winner is determined and the bet is settled.
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="sessionId">Public identifier of the session.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the final <see cref="BlackjackStateDto"/> on success,
        /// or an error message and status code on failure.
        /// </returns>
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

        /// <summary>
        /// Doubles the bet, draws exactly one card, and automatically ends the round.
        /// <para>Only allowed on the player's first two cards.</para>
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="sessionId">Public identifier of the session.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the final <see cref="BlackjackStateDto"/> on success,
        /// or an error message and status code on failure.
        /// </returns>
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

        /// <summary>
        /// Returns the current session state without changing anything.
        /// <para>Useful for reconnect from React Native.</para>
        /// <para>The dealer's second card is shown as <c>"?"</c> until the round is finished.</para>
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="sessionId">Public identifier of the session.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the current <see cref="BlackjackStateDto"/> on success,
        /// or an error message and status code on failure.
        /// </returns>
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
        /// Loads a session by <c>PublicId</c> and <c>userId</c> (ownership check).
        /// <para>Returns 404 if not found – not 403, to avoid leaking information.</para>
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="publicId">Public identifier of the session.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the <see cref="BlackjackSession"/> on success,
        /// or an error message and status code on failure.
        /// </returns>
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
        /// The dealer draws cards until reaching 17 or more (stands on soft 17).
        /// </summary>
        /// <param name="s">The current Blackjack session.</param>
        private async Task PlayDealerAsync(BlackjackSession s)
        {
            var dealer = s.DealerHand.Split(',').ToList();
            while (CalculateScore(dealer) < 17)
            {
                dealer.Add(DrawCard());
            }
            s.DealerHand = string.Join(",", dealer);
            s.DealerScore = CalculateScore(dealer);
            await Task.CompletedTask;   
        }

        /// <summary>
        /// Determines the round result: <c>Win</c> / <c>Lose</c> / <c>Push</c>.
        /// </summary>
        /// <param name="s">The current Blackjack session.</param>
        /// <returns>The result name as a string.</returns>
        private static string ResolveResult(BlackjackSession s)
        {
            if (s.PlayerScore > 21) return "Lose";
            if (s.DealerScore > 21) return "Win";
            if (s.PlayerScore > s.DealerScore) return "Win";
            if (s.PlayerScore < s.DealerScore) return "Lose";
            return "Push";
        }

        /// <summary>
        /// Saves a <c>GameRound</c> to the history with details about the player's and dealer's hands.
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="gameId">Game ID.</param>
        /// <param name="bet">The total bet amount.</param>
        /// <param name="win">The total win amount.</param>
        /// <param name="s">The current Blackjack session.</param>
        /// <param name="result">The result name (Win / Lose / Push), or null if unresolved.</param>
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

        /// <summary>Returns the ID of a status by its code (e.g. <c>"PlayerTurn"</c>).</summary>
        /// <param name="code">The status code.</param>
        /// <returns>The status ID.</returns>
        private async Task<int> StatusIdAsync(string code)
            => await _db.BlackjackStatuses.Where(s => s.Code == code).Select(s => s.Id).FirstAsync();

        /// <summary>Returns the current balance of the user.</summary>
        /// <param name="userId">User ID.</param>
        /// <returns>The user's current wallet balance.</returns>
        private async Task<decimal> GetBalanceAsync(int userId)
            => await _db.Wallets.Where(w => w.UserId == userId).Select(w => w.Balance).FirstAsync();

        /// <summary>Draws a random card (rank + suit), e.g. <c>"A♠"</c>.</summary>
        /// <returns>A string representing the drawn card.</returns>
        private static string DrawCard()
        {
            var rank = Ranks[Random.Shared.Next(Ranks.Length)];
            var suit = Suits[Random.Shared.Next(Suits.Length)];
            return rank + suit;
        }

        /// <summary>
        /// Calculates the value of a hand. An Ace counts as 11, unless that would cause a bust → 1.
        /// </summary>
        /// <param name="hand">The list of cards in the hand.</param>
        /// <returns>The total score of the hand.</returns>
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
        /// Builds a <see cref="BlackjackStateDto"/> to return to the client.
        /// <para>Hides the dealer's second card with <c>"?"</c> if the round is not finished.</para>
        /// </summary>
        /// <param name="s">The current Blackjack session.</param>
        /// <param name="balance">The user's current balance.</param>
        /// <param name="result">The result name (Win / Lose / Push), or null if unresolved.</param>
        /// <param name="winAmount">The total win amount.</param>
        /// <returns>A <see cref="BlackjackStateDto"/> representing the current state.</returns>
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
