namespace OnlineCasino.Services
{
    using Microsoft.EntityFrameworkCore;

    using OnlineCasino.Common.Models;
    using OnlineCasino.Data;
    using OnlineCasino.DTOs.History;
    using OnlineCasino.Models;

    /// <summary>
    /// History of played rounds for a given user.
    /// <para>
    /// All queries are scoped to <c>userId</c> – the ownership check is built into the
    /// <c>Where</c> clause, rather than a separate check.
    /// </para>
    /// <para>
    /// Results are sorted by <c>CreatedAt</c> descending (newest first).
    /// </para>
    /// </summary>
    public interface IGameHistoryService
    {
        /// <summary>
        /// Returns a paginated history of the user's rounds.
        /// <para>
        /// Can be filtered by game code (e.g. <c>"slots"</c>, <c>"roulette"</c>,
        /// <c>"blackjack"</c>). If <paramref name="gameCode"/> is <c>null</c> or empty –
        /// returns all games.
        /// </para>
        /// </summary>
        /// <param name="userId">User ID (from JWT).</param>
        /// <param name="gameCode">Optional filter by game code.</param>
        /// <param name="page">Page number (starts at 1). Values &lt; 1 are normalized to 1.</param>
        /// <param name="pageSize">Number of records per page. Normalized to 1–100, default 20.</param>
        /// <returns><see cref="PagedResult{T}"/> of <see cref="GameRoundDto"/>.</returns>
        Task<Result<PagedResult<GameRoundDto>>> GetAsync(int userId, string? gameCode, int page, int pageSize);

        /// <summary>
        /// Returns details for a specific round by its <c>PublicId</c>.
        /// <para>
        /// Ownership check – if the round does not exist or does not belong to the user,
        /// returns <c>404 Not Found</c> (not 403, to avoid leaking information).
        /// </para>
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="roundId">Public identifier of the round.</param>
        Task<Result<GameRoundDto>> GetByRoundIdAsync(int userId, string roundId);
    }

    /// <summary>
    /// Implementation of <see cref="IGameHistoryService"/>.
    /// <para>
    /// All queries use projection directly to DTO – no <c>Include</c>,
    /// no loading of full entities.
    /// </para>
    /// </summary>
    public class GameHistoryService : IGameHistoryService
    {
        private readonly AppDbContext _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameHistoryService"/> class.
        /// </summary>
        /// <param name="db">The application database context.</param>
        public GameHistoryService(AppDbContext db) => _db = db;

        /// <inheritdoc />
        public async Task<Result<PagedResult<GameRoundDto>>> GetAsync(int userId, string? gameCode, int page, int pageSize)
        { 
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _db.GameRounds
                .AsNoTracking()
                .Where(r => r.UserId == userId);

            if (!string.IsNullOrEmpty(gameCode))
                query = query.Where(r => r.Game.Code == gameCode);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new GameRoundDto
                {
                    RoundId = r.PublicId,
                    Game = r.Game.Code,
                    BetAmount = r.BetAmount,
                    WinAmount = r.WinAmount,
                    Result = r.RoundResult != null ? r.RoundResult.Name : null,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return Result<PagedResult<GameRoundDto>>.Success(new PagedResult<GameRoundDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                Total = total
            });
        }

        /// <inheritdoc />
        public async Task<Result<GameRoundDto>> GetByRoundIdAsync(int userId, string roundId)
        {
            var dto = await _db.GameRounds
                .AsNoTracking()
                .Where(r => r.PublicId == roundId && r.UserId == userId)
                .Select(r => new GameRoundDto
                {
                    RoundId = r.PublicId,
                    Game = r.Game.Code,
                    BetAmount = r.BetAmount,
                    WinAmount = r.WinAmount,
                    Result = r.RoundResult != null ? r.RoundResult.Name : null,
                    CreatedAt = r.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (dto is null)
                return Result<GameRoundDto>.Failure("Round not found.", 404);

            return Result<GameRoundDto>.Success(dto);
        }
    }
}