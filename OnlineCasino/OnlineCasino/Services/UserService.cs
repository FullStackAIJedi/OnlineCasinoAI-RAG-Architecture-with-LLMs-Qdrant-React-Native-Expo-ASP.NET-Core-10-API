namespace OnlineCasino.Services
{
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using OnlineCasino.Data;
    using OnlineCasino.DTOs.Users;
    using OnlineCasino.Models;

    /// <summary>
    /// User service – профил, администрация и блокиране на потребители.
    /// <para>
    /// Всички заявки използват projection директно към DTO – без <c>Include</c>,
    /// без зареждане на цели entities.
    /// </para>
    /// <para>
    /// Методите <see cref="GetAllAsync"/> и <see cref="SetBlockedAsync"/> са предназначени
    /// за Admin endpoints. Останалите работят върху текущо логнатия потребител.
    /// </para>
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Връща профила на потребителя по неговото id.
        /// <para>
        /// Включва: id, email, nickname, firstName, lastName, role, createdAt.
        /// Не включва чувствителни данни (password hash, internal flags).
        /// </para>
        /// </summary>
        /// <param name="userId">Id на потребителя (от JWT).</param>
        /// <returns>
        /// <see cref="UserProfileDto"/> при успех;
        /// <c>404 Not Found</c> ако потребителят не съществува.
        /// </returns>
        Task<Result<UserProfileDto>> GetProfileAsync(int userId);

        /// <summary>
        /// Обновява профила на потребителя (partial update).
        /// <para>
        /// Позволени полета: <c>FirstName</c>, <c>LastName</c>.
        /// Ако поле е <c>null</c> – текущата стойност се запазва.
        /// </para>
        /// <para>
        /// Email, NickName и Role <b>не</b> могат да се променят през този метод.
        /// </para>
        /// </summary>
        /// <param name="userId">Id на потребителя.</param>
        /// <param name="req">Заявка с <c>FirstName</c> и/или <c>LastName</c>.</param>
        /// <returns>Обновения профил след успешен update.</returns>
        Task<Result<UserProfileDto>> UpdateProfileAsync(int userId, UpdateProfileRequest req);

        /// <summary>
        /// Изтрива (hard delete) акаунта на потребителя.
        /// <para>
        /// Демо решение – cascade-ва всички свързани записи (wallet, transactions, rounds, sessions).
        /// В production трябва да е soft delete + GDPR анонимизация, за да се запази
        /// историята на транзакциите за одит.
        /// </para>
        /// </summary>
        /// <param name="userId">Id на потребителя.</param>
        Task<Result<bool>> DeleteAccountAsync(int userId);

        /// <summary>
        /// Връща списък с всички потребители (Admin only).
        /// <para>
        /// Включва: id, email, nickname, role, isBlocked, balance.
        /// Без pagination – за демо.
        /// </para>
        /// </summary>
        Task<Result<List<UserListItemDto>>> GetAllAsync();

        /// <summary>
        /// Блокира или отблокира потребител.
        /// <para>
        /// Блокиран потребител не може да се логне – <c>SignInAsync</c> връща 403.
        /// Съществуващите JWT токени остават валидни до изтичане (демо решение).
        /// </para>
        /// </summary>
        /// <param name="userId">Id на потребителя.</param>
        /// <param name="blocked"><c>true</c> за блокиране, <c>false</c> за отблокиране.</param>
        Task<Result<bool>> SetBlockedAsync(int userId, bool blocked);
    }

    /// <summary>
    /// Имплементация на <see cref="IUserService"/>.
    /// <para>
    /// Използва <see cref="IMapper"/> само за mapping в update операциите, където
    /// projection не е достатъчен. Всички read-only заявки ползват директна проекция.
    /// </para>
    /// </summary>
    public class UserService : IUserService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public UserService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        /// <inheritdoc />
        public async Task<Result<UserProfileDto>> GetProfileAsync(int userId)
        {
            var dto = await _db.Users
                .Where(u => u.Id == userId)
                .Select(u => new UserProfileDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    NickName = u.NickName,
                    PofileImage = u.PofileImage,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Role = u.Role.Name,
                    CreatedAt = u.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (dto is null)
                return Result<UserProfileDto>.Failure("User not found.", 404);

            return Result<UserProfileDto>.Success(dto);
        }

        /// <inheritdoc />
        public async Task<Result<UserProfileDto>> UpdateProfileAsync(int userId, UpdateProfileRequest req)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user is null)
                return Result<UserProfileDto>.Failure("User not found.", 404);

            // Partial update – null полета не променят текущата стойност
            user.FirstName = req.FirstName ?? user.FirstName;
            user.LastName = req.LastName ?? user.LastName;
            user.PofileImage = req.PofileImage ?? user.PofileImage;
            await _db.SaveChangesAsync();

            return await GetProfileAsync(userId);
        }

        /// <inheritdoc />
        public async Task<Result<bool>> DeleteAccountAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user is null)
                return Result<bool>.Failure("User not found.", 404);

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        /// <inheritdoc />
        public async Task<Result<List<UserListItemDto>>> GetAllAsync()
        {
            var list = await _db.Users
                .AsNoTracking()
                .Select(u => new UserListItemDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    NickName = u.NickName,
                    PofileImage = u.PofileImage,
                    Role = u.Role.Name,
                    IsBlocked = u.IsBlocked,
                    Balance = u.Wallet.Balance
                })
                .ToListAsync();

            return Result<List<UserListItemDto>>.Success(list);
        }

        /// <inheritdoc />
        public async Task<Result<bool>> SetBlockedAsync(int userId, bool blocked)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user is null)
                return Result<bool>.Failure("User not found.", 404);

            user.IsBlocked = blocked;
            await _db.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
    }
}