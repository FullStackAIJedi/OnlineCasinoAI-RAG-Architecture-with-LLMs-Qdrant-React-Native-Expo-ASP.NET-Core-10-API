namespace OnlineCasino.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using OnlineCasino.DTOs.Roulette;
    using OnlineCasino.Services;

    /// <summary>
    /// Roulette endpoints – залагане, преглед на текущи залози и завъртане на колелото.
    /// <para>
    /// Играта е двуфазна:
    /// <list type="number">
    ///   <item>Играчът поставя един или повече залози чрез <c>POST /bet</c>.</item>
    ///   <item>Играчът завърта колелото чрез <c>POST /spin</c> – тогава сървърът удържа общата сума,
    ///         генерира печелившото число и изплаща печалбите.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Залозите се пазят в <c>RouletteBets</c> таблицата, докато не се spin-не.
    /// Клиентът <b>никога</b> не изпраща payout – сървърът изчислява всичко.
    /// </para>
    /// </summary>
    [Authorize]
    [Route("api/games/roulette")]
    public class RouletteController : BaseApiController
    {
        private readonly IRouletteService _roulette;

        public RouletteController(IRouletteService roulette) => _roulette = roulette;

        /// <summary>
        /// Връща текущите неизиграни залози на потребителя за тази игра.
        /// <para>
        /// Използва се от React Native, за да рендерира вече поставените залози
        /// върху масата преди spin.
        /// </para>
        /// </summary>
        /// <param name="gameCode">Код на играта (напр. <c>"roulette"</c>).</param>
        [HttpGet("{gameCode}/bets")]
        public async Task<IActionResult> GetBets([FromRoute] string gameCode)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _roulette.GetCurrentBetsAsync(userId.Value));
        }

        /// <summary>
        /// Поставя нов залог. Може да се вика многократно преди spin.
        /// <para>
        /// Валидации:
        /// <list type="bullet">
        ///   <item>Залогът трябва да е положителен.</item>
        ///   <item>Типът залог трябва да съществува в <c>RouletteBetTypes</c>.</item>
        ///   <item>За <c>number</c> – стойността трябва да е 0–36.</item>
        ///   <item>За <c>dozen</c> – стойността трябва да е 1, 2 или 3.</item>
        ///   <item>Балансът трябва да покрива текущите неизиграни залози + новия залог.</item>
        /// </list>
        /// Парите <b>още не се удържат</b> – това става при spin.
        /// </para>
        /// </summary>
        /// <param name="gameCode">Код на играта (напр. <c>"roulette"</c>).</param>
        /// <param name="req">
        /// Заявка с полета:
        /// <list type="bullet">
        ///   <item><c>Type</c> – <c>"red"</c>, <c>"black"</c>, <c>"even"</c>, <c>"odd"</c>,
        ///         <c>"low"</c>, <c>"high"</c>, <c>"dozen"</c>, <c>"number"</c>.</item>
        ///   <item><c>Value</c> – само за <c>number</c> (0–36) и <c>dozen</c> (1–3).</item>
        ///   <item><c>Amount</c> – сумата на залога.</item>
        /// </list>
        /// </param>
        [HttpPost("{gameCode}/bet")]
        public async Task<IActionResult> PlaceBet([FromRoute] string gameCode, [FromBody] PlaceBetRequest req)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _roulette.PlaceBetAsync(userId.Value, gameCode, req));
        }

        /// <summary>
        /// Завърта колелото – удържа общата сума на всички текущи залози, генерира печелившото
        /// число (0–36) и изплаща печалбите според типа на всеки залог.
        /// <para>
        /// След успешен spin всички текущи залози на потребителя се изтриват.
        /// Записва се <c>GameRound</c> с общия залог, общата печалба и резултата
        /// (<c>Win</c> / <c>Lose</c> / <c>Push</c>).
        /// </para>
        /// <para>
        /// Payout таблица:
        /// <list type="bullet">
        ///   <item><c>red</c>, <c>black</c>, <c>even</c>, <c>odd</c>, <c>low</c>, <c>high</c> → 2×</item>
        ///   <item><c>dozen</c> → 3×</item>
        ///   <item><c>number</c> → 36×</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="gameCode">Код на играта (напр. <c>"roulette"</c>).</param>
        /// <param name="req">
        /// Заявка с <c>ClientRoundId</c> – уникален идентификатор от клиента,
        /// използван за идемпотентност (предпазва от двойно завъртане при retry).
        /// </param>
        [HttpPost("{gameCode}/spin")]
        public async Task<IActionResult> Spin([FromRoute] string gameCode, [FromBody] SpinRouletteRequest req)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _roulette.SpinAsync(userId.Value, gameCode, req));
        }
    }
}