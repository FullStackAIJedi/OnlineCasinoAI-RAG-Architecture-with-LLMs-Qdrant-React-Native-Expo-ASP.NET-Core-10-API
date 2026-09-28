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
    /// Authentication service – регистрация, логин и смяна на парола.
    /// <para>
    /// Генерира JWT токени с claims: <c>sub</c>, <c>NameIdentifier</c>, <c>email</c>,
    /// <c>nickname</c>, <c>role</c>. Токенът се записва в HttpOnly cookie от контролера.
    /// </para>
    /// <para>
    /// Паролите се хешират с BCrypt. Никога не се логват или връщат в response.
    /// </para>
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Регистрира нов потребител с роля <c>User</c> и начален wallet (демо бонус 1000 EUR).
        /// </summary>
        /// <param name="req">Email, NickName, Password.</param>
        /// <returns>
        /// При успех – JWT токен + дата на изтичане.
        /// При грешка – <c>409 Conflict</c> ако email или nickname вече съществуват.
        /// </returns>
        Task<Result<(string Token, DateTime Expires)>> SignUpAsync(SignUpRequest req);

        /// <summary>
        /// Логва съществуващ потребител.
        /// </summary>
        /// <param name="req">Email и Password.</param>
        /// <returns>
        /// При успех – JWT токен + дата на изтичане.
        /// При грешка – <c>401</c> (грешни credentials) или <c>403</c> (блокиран акаунт).
        /// </returns>
        Task<Result<(string Token, DateTime Expires)>> SignInAsync(SignInRequest req);

        /// <summary>
        /// Сменя паролата на потребителя след проверка на текущата парола.
        /// </summary>
        /// <param name="userId">Id на потребителя (от JWT claims).</param>
        /// <param name="req">CurrentPassword и NewPassword.</param>
        /// <returns>
        /// При успех – <c>true</c>.
        /// При грешка – <c>400</c> (грешна текуща парола) или <c>404</c> (потребителят не е намерен).
        /// </returns>
        Task<Result<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest req);
    }

    /// <summary>
    /// Имплементация на <see cref="IAuthService"/>.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _cfg;
        private readonly IMapper _mapper;

        public AuthService(AppDbContext db, IConfiguration cfg, IMapper mapper)
        {
            _db = db;
            _cfg = cfg;
            _mapper = mapper;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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
        /// Генерира JWT токен с claims от потребителя и ролята му.
        /// <para>
        /// Claims:
        /// <list type="bullet">
        ///   <item><c>sub</c> – user id (JWT standard).</item>
        ///   <item><c>NameIdentifier</c> – user id (използва се от <c>GetUserId()</c>).</item>
        ///   <item><c>email</c> – имейл на потребителя.</item>
        ///   <item><c>nickname</c> – nickname (custom claim).</item>
        ///   <item><c>role</c> – роля (User / Admin).</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="user">Потребител (може да е частичен entity – важно е Id, Email, NickName).</param>
        /// <param name="role">Име на ролята (<c>"User"</c> или <c>"Admin"</c>).</param>
        /// <returns>Tuple от (JWT токен като string, UTC дата на изтичане).</returns>
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