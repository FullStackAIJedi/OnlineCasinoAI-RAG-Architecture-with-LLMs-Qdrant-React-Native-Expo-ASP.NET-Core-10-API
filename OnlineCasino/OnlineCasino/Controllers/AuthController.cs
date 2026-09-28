namespace OnlineCasino.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using OnlineCasino.DTOs.Auth;
    using OnlineCasino.Services;

    /// <summary>
    /// Authentication endpoints – регистрация, логин, логаут, смяна на парола.
    /// Използва HttpOnly cookie ("access_token") за JWT токена.
    /// </summary>
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth) => _auth = auth;

        /// <summary>
        /// Регистрира нов потребител.
        /// При успех записва JWT токен в HttpOnly cookie и връща { message = "OK" }.
        /// </summary>
        /// <param name="req">Email, NickName, Password.</param>
        /// <response code="200">Успешна регистрация, cookie е сет-нат.</response>
        /// <response code="409">Email или NickName вече е зает.</response>
        /// <response code="500">Неочаквана грешка.</response>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] SignUpRequest req)
        {
            var result = await _auth.SignUpAsync(req);
            if (!result.IsSuccess) return AsActionResult(result);

            SetAuthCookie(result.Data.Token, result.Data.Expires);
            return AsActionResult(ResultMap(result));
        }

        /// <summary>
        /// Логва съществуващ потребител.
        /// При успех записва JWT токен в HttpOnly cookie и връща { message = "OK" }.
        /// </summary>
        /// <param name="req">Email и Password.</param>
        /// <response code="200">Успешен логин, cookie е сет-нат.</response>
        /// <response code="401">Грешен email или парола.</response>
        /// <response code="403">Акаунтът е блокиран.</response>
        /// <response code="500">Неочаквана грешка.</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] SignInRequest req)
        {
            var result = await _auth.SignInAsync(req);
            if (!result.IsSuccess) return AsActionResult(result);

            SetAuthCookie(result.Data.Token, result.Data.Expires);
            return AsActionResult(ResultMap(result));
        }

        /// <summary>
        /// Изчиства auth cookie-то (logout на текущата сесия).
        /// Не инвалидира JWT-то server-side (демо решение).
        /// </summary>
        /// <response code="200">Винаги успешен.</response>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Logout()
        {
            ClearAuthCookie();
            return Ok(new { message = "OK" });
        }

        /// <summary>
        /// Сменя паролата на логнатия потребител.
        /// Изисква текущата парола за потвърждение.
        /// </summary>
        /// <param name="req">CurrentPassword и NewPassword.</param>
        /// <response code="200">Паролата е сменена успешно.</response>
        /// <response code="400">Текущата парола е грешна.</response>
        /// <response code="401">Липсва валиден токен.</response>
        /// <response code="404">Потребителят не е намерен.</response>
        /// <response code="500">Неочаквана грешка.</response>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });

            var result = await _auth.ChangePasswordAsync(userId.Value, req);
            return AsActionResult(result);
        }

        /// <summary>
        /// Връща информация за текущо логнатия потребител от JWT claims.
        /// Полезно за React Native при app start (за да възстановиш session state).
        /// </summary>
        /// <response code="200">Връща id, email, nickname, role.</response>
        /// <response code="401">Липсва валиден токен.</response>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Me()
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });

            return Ok(new
            {
                id = userId.Value,
                email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                nickname = User.FindFirst("nickname")?.Value,
                role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
            });
        }
    }
}
