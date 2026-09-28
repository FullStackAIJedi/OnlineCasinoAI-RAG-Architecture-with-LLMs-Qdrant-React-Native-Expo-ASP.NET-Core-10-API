namespace OnlineCasino.Services
{
    using Microsoft.EntityFrameworkCore;

    using OnlineCasino.Common.Models;
    using OnlineCasino.Data;
    using OnlineCasino.DTOs.History;
    using OnlineCasino.Models;

    /// <summary>
    /// История на изиграните рундове за даден потребител.
    /// <para>
    /// Всички заявки са ограничени до <c>userId</c> – ownership check-ът е вграден в
    /// <c>Where</c> клаузата, а не в отделна проверка.
    /// </para>
    /// <para>
    /// Резултатите са сортирани по <c>CreatedAt</c> descending (най-новите първи).
    /// </para>
    /// </summary>
    public interface IGameHistoryService
    {
        /// <summary>
        /// Връща пагинирана история на рундовете на потребителя.
        /// <para>
        /// Може да се филтрира по код на игра (напр. <c>"slots"</c>, <c>"roulette"</c>,
        /// <c>"blackjack"</c>). Ако <paramref name="gameCode"/> е <c>null</c> или празен –
        /// връща всички игри.
        /// </para>
        /// </summary>
        /// <param name="userId">Id на потребителя (от JWT).</param>
        /// <param name="gameCode">Опционален филтър по код на игра.</param>
        /// <param name="page">Номер на страница (започва от 1). Стойности &lt; 1 се нормализират към 1.</param>
        /// <param name="pageSize">Брой записи на страница. Нормализира се към 1–100, default 20.</param>
        /// <returns><see cref="PagedResult{T}"/> от <see cref="GameRoundDto"/>.</returns>
        Task<Result<PagedResult<GameRoundDto>>> GetAsync(int userId, string? gameCode, int page, int pageSize);

        /// <summary>
        /// Връща детайли за конкретен рунд по неговия <c>PublicId</c>.
        /// <para>
        /// Ownership check – ако рундът не съществува или не принадлежи на потребителя,
        /// връща <c>404 Not Found</c> (не 403, за да не изтича информация).
        /// </para>
        /// </summary>
        /// <param name="userId">Id на потребителя.</param>
        /// <param name="roundId">Публичен идентификатор на рунда.</param>
        Task<Result<GameRoundDto>> GetByRoundIdAsync(int userId, string roundId);
    }

    /// <summary>
    /// Имплементация на <see cref="IGameHistoryService"/>.
    /// <para>
    /// Всички заявки използват projection директно към DTO – без <c>Include</c>,
    /// без зареждане на цели entities.
    /// </para>
    /// </summary>
    public class GameHistoryService : IGameHistoryService
    {
        private readonly AppDbContext _db;

        public GameHistoryService(AppDbContext db) => _db = db;

        /// <inheritdoc />
        public async Task<Result<PagedResult<GameRoundDto>>> GetAsync(int userId, string? gameCode, int page, int pageSize)
        {
            // Нормализация на пагинацията
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