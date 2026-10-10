namespace OnlineCasino.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using OnlineCasino.Services;

    /// <summary>
    /// Games endpoints – read-only list and details of available games.
    /// <para>
    /// Used by the client (lobby screen) to display the available games
    /// and their min/max bets before the player enters a specific game.
    /// </para>
    /// <para>
    /// The actual gameplay endpoints live in their own controllers:
    /// <c>SlotsController</c>, <c>RouletteController</c>, <c>BlackjackController</c>.
    /// </para>
    /// </summary>
    [Authorize]
    public class GamesController : BaseApiController
    {
        private readonly IGameService _games;

        /// <summary>
        /// Initializes a new instance of the <see cref="GamesController"/> class.
        /// </summary>
        /// <param name="games">The game service used to retrieve game data.</param>
        public GamesController(IGameService games) => _games = games;

        /// <summary>
        /// Returns all enabled games (slots, roulette, blackjack).
        /// GET /api/games
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => AsActionResult(await _games.GetAllAsync());

        /// <summary>
        /// Returns details for a specific game by its code (slots / roulette / blackjack).
        /// GET /api/games/{code}
        /// </summary>
        /// <param name="code">Game code (e.g. <c>"slots"</c>, <c>"roulette"</c>, <c>"blackjack"</c>).</param>
        [HttpGet("{code}")]
        public async Task<IActionResult> GetByCode([FromRoute] string code)
            => AsActionResult(await _games.GetByCodeAsync(code));
    }
}
