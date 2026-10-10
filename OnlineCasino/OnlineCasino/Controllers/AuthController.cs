namespace OnlineCasino.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using OnlineCasino.DTOs.Auth;
    using OnlineCasino.Services;

    /// <summary>
    /// Authentication endpoints – registration, login, logout, password change.
    /// Uses an HttpOnly cookie ("access_token") for the JWT token.
    /// </summary>
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth) => _auth = auth;

        /// <summary>
        /// Registers a new user.
        /// On success, writes the JWT token into an HttpOnly cookie and returns { message = "OK" }.
        /// </summary>
        /// <param name="req">Email, NickName, Password.</param>
        [HttpPost("register")]
        [AllowAnonymous] 
        public async Task<IActionResult> Register([FromBody] SignUpRequest req)
        {
            var result = await _auth.SignUpAsync(req);
            if (!result.IsSuccess) return AsActionResult(result);

            SetAuthCookie(result.Data.Token, result.Data.Expires);
            return AsActionResult(ResultMap(result));
        }

        /// <summary>
        /// Logs in an existing user.
        /// On success, writes the JWT token into an HttpOnly cookie and returns { message = "OK" }.
        /// </summary>
        /// <param name="req">Email and Password.</param>
        [HttpPost("login")]
        [AllowAnonymous] 
        public async Task<IActionResult> Login([FromBody] SignInRequest req)
        {
            var result = await _auth.SignInAsync(req);
            if (!result.IsSuccess) return AsActionResult(result);

            SetAuthCookie(result.Data.Token, result.Data.Expires);
            return AsActionResult(ResultMap(result));
        }

        /// <summary>
        /// Clears the auth cookie (logout of the current session).
        /// Does not invalidate the JWT server-side (demo solution).
        /// </summary>
        [HttpPost("logout")]
        [Authorize] 
        public IActionResult Logout()
        {
            ClearAuthCookie();
            return Ok(new { message = "OK" });
        }

        /// <summary>
        /// Changes the password of the logged-in user.
        /// Requires the current password for confirmation.
        /// </summary>
        /// <param name="req">CurrentPassword and NewPassword.</param>
        [HttpPost("change-password")]
        [Authorize] 
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });

            var result = await _auth.ChangePasswordAsync(userId.Value, req);
            return AsActionResult(result);
        }

        /// <summary>
        /// Returns information about the currently logged-in user from JWT claims.
        /// Useful for React Native on app start (to restore session state).
        /// </summary>
        [HttpGet("me")]
        [Authorize] 
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
