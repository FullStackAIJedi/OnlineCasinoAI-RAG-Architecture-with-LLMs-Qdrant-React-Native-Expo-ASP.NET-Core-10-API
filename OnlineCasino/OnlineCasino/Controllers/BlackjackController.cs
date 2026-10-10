namespace OnlineCasino.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using OnlineCasino.DTOs.Blackjack;
    using OnlineCasino.Services;

    /// <summary>
    /// Blackjack endpoints – starting a game, hit, stand, double, and state retrieval.
    /// <para>
    /// The game is stateful – the server keeps the player's and dealer's hands in <c>BlackjackSessions</c>.
    /// The client <b>never</b> sends cards – only actions (hit / stand / double).
    /// The server generates the cards, calculates the result, and settles the bet.
    /// </para>
    /// <para>
    /// All endpoints require a valid JWT token (cookie <c>access_token</c>).
    /// </para>
    /// </summary>
    [Authorize]
    [Route("api/games/blackjack")]
    public class BlackjackController : BaseApiController
    {
        private readonly IBlackjackService _bj;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackjackController"/> class.
        /// </summary>
        /// <param name="bj">The blackjack service used to run game sessions.</param>
        public BlackjackController(IBlackjackService bj) => _bj = bj;

        /// <summary>
        /// Starts a new Blackjack session – draws 2 cards each for the player and the dealer,
        /// deducts the bet from the wallet, and returns the initial state.
        /// </summary>
        /// <param name="gameCode">Game code (e.g. <c>"blackjack"</c>).</param>
        /// <param name="req">Request containing <c>BetAmount</c>.</param>
        [HttpPost("{gameCode}/start")] 
        public async Task<IActionResult> Start([FromRoute] string gameCode, [FromBody] StartBlackjackRequest req)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _bj.StartAsync(userId.Value, gameCode, req));
        }

        /// <summary>
        /// The player draws another card.
        /// <para>
        /// If the total exceeds 21 → the session ends with <c>Bust</c> (loss).
        /// If it becomes exactly 21 → the dealer plays and the session ends.
        /// Otherwise, the game continues with <c>PlayerTurn</c>.
        /// </para>
        /// <para>
        /// Does not require a body – the server decides which card to draw.
        /// </para>
        /// </summary>
        /// <param name="sessionId">The public identifier of the session (<c>PublicId</c>).</param>
        [HttpPost("sessions/{sessionId}/hit")] 
        public async Task<IActionResult> Hit([FromRoute] string sessionId)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _bj.HitAsync(userId.Value, sessionId));
        }

        /// <summary>
        /// The player stops drawing. The dealer draws cards until reaching at least 17,
        /// after which the winner is determined and the bet is settled.
        /// </summary>
        /// <param name="sessionId">The public identifier of the session.</param>
        [HttpPost("sessions/{sessionId}/stand")] 
        public async Task<IActionResult> Stand([FromRoute] string sessionId)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _bj.StandAsync(userId.Value, sessionId));
        }

        ////// <summary>
        /// Doubles the bet, draws exactly one card, and automatically ends the round (stand).
        /// <para>
        /// Only allowed on the player's first two cards.
        /// The doubled amount is deducted additionally from the wallet.
        /// </para>
        /// </summary>
        /// <param name="sessionId">The public identifier of the session.</param>
        [HttpPost("sessions/{sessionId}/double")] 
        public async Task<IActionResult> Double([FromRoute] string sessionId)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _bj.DoubleAsync(userId.Value, sessionId));
        }

        /// <summary>
        /// Returns the current session state without changing anything.
        /// <para>
        /// Useful for reconnect from React Native (e.g. after app background / foreground).
        /// The dealer's second card is shown as <c>"?"</c> until the session is finished.
        /// </para>
        /// </summary>
        /// <param name="sessionId">The public identifier of the session.</param>
        [HttpGet("sessions/{sessionId}")] 
        public async Task<IActionResult> GetState([FromRoute] string sessionId)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _bj.GetStateAsync(userId.Value, sessionId));
        }
    }
}
