namespace OnlineCasino.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using OnlineCasino.DTOs.Slots;
    using OnlineCasino.Services;

    /// <summary>
    /// Slots endpoints – spinning the reels.
    /// <para>
    /// The game is single-phase: the player sends a bet + <c>ClientRoundId</c>,
    /// the server deducts the bet, generates the reels (RNG), calculates the win,
    /// and credits it back to the wallet.
    /// </para>
    /// <para>
    /// The client <b>never</b> sends the reel outcome – the server generates everything.
    /// </para>
    /// </summary>
    [Authorize]
    [Route("api/games/slots")]
    public class SlotsController : BaseApiController
    {
        private readonly ISlotsService _slots;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlotsController"/> class.
        /// </summary>
        /// <param name="slots">The slots service used to execute spins.</param>
        public SlotsController(ISlotsService slots) => _slots = slots;

        /// <summary>
        /// Spins the reels.
        /// <para>
        /// Process:
        /// <list type="number">
        ///   <item>Validates the bet against the game's min/max.</item>
        ///   <item>Deducts the bet from the wallet (with <c>TransactionType = Bet</c>).</item>
        ///   <item>Generates the reels via weighted RNG (<c>SlotSymbols.Weight</c>).</item>
        ///   <item>If all reels match → win = <c>BetAmount × Payout{N}</c> for the symbol
        ///         (where N = ReelsCount, or N-1 for a partial match).</item>
        ///   <item>Credits the win (if any) with <c>TransactionType = Win</c>.</item>
        ///   <item>Saves a <c>GameRound</c> with the result (<c>Win</c> / <c>Lose</c>).</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="gameCode">Game code (e.g. <c>"slots"</c> or <c>"slots4"</c>).</param>
        /// <param name="req">
        /// Request containing:
        /// <list type="bullet">
        ///   <item><c>BetAmount</c> – the bet (must be within the min/max bounds).</item>
        ///   <item><c>ClientRoundId</c> – a unique client-generated identifier used for
        ///         idempotency and as a reference in <c>Transactions.ReferenceId</c>.</item>
        /// </list>
        /// </param>
        [HttpPost("{gameCode}/spin")]
        public async Task<IActionResult> Spin([FromRoute] string gameCode, [FromBody] SpinRequest req)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });

            return AsActionResult(await _slots.SpinAsync(userId.Value, gameCode, req));
        }
    }
}