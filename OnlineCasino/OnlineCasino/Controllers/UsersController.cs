namespace OnlineCasino.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using OnlineCasino.DTOs.Users;
    using OnlineCasino.Services;

    /// <summary>
    /// User management.
    /// <para>
    /// Endpoints with <c>/me</c> operate on the currently logged-in user (from JWT claims).
    /// Endpoints without <c>/me</c> require the <c>Admin</c> role.
    /// </para>
    /// </summary>
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserService _users;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController"/> class.
        /// </summary>
        /// <param name="users">The user service used for all profile operations.</param>
        public UsersController(IUserService users) => _users = users;

        /// <summary>
        /// Returns the profile of the currently logged-in user.
        /// <para>
        /// Includes: id, email, nickname, firstName, lastName, role, createdAt.
        /// Does not include sensitive data (password hash, internal flags).
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
        /// Updates the profile of the currently logged-in user.
        /// <para>
        /// Allowed fields: <c>FirstName</c>, <c>LastName</c>.
        /// If a field is <c>null</c> – the current value is preserved (partial update).
        /// </para>
        /// <para>
        /// Email, NickName, and Role <b>cannot</b> be changed through this endpoint.
        /// </para>
        /// </summary>
        /// <param name="req">Request containing <c>FirstName</c> and/or <c>LastName</c>.</param>
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest req)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });

            return AsActionResult(await _users.UpdateProfileAsync(userId.Value, req));
        }

        /// <summary>
        /// Deletes (hard delete) the account of the currently logged-in user.
        /// <para>
        /// Demo solution – cascades all related records (wallet, transactions, rounds).
        /// In production, this should be a soft delete + GDPR anonymization.
        /// </para>
        /// <para>
        /// After success, the client should clear its auth cookie locally.
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
        /// Returns a list of all users (Admin only).
        /// <para>
        /// Includes: id, email, nickname, role, isBlocked, balance.
        /// No pagination – for demo purposes.
        /// </para>
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
            => AsActionResult(await _users.GetAllAsync());

        /// <summary>
        /// Blocks a user (Admin only).
        /// <para>
        /// A blocked user cannot sign in – <c>SignInAsync</c> returns 403.
        /// Existing JWT tokens remain valid until they expire (demo solution).
        /// </para>
        /// </summary>
        /// <param name="userId">ID of the user to block.</param>
        [HttpPost("{userId:int}/block")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Block([FromRoute] int userId)
            => AsActionResult(await _users.SetBlockedAsync(userId, true));

        /// <summary>
        /// Unblocks a user (Admin only).
        /// </summary>
        /// <param name="userId">ID of the user to unblock.</param>
        [HttpPost("{userId:int}/unblock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Unblock([FromRoute] int userId)
            => AsActionResult(await _users.SetBlockedAsync(userId, false));
    }
}
