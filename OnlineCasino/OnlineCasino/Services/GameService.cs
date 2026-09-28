namespace OnlineCasino.Services
{
    using Microsoft.EntityFrameworkCore;

    using OnlineCasino.Data;
    using OnlineCasino.DTOs.Games;
    using OnlineCasino.Models;

    /// <summary>
    /// Read-only service за игрите – списък и детайли по код.
    /// <para>
    /// Използва се от клиента при зареждане на началния екран (lobby),
    /// за да покаже наличните игри и техните min/max залози.
    /// </para>
    /// <para>
    /// Всички заявки са projection директно към <see cref="GameDto"/> –
    /// без <c>Include</c>, без зареждане на цели entities.
    /// </para>
    /// </summary>
    public interface IGameService
    {
        /// <summary>
        /// Връща всички <b>включени</b> игри (<c>Enabled == true</c>).
        /// <para>
        /// Изключените игри (disable-нати от Admin) не се показват на клиента.
        /// </para>
        /// </summary>
        /// <returns>Списък от <see cref="GameDto"/> – може да е празен.</returns>
        Task<Result<List<GameDto>>> GetAllAsync();

        /// <summary>
        /// Връща детайли за конкретна игра по нейния код.
        /// <para>
        /// За разлика от <see cref="GetAllAsync"/>, този метод връща и disabled игри –
        /// за да може клиентът да покаже подходящо съобщение ("играта е временно недостъпна").
        /// </para>
        /// </summary>
        /// <param name="code">Код на играта (напр. <c>"slots"</c>, <c>"roulette"</c>, <c>"blackjack"</c>).</param>
        /// <returns>
        /// <see cref="GameDto"/> при успех;
        /// <c>404 Not Found</c> ако кодът не съществува.
        /// </returns>
        Task<Result<GameDto>> GetByCodeAsync(string code);
    }

    /// <summary>
    /// Имплементация на <see cref="IGameService"/>.
    /// </summary>
    public class GameService : IGameService
    {
        private readonly AppDbContext _db;

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