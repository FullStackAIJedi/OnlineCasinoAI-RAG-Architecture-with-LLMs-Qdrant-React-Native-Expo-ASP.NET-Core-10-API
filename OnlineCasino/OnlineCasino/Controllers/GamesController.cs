using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCasino.Services;

namespace OnlineCasino.Controllers
{
    [Authorize]
    public class GamesController : BaseApiController
    {
        private readonly IGameService _games;

        public GamesController(IGameService games) => _games = games;

        /// <summary>
        /// Връща всички enabled игри (slots, roulette, blackjack).
        /// GET /api/games
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => AsActionResult(await _games.GetAllAsync());

        /// <summary>
        /// Връща детайли за конкретна игра по code (slots / roulette / blackjack).
        /// GET /api/games/{code}
        /// </summary>
        [HttpGet("{code}")]
        public async Task<IActionResult> GetByCode([FromRoute] string code)
            => AsActionResult(await _games.GetByCodeAsync(code));
    }
}