namespace OnlineCasino.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using OnlineCasino.DTOs.Roulette;
    using OnlineCasino.Services;

    /// <summary>
    /// Roulette endpoints – placing bets, viewing current bets, and spinning the wheel.
    /// <para>
    /// The game is two-phase:
    /// <list type="number">
    ///   <item>The player places one or more bets via <c>POST /bet</c>.</item>
    ///   <item>The player spins the wheel via <c>POST /spin</c> – then the server deducts the total amount,
    ///         generates the winning number, and pays out the winnings.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Bets are kept in the <c>RouletteBets</c> table until a spin occurs.
    /// The client <b>never</b> sends the payout – the server calculates everything.
    /// </para>
    /// </summary>
    [Authorize]
    [Route("api/games/roulette")]
    public class RouletteController : BaseApiController
    {
        private readonly IRouletteService _roulette;

        /// <summary>
        /// Initializes a new instance of the <see cref="RouletteController"/> class.
        /// </summary>
        /// <param name="roulette">The roulette service used to place bets and spin the wheel.</param>
        public RouletteController(IRouletteService roulette) => _roulette = roulette;

        /// <summary>
        /// Returns the user's current unresolved bets for this game.
        /// <para>
        /// Used by React Native to render the already placed bets
        /// on the table before a spin.
        /// </para>
        /// </summary>
        /// <param name="gameCode">Game code (e.g. <c>"roulette"</c>).</param>
        [HttpGet("{gameCode}/bets")]
        public async Task<IActionResult> GetBets([FromRoute] string gameCode)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _roulette.GetCurrentBetsAsync(userId.Value));
        }

        /// <summary>
        /// Places a new bet. Can be called multiple times before a spin.
        /// <para>
        /// Validations:
        /// <list type="bullet">
        ///   <item>The bet must be positive.</item>
        ///   <item>The bet type must exist in <c>RouletteBetTypes</c>.</item>
        ///   <item>For <c>number</c> – the value must be 0–37 (37 = 00).</item>
        ///   <item>For <c>dozen</c> – the value must be 1, 2, or 3.</item>
        ///   <item>The balance must cover the current unresolved bets + the new bet.</item>
        /// </list>
        /// The money is <b>not deducted yet</b> – that happens on spin.
        /// </para>
        /// </summary>
        /// <param name="gameCode">Game code (e.g. <c>"roulette"</c>).</param>
        /// <param name="req">
        /// Request containing:
        /// <list type="bullet">
        ///   <item><c>Type</c> – <c>"red"</c>, <c>"black"</c>, <c>"even"</c>, <c>"odd"</c>,
        ///         <c>"low"</c>, <c>"high"</c>, <c>"dozen"</c>, <c>"number"</c>.</item>
        ///   <item><c>Value</c> – only for <c>number</c> (0–37, where 37 = 00) and <c>dozen</c> (1–3).</item>
        ///   <item><c>Amount</c> – the bet amount.</item>
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
        /// Spins the wheel – deducts the total amount of all current bets, generates the winning
        /// number (0–37, where 37 = 00), and pays out the winnings according to each bet's type.
        /// <para>
        /// After a successful spin, all of the user's current bets are deleted.
        /// A <c>GameRound</c> is saved with the total bet, total win, and the result
        /// (<c>Win</c> / <c>Lose</c> / <c>Push</c>).
        /// </para>
        /// <para>
        /// Payout table:
        /// <list type="bullet">
        ///   <item><c>red</c>, <c>black</c>, <c>even</c>, <c>odd</c>, <c>low</c>, <c>high</c> → 2×</item>
        ///   <item><c>dozen</c> → 3×</item>
        ///   <item><c>number</c> → 36×</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="gameCode">Game code (e.g. <c>"roulette"</c>).</param>
        /// <param name="req">
        /// Request containing <c>ClientRoundId</c> – a unique client-generated identifier
        /// used for idempotency (prevents double spins on retry).
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