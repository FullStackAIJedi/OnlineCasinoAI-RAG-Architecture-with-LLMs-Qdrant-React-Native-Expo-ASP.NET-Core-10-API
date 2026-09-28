namespace OnlineCasino.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using OnlineCasino.DTOs.Blackjack;
    using OnlineCasino.Services;

    /// <summary>
    /// Blackjack endpoints – стартиране на игра, hit, stand, double и проверка на състояние.
    /// <para>
    /// Играта е stateful – сървърът пази ръцете на играча и дилъра в <c>BlackjackSessions</c>.
    /// Клиентът <b>никога</b> не изпраща карти – само действия (hit / stand / double).
    /// Сървърът генерира картите, изчислява резултата и сетълва залога.
    /// </para>
    /// <para>
    /// Всички endpoints изискват валиден JWT токен (cookie <c>access_token</c>).
    /// </para>
    /// </summary>
    [Authorize]
    [Route("api/games/blackjack")]
    public class BlackjackController : BaseApiController
    {
        private readonly IBlackjackService _bj;

        public BlackjackController(IBlackjackService bj) => _bj = bj;

        /// <summary>
        /// Стартира нова Blackjack сесия – тегли по 2 карти на играча и дилъра,
        /// удържа залога от wallet-а и връща началното състояние.
        /// </summary>
        /// <param name="gameCode">Код на играта (напр. <c>"blackjack"</c>).</param>
        /// <param name="req">Заявка съдържаща <c>BetAmount</c>.</param>
        /// <response code="200">Сесията е стартирана успешно. Връща <c>BlackjackStateDto</c>.</response>
        /// <response code="400">Невалиден залог (извън min/max) или неположителна сума.</response>
        /// <response code="401">Липсва валиден JWT токен.</response>
        /// <response code="404">Играта не е намерена или е изключена.</response>
        /// <response code="500">Неочаквана грешка.</response>
        [HttpPost("{gameCode}/start")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Start([FromRoute] string gameCode, [FromBody] StartBlackjackRequest req)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _bj.StartAsync(userId.Value, gameCode, req));
        }

        /// <summary>
        /// Играчът тегли още една карта.
        /// <para>
        /// Ако резултатът надхвърли 21 → сесията приключва с <c>Bust</c> (загуба).
        /// Ако стане точно 21 → дилърът играе и сесията приключва.
        /// Иначе играта продължава с <c>PlayerTurn</c>.
        /// </para>
        /// <para>
        /// Не изисква body – сървърът сам решава коя карта да изтегли.
        /// </para>
        /// </summary>
        /// <param name="sessionId">Публичният идентификатор на сесията (<c>PublicId</c>).</param>
        /// <response code="200">Картата е изтеглена, връща обновено <c>BlackjackStateDto</c>.</response>
        /// <response code="400">Сесията не е в <c>PlayerTurn</c> (вече е приключила).</response>
        /// <response code="401">Липсва валиден JWT токен.</response>
        /// <response code="404">Сесията не е намерена или не принадлежи на текущия потребител.</response>
        /// <response code="500">Неочаквана грешка.</response>
        [HttpPost("sessions/{sessionId}/hit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Hit([FromRoute] string sessionId)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _bj.HitAsync(userId.Value, sessionId));
        }

        /// <summary>
        /// Играчът спира да тегли. Дилърът тегли карти, докато достигне поне 17,
        /// след което се определя победителят и се сетълва залога.
        /// </summary>
        /// <param name="sessionId">Публичният идентификатор на сесията.</param>
        /// <response code="200">Рундът е приключил. Връща финалното <c>BlackjackStateDto</c> с <c>Result</c> и <c>WinAmount</c>.</response>
        /// <response code="400">Сесията не е в <c>PlayerTurn</c>.</response>
        /// <response code="401">Липсва валиден JWT токен.</response>
        /// <response code="404">Сесията не е намерена или не принадлежи на текущия потребител.</response>
        /// <response code="500">Неочаквана грешка.</response>
        [HttpPost("sessions/{sessionId}/stand")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Stand([FromRoute] string sessionId)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _bj.StandAsync(userId.Value, sessionId));
        }

        /// <summary>
        /// Удвоява залога, тегли точно една карта и автоматично приключва рунда (stand).
        /// <para>
        /// Позволено само на първите две карти на играча.
        /// Удвоената сума се удържа допълнително от wallet-а.
        /// </para>
        /// </summary>
        /// <param name="sessionId">Публичният идентификатор на сесията.</param>
        /// <response code="200">Рундът е приключил с удвоен залог.</response>
        /// <response code="400">Сесията не е в <c>PlayerTurn</c> или играчът вече е теглил карта.</response>
        /// <response code="401">Липсва валиден JWT токен.</response>
        /// <response code="404">Сесията не е намерена или не принадлежи на текущия потребител.</response>
        /// <response code="500">Неочаквана грешка.</response>
        [HttpPost("sessions/{sessionId}/double")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Double([FromRoute] string sessionId)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _bj.DoubleAsync(userId.Value, sessionId));
        }

        /// <summary>
        /// Връща текущото състояние на сесията без да променя нищо.
        /// <para>
        /// Полезно при reconnect от React Native (напр. след app background / foreground).
        /// Втората карта на дилъра се показва като <c>"?"</c>, докато сесията не приключи.
        /// </para>
        /// </summary>
        /// <param name="sessionId">Публичният идентификатор на сесията.</param>
        /// <response code="200">Връща текущото <c>BlackjackStateDto</c>.</response>
        /// <response code="401">Липсва валиден JWT токен.</response>
        /// <response code="404">Сесията не е намерена или не принадлежи на текущия потребител.</response>
        /// <response code="500">Неочаквана грешка.</response>
        [HttpGet("sessions/{sessionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetState([FromRoute] string sessionId)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _bj.GetStateAsync(userId.Value, sessionId));
        }
    }
}
