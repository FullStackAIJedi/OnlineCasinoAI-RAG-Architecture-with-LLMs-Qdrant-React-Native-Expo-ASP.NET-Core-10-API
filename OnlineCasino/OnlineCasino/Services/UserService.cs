namespace OnlineCasino.Services
{ 
    using Microsoft.EntityFrameworkCore;

    using AutoMapper;

    using OnlineCasino.Data;
    using OnlineCasino.DTOs.Users;
    using OnlineCasino.Models;
     
    /// <summary>
    /// User service – profile management, administration, and user blocking.
    /// <para>
    /// All queries use direct projection to DTOs – without <c>Include</c>,
    /// without loading entire entities.
    /// </para>
    /// <para>
    /// The <see cref="GetAllAsync"/> and <see cref="SetBlockedAsync"/> methods are intended
    /// for Admin endpoints. The remaining methods operate on the currently logged-in user.
    /// </para>
    /// </summary> 
    public interface IUserService
    {
        /// <summary>
        /// Returns the user's profile by their ID.
        /// <para>
        /// Includes: ID, email, nickname, first name, last name, role, and creation date.
        /// Excludes sensitive data (password hash, internal flags).
        /// </para>
        /// </summary>
        /// <param name="userId">User ID (from JWT).</param>
        /// <returns>
        /// <see cref="UserProfileDto"/> on success;
        /// <c>404 Not Found</c> if the user does not exist.
        /// </returns> 
        Task<Result<UserProfileDto>> GetProfileAsync(int userId);

        /// <summary>
        /// Updates the user's profile (partial update).
        /// <para>
        /// Allowed fields: <c>FirstName</c>, <c>LastName</c>.
        /// If a field is <c>null</c>, its current value is preserved.
        /// </para>
        /// <para>
        /// Email, NickName, and Role cannot be changed through this method.
        /// </para>
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="req">Request containing <c>FirstName</c> and/or <c>LastName</c>.</param>
        /// <returns>The updated profile after a successful update.</returns> 
        Task<Result<UserProfileDto>> UpdateProfileAsync(int userId, UpdateProfileRequest req);

        /// <summary>
        /// Deletes the user's account (hard delete).
        /// <para>
        /// Demo implementation – cascades deletion to all related records (wallet, transactions, rounds, sessions).
        /// In production, this should be a soft delete combined with GDPR anonymization to preserve
        /// transaction history for auditing purposes.
        /// </para>
        /// </summary>
        /// <param name="userId">User ID.</param> 
        Task<Result<bool>> DeleteAccountAsync(int userId);

        /// <summary>
        /// Returns a list of all users (Admin only).
        /// <para>
        /// Includes: ID, email, nickname, role, blocked status, and balance.
        /// No pagination – for demo purposes.
        /// </para>
        /// </summary> 
        Task<Result<List<UserListItemDto>>> GetAllAsync();

        /// <summary>
        /// Blocks or unblocks a user.
        /// <para>
        /// A blocked user cannot log in – <c>SignInAsync</c> returns 403.
        /// Existing JWT tokens remain valid until they expire (demo implementation).
        /// </para>
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="blocked"><c>true</c> to block, <c>false</c> to unblock.</param> 
        Task<Result<bool>> SetBlockedAsync(int userId, bool blocked);
    }

    /// <summary>
    /// Implementation of <see cref="IUserService"/>.
    /// <para>
    /// Uses <see cref="IMapper"/> only for mapping in update operations where
    /// projection is insufficient. All read-only queries use direct projection.
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
