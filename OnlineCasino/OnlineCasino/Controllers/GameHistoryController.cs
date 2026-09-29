namespace OnlineCasino.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using OnlineCasino.Services;

    /// <summary>
    /// История на изиграните рундове за текущия потребител.
    /// <para>
    /// Всички endpoints връщат само рундове на логнатия потребител – ownership check-ът
    /// е вграден в заявките чрез <c>userId</c>.
    /// </para>
    /// <para>
    /// Резултатите са сортирани по <c>CreatedAt</c> descending (най-новите първи)
    /// и поддържат pagination чрез <c>page</c> и <c>pageSize</c>.
    /// </para>
    /// </summary>
    [Authorize]
    [Route("api/game-history")]
    public class GameHistoryController : BaseApiController
    {
        private readonly IGameHistoryService _history;

        public GameHistoryController(IGameHistoryService history) => _history = history;

        /// <summary>
        /// Returns a paginated history of all rounds for the current user.
        /// Can be filtered by game code using <paramref name="game"/>.
        /// </summary>
        /// <param name="game">
        /// Optional filter by game code: <c>"slots"</c>, <c>"roulette"</c>, <c>"blackjack"</c>.
        /// If <c>null</c> or empty, returns all games.
        /// </param>
        /// <param name="page">Page number (starts from 1). Default: 1.</param>
        /// <param name="pageSize">Number of records per page (1–100). Default: 20.</param>
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? game = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _history.GetAsync(userId.Value, game, page, pageSize));
        }

        /// <summary>
        /// Връща пагинирана история само на Slots рундове.
        /// <para>Shorthand за <c>GET /api/game-history?game=slots</c>.</para>
        /// </summary>
        /// <param name="page">Номер на страница (започва от 1). Default: 1.</param>
        /// <param name="pageSize">Брой записи на страница (1–100). Default: 20.</param>
        [HttpGet("slots")]
        public async Task<IActionResult> GetSlots([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _history.GetAsync(userId.Value, "slots", page, pageSize));
        }

        /// <summary>
        /// Връща пагинирана история само на Roulette рундове.
        /// <para>Shorthand за <c>GET /api/game-history?game=roulette</c>.</para>
        /// </summary>
        /// <param name="page">Номер на страница (започва от 1). Default: 1.</param>
        /// <param name="pageSize">Брой записи на страница (1–100). Default: 20.</param>
        [HttpGet("roulette")]
        public async Task<IActionResult> GetRoulette([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _history.GetAsync(userId.Value, "roulette", page, pageSize));
        }

        /// <summary>
        /// Връща пагинирана история само на Blackjack рундове.
        /// <para>Shorthand за <c>GET /api/game-history?game=blackjack</c>.</para>
        /// </summary>
        /// <param name="page">Номер на страница (започва от 1). Default: 1.</param>
        /// <param name="pageSize">Брой записи на страница (1–100). Default: 20.</param>
        [HttpGet("blackjack")]
        public async Task<IActionResult> GetBlackjack([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _history.GetAsync(userId.Value, "blackjack", page, pageSize));
        }

        /// <summary>
        /// Връща детайли за конкретен рунд по неговия публичен идентификатор.
        /// <para>
        /// Ownership check – връща 404, ако рундът не съществува или не принадлежи на текущия потребител
        /// (не 403, за да не изтича информация дали даден roundId съществува).
        /// </para>
        /// </summary>
        /// <param name="roundId">Публичният идентификатор на рунда (<c>PublicId</c>).</param>
        [HttpGet("{roundId}")]
        public async Task<IActionResult> GetRound([FromRoute] string roundId)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _history.GetByRoundIdAsync(userId.Value, roundId));
        }
    }
}