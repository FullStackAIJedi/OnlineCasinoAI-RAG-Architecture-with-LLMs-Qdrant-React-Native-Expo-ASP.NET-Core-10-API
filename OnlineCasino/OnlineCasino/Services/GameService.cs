namespace OnlineCasino.Services
{
    using Microsoft.EntityFrameworkCore;

    using OnlineCasino.Data;
    using OnlineCasino.DTOs.Games;
    using OnlineCasino.Models;

    /// <summary>
    /// Read-only service for games – list and details by code.
    /// <para>
    /// Used by the client when loading the lobby screen,
    /// to display the available games and their min/max bets.
    /// </para>
    /// <para>
    /// All queries are projected directly to <see cref="GameDto"/> –
    /// no <c>Include</c>, no loading of full entities.
    /// </para>
    /// </summary>
    public interface IGameService
    {
        /// <summary>
        /// Returns all <b>enabled</b> games (<c>Enabled == true</c>).
        /// <para>
        /// Disabled games (disabled by Admin) are not shown to the client.
        /// </para>
        /// </summary>
        /// <returns>A list of <see cref="GameDto"/> – may be empty.</returns>
        Task<Result<List<GameDto>>> GetAllAsync();

        /// <summary>
        /// Returns details for a specific game by its code.
        /// <para>
        /// Unlike <see cref="GetAllAsync"/>, this method also returns disabled games –
        /// so the client can display an appropriate message ("the game is temporarily unavailable").
        /// </para>
        /// </summary>
        /// <param name="code">Game code (e.g. <c>"slots"</c>, <c>"roulette"</c>, <c>"blackjack"</c>).</param>
        /// <returns>
        /// <see cref="GameDto"/> on success;
        /// <c>404 Not Found</c> if the code does not exist.
        /// </returns>
        Task<Result<GameDto>> GetByCodeAsync(string code);
    }

    /// <summary>
    /// Implementation of <see cref="IGameService"/>.
    /// </summary>
    public class GameService : IGameService
    {
        private readonly AppDbContext _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameService"/> class.
        /// </summary>
        /// <param name="db">The application database context.</param>
        public GameService(AppDbContext db) => _db = db;

        /// <inheritdoc />
        public async Task<Result<List<GameDto>>> GetAllAsync()
        {
            var list = await _db.Games
                .AsNoTracking()
                .Where(g => g.Enabled)
                .Select(g => new GameDto
                {
                    Id = g.Id,
                    Code = g.Code,
                    Name = g.Name,
                    Type = g.GameType.Name,
                    Enabled = g.Enabled,
                    MinBet = g.MinBet,
                    MaxBet = g.MaxBet
                })
                .ToListAsync();

            return Result<List<GameDto>>.Success(list);
        }

        /// <inheritdoc />
        public async Task<Result<GameDto>> GetByCodeAsync(string code)
        {
            var dto = await _db.Games
                .AsNoTracking()
                .Where(g => g.Code == code)
                .Select(g => new GameDto
                {
                    Id = g.Id,
                    Code = g.Code,
                    Name = g.Name,
                    Type = g.GameType.Name,
                    Enabled = g.Enabled,
                    MinBet = g.MinBet,
                    MaxBet = g.MaxBet
                })
                .FirstOrDefaultAsync();

            if (dto is null)
                return Result<GameDto>.Failure("Game not found.", 404);

            return Result<GameDto>.Success(dto);
        }
    }
}