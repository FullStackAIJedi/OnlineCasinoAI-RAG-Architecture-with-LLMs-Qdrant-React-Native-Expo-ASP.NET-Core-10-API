namespace OnlineCasino.Services
{
    using System.Text;
    using System.Security.Claims;
    using System.IdentityModel.Tokens.Jwt;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;

    using AutoMapper;

    using OnlineCasino.DTOs.Auth;
    using OnlineCasino.Data;
    using OnlineCasino.Entities;
    using OnlineCasino.Models;

    /// <summary>
    /// Authentication service – registration, login, and password change.
    /// <para>
    /// Generates JWT tokens with claims: <c>sub</c>, <c>NameIdentifier</c>, <c>email</c>,
    /// <c>nickname</c>, <c>role</c>. The token is written to an HttpOnly cookie by the controller.
    /// </para>
    /// <para>
    /// Passwords are hashed with BCrypt. They are never logged or returned in a response.
    /// </para>
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user with the <c>User</c> role and an initial wallet (demo bonus of 1000 EUR).
        /// </summary>
        /// <param name="req">Email, NickName, Password.</param>
        /// <returns>
        /// On success – a JWT token + expiration date.
        /// On failure – <c>409 Conflict</c> if the email or nickname already exists.
        /// </returns>
        Task<Result<(string Token, DateTime Expires)>> SignUpAsync(SignUpRequest req);

        /// <summary>
        /// Logs in an existing user.
        /// </summary>
        /// <param name="req">Email and Password.</param>
        /// <returns>
        /// On success – a JWT token + expiration date.
        /// On failure – <c>401</c> (invalid credentials) or <c>403</c> (blocked account).
        /// </returns>
        Task<Result<(string Token, DateTime Expires)>> SignInAsync(SignInRequest req);

        /// <summary>
        /// Changes the user's password after verifying the current password.
        /// </summary>
        /// <param name="userId">User ID (from JWT claims).</param>
        /// <param name="req">CurrentPassword and NewPassword.</param>
        /// <returns>
        /// On success – <c>true</c>.
        /// On failure – <c>400</c> (incorrect current password) or <c>404</c> (user not found).
        /// </returns>
        Task<Result<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest req);
    }

    /// <summary>
    /// Implementation of <see cref="IAuthService"/>.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _cfg;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthService"/> class.
        /// </summary>
        /// <param name="db">The application database context.</param>
        /// <param name="cfg">The application configuration (used for JWT settings).</param>
        /// <param name="mapper">The AutoMapper instance used to map request DTOs to entities.</param>
        public AuthService(AppDbContext db, IConfiguration cfg, IMapper mapper)
        {
            _db = db;
            _cfg = cfg;
            _mapper = mapper;
        }

        /// <summary>
        /// Registers a new user with the <c>User</c> role and an initial wallet (demo bonus of 1000 EUR).
        /// </summary>
        /// <param name="req">Email, NickName, Password.</param>
        /// <returns>
        /// On success – a JWT token + expiration date.
        /// On failure – <c>409 Conflict</c> if the email or nickname already exists.
        /// </returns>
        public async Task<Result<(string Token, DateTime Expires)>> SignUpAsync(SignUpRequest req)
        {
            if (await _db.Users.AnyAsync(u => u.Email == req.Email))
                return Result<(string, DateTime)>.Failure("Email is already in use.", 409);

            if (await _db.Users.AnyAsync(u => u.NickName == req.NickName))
                return Result<(string, DateTime)>.Failure("Nickname is already in use.", 409);

            var userRoleId = await _db.Roles
                .Where(r => r.Name == "User")
                .Select(r => r.Id)
                .FirstAsync();

            var user = _mapper.Map<User>(req);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password);
            user.RoleId = userRoleId;
            user.Wallet = new Wallet { Balance = 1000m, Currency = "EUR" };  // демо бонус

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Result<(string, DateTime)>.Success(GenerateToken(user, "User"));
        }

        /// <summary>
        /// Logs in an existing user.
        /// </summary>
        /// <param name="req">Email and Password.</param>
        /// <returns>
        /// On success – a JWT token + expiration date.
        /// On failure – <c>401</c> (invalid credentials) or <c>403</c> (blocked account).
        /// </returns>
        public async Task<Result<(string Token, DateTime Expires)>> SignInAsync(SignInRequest req)
        {
            // Projection – вземаме само нужните полета + role name, без Include
            var user = await _db.Users
                .Where(u => u.Email == req.Email)
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.NickName,
                    u.PasswordHash,
                    u.IsBlocked,
                    RoleName = u.Role.Name
                })
                .FirstOrDefaultAsync();

            if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
                return Result<(string, DateTime)>.Failure("Invalid email or password.", 401);

            if (user.IsBlocked)
                return Result<(string, DateTime)>.Failure("Account is blocked.", 403);

            var entity = new User { Id = user.Id, Email = user.Email, NickName = user.NickName };
            return Result<(string, DateTime)>.Success(GenerateToken(entity, user.RoleName));
        }

        /// <summary>
        /// Changes the user's password after verifying the current password.
        /// </summary>
        /// <param name="userId">User ID (from JWT claims).</param>
        /// <param name="req">CurrentPassword and NewPassword.</param>
        /// <returns>
        /// On success – <c>true</c>.
        /// On failure – <c>400</c> (incorrect current password) or <c>404</c> (user not found).
        /// </returns>
        public async Task<Result<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest req)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user is null)
                return Result<bool>.Failure("User not found.", 404);

            if (!BCrypt.Net.BCrypt.Verify(req.CurrentPassword, user.PasswordHash))
                return Result<bool>.Failure("Current password is incorrect.", 400);

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
            await _db.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        /// <summary>
        /// Generates a JWT token with claims from the user and their role.
        /// <para>
        /// Claims:
        /// <list type="bullet">
        ///   <item><c>sub</c> – user id (JWT standard).</item>
        ///   <item><c>NameIdentifier</c> – user id (used by <c>GetUserId()</c>).</item>
        ///   <item><c>email</c> – user's email.</item>
        ///   <item><c>nickname</c> – nickname (custom claim).</item>
        ///   <item><c>role</c> – role (User / Admin).</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="user">User (may be a partial entity – only Id, Email, NickName matter).</param>
        /// <param name="role">Role name (<c>"User"</c> or <c>"Admin"</c>).</param>
        /// <returns>A tuple of (JWT token as string, UTC expiration date).</returns>
        private (string, DateTime) GenerateToken(User user, string role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(int.Parse(_cfg["Jwt:ExpireMinutes"]!));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("nickname", user.NickName),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: _cfg["Jwt:Issuer"],
                audience: _cfg["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(token), expires);
        }
    }
}
