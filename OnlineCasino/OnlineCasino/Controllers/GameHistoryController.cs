namespace OnlineCasino.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using OnlineCasino.Services;

    /// <summary>
    /// History of played rounds for the current user.
    /// <para>
    /// All endpoints return only rounds belonging to the logged-in user – the ownership check
    /// is built into the queries via <c>userId</c>.
    /// </para>
    /// <para>
    /// Results are sorted by <c>CreatedAt</c> descending (newest first)
    /// and support pagination via <c>page</c> and <c>pageSize</c>.
    /// </para>
    /// </summary>
    [Authorize]
    [Route("api/game-history")]
    public class GameHistoryController : BaseApiController
    {
        private readonly IGameHistoryService _history;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameHistoryController"/> class.
        /// </summary>
        /// <param name="history">The game history service used to retrieve rounds.</param>
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
        /// Returns a paginated history of Slots rounds only.
        /// <para>Shorthand for <c>GET /api/game-history?game=slots</c>.</para>
        /// </summary>
        /// <param name="page">Page number (starts from 1). Default: 1.</param>
        /// <param name="pageSize">Number of records per page (1–100). Default: 20.</param>
        [HttpGet("slots")]
        public async Task<IActionResult> GetSlots([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _history.GetAsync(userId.Value, "slots", page, pageSize));
        }

        /// <summary>
        /// Returns a paginated history of Roulette rounds only.
        /// <para>Shorthand for <c>GET /api/game-history?game=roulette</c>.</para>
        /// </summary>
        /// <param name="page">Page number (starts from 1). Default: 1.</param>
        /// <param name="pageSize">Number of records per page (1–100). Default: 20.</param>
        [HttpGet("roulette")]
        public async Task<IActionResult> GetRoulette([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _history.GetAsync(userId.Value, "roulette", page, pageSize));
        }

        /// <summary>
        /// Returns a paginated history of Blackjack rounds only.
        /// <para>Shorthand for <c>GET /api/game-history?game=blackjack</c>.</para>
        /// </summary>
        /// <param name="page">Page number (starts from 1). Default: 1.</param>
        /// <param name="pageSize">Number of records per page (1–100). Default: 20.</param>
        [HttpGet("blackjack")]
        public async Task<IActionResult> GetBlackjack([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _history.GetAsync(userId.Value, "blackjack", page, pageSize));
        }

        /// <summary>
        /// Returns details for a specific round by its public identifier.
        /// <para>
        /// Ownership check – returns 404 if the round does not exist or does not belong to the current user
        /// (not 403, to avoid leaking information about whether a given roundId exists).
        /// </para>
        /// </summary>
        /// <param name="roundId">The public identifier of the round (<c>PublicId</c>).</param>
        [HttpGet("{roundId}")]
        public async Task<IActionResult> GetRound([FromRoute] string roundId)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _history.GetByRoundIdAsync(userId.Value, roundId));
        }
    }
}
