namespace OnlineCasino.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using OnlineCasino.DTOs.Users;
    using OnlineCasino.Services;

    /// <summary>
    /// Управление на потребители.
    /// <para>
    /// Endpoints с <c>/me</c> работят върху текущо логнатия потребител (от JWT claims).
    /// Endpoints без <c>/me</c> изискват роля <c>Admin</c>.
    /// </para>
    /// </summary>
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserService _users;

        public UsersController(IUserService users) => _users = users;

        /// <summary>
        /// Връща профила на текущо логнатия потребител.
        /// <para>
        /// Включва: id, email, nickname, firstName, lastName, role, createdAt.
        /// Не включва чувствителни данни (password hash, internal flags).
        /// </para>
        /// </summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });

            return AsActionResult(await _users.GetProfileAsync(userId.Value));
        }

        /// <summary>
        /// Обновява профила на текущо логнатия потребител.
        /// <para>
        /// Позволени полета: <c>FirstName</c>, <c>LastName</c>.
        /// Ако поле е <c>null</c> – текущата стойност се запазва (partial update).
        /// </para>
        /// <para>
        /// Email, NickName и Role <b>не</b> могат да се променят през този endpoint.
        /// </para>
        /// </summary>
        /// <param name="req">Заявка с <c>FirstName</c> и/или <c>LastName</c>.</param>
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest req)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });

            return AsActionResult(await _users.UpdateProfileAsync(userId.Value, req));
        }

        /// <summary>
        /// Изтрива (hard delete) акаунта на текущо логнатия потребител.
        /// <para>
        /// Демо решение – cascade-ва всички свързани записи (wallet, transactions, rounds).
        /// В production трябва да е soft delete + GDPR анонимизация.
        /// </para>
        /// <para>
        /// След успех клиентът трябва да изчисти auth cookie-то си локално.
        /// </para>
        /// </summary>
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteMe()
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });

            return AsActionResult(await _users.DeleteAccountAsync(userId.Value));
        }

        /// <summary>
        /// Връща списък с всички потребители (Admin only).
        /// <para>
        /// Включва: id, email, nickname, role, isBlocked, balance.
        /// Без pagination – за демо.
        /// </para>
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
            => AsActionResult(await _users.GetAllAsync());

        /// <summary>
        /// Блокира потребител (Admin only).
        /// <para>
        /// Блокиран потребител не може да се логне – <c>SignInAsync</c> връща 403.
        /// Съществуващите JWT токени остават валидни до изтичане (демо решение).
        /// </para>
        /// </summary>
        /// <param name="userId">Id на потребителя за блокиране.</param>
        [HttpPost("{userId:int}/block")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Block([FromRoute] int userId)
            => AsActionResult(await _users.SetBlockedAsync(userId, true));

        /// <summary>
        /// Отблокира потребител (Admin only).
        /// </summary>
        /// <param name="userId">Id на потребителя за отблокиране.</param>
        [HttpPost("{userId:int}/unblock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Unblock([FromRoute] int userId)
            => AsActionResult(await _users.SetBlockedAsync(userId, false));
    }
}
