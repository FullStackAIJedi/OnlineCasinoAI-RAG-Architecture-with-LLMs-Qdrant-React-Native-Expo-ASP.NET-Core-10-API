namespace OnlineCasino.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using OnlineCasino.DTOs.Slots;
    using OnlineCasino.Services;

    /// <summary>
    /// Slots endpoints – завъртане на барабаните.
    /// <para>
    /// Играта е еднофазна: играчът изпраща залог + <c>ClientRoundId</c>,
    /// сървърът удържа залога, генерира 3 символа (RNG), изчислява печалбата
    /// и я кредитира обратно в wallet-а.
    /// </para>
    /// <para>
    /// Клиентът <b>никога</b> не изпраща резултата от барабаните – сървърът генерира всичко.
    /// </para>
    /// </summary>
    [Authorize]
    [Route("api/games/slots")]
    public class SlotsController : BaseApiController
    {
        private readonly ISlotsService _slots;

        public SlotsController(ISlotsService slots) => _slots = slots;

        /// <summary>
        /// Завърта барабаните.
        /// <para>
        /// Процес:
        /// <list type="number">
        ///   <item>Валидира залога спрямо min/max на играта.</item>
        ///   <item>Удържа залога от wallet-а (с <c>TransactionType = Bet</c>).</item>
        ///   <item>Генерира 3 символа чрез претеглен RNG (<c>SlotSymbols.Weight</c>).</item>
        ///   <item>Ако трите символа съвпадат → печалба = <c>BetAmount × Payout3</c> на символа.</item>
        ///   <item>Кредитира печалбата (ако има такава) с <c>TransactionType = Win</c>.</item>
        ///   <item>Записва <c>GameRound</c> с резултата (<c>Win</c> / <c>Lose</c>).</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="gameCode">Код на играта (напр. <c>"slots"</c>).</param>
        /// <param name="req">
        /// Заявка с полета:
        /// <list type="bullet">
        ///   <item><c>BetAmount</c> – залогът (трябва да е в min/max границите).</item>
        ///   <item><c>ClientRoundId</c> – уникален идентификатор от клиента, използван за
        ///         идемпотентност и за референция в <c>Transactions.ReferenceId</c>.</item>
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