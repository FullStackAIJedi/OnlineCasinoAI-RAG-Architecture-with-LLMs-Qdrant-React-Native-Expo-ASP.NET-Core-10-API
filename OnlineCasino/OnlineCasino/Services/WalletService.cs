namespace OnlineCasino.Services
{
    using Microsoft.EntityFrameworkCore;

    using OnlineCasino.Common.Models;
    using OnlineCasino.Data;
    using OnlineCasino.DTOs.Wallet;
    using OnlineCasino.Entities;
    using OnlineCasino.Models;

    /// <summary>
    /// Wallet service – balance, transactions, and atomic operations on the wallet.
    /// <para>
    /// All balance changes go through <see cref="ApplyAsync"/>, which:
    /// <list type="bullet">
    ///   <item>updates <c>Wallet.Balance</c>,</item>
    ///   <item>records an entry in <c>Transactions</c> with the type, amount, and balance after the operation,</item>
    ///   <item>records the <c>UpdatedAt</c> timestamp.</item>
    /// </list>
    /// </para>
    /// <para>
    /// The balance must never be modified directly from a controller – always through this service.
    /// </para>
    /// </summary> 
    public interface IWalletService
    {
        /// <summary>
        /// Returns the user's current balance and currency.
        /// </summary>
        /// <param name="userId">User ID (from JWT).</param>
        Task<Result<WalletDto>> GetWalletAsync(int userId);

        /// <summary>
        /// Returns the user's paginated transaction history.
        /// <para>
        /// Sorted by <c>CreatedAt</c> descending (newest first).
        /// </para>
        /// <para>
        /// Pagination is normalized: <c>page &lt; 1 → 1</c>, <c>pageSize &lt; 1 or &gt; 100 → 20</c>.
        /// </para>
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="page">Page number (starts at 1).</param>
        /// <param name="pageSize">Number of records per page (1–100).</param>
        Task<Result<PagedResult<TransactionDto>>> GetTransactionsAsync(int userId, int page, int pageSize);

        /// <summary>
        /// Demo deposit – credits the amount immediately.
        /// <para>
        /// Creates a <c>Transaction</c> with type <c>Deposit</c> and a <c>null</c> reference.
        /// </para>
        /// <para>
        /// In production, it should create a pending payment and wait for a webhook.
        /// </para>
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="amount">The amount (must be positive).</param>
        Task<Result<WalletDto>> DepositAsync(int userId, decimal amount);


        /// <summary>
        /// Demo withdrawal – debits the amount immediately if there is a sufficient balance.
        /// <para>
        /// Creates a <c>Transaction</c> with type <c>Withdraw</c> and a negative amount.
        /// </para>
        /// <para>
        /// In production, it should create a pending withdrawal that goes
        /// through Finance approval and the payment provider.
        /// </para>
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="amount">The amount (must be positive and ≤ the balance).</param>
        Task<Result<WalletDto>> WithdrawAsync(int userId, decimal amount);
 
        /// <summary>
        /// Internal method – debits an amount from the wallet with type <c>Bet</c>.
        /// <para>Used by game services (Slots, Roulette, Blackjack) when placing a bet.</para>
        /// <para>
        /// Checks whether the balance is sufficient. Returns the new balance on success.
        /// </para>
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="amount">The amount to debit (positive).</param>
        /// <param name="referenceId">Reference to the round (e.g. <c>ClientRoundId</c> or <c>PublicId</c>).</param>
        /// <returns>The new balance after the debit.</returns> 
        Task<Result<decimal>> DebitAsync(int userId, decimal amount, string referenceId);


        /// <summary>
        /// Internal method – credits an amount to the wallet with type <c>Win</c>.
        /// <para>Used by game services when a win occurs.</para>
        /// <para>
        /// If <paramref name="amount"/> ≤ 0 – does nothing and returns the current balance
        /// (for the convenience of clients that call <c>CreditAsync</c> even when the win amount is 0).
        /// </para>
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="amount">The amount to credit (can be 0).</param>
        /// <param name="referenceId">Reference to the round.</param>
        /// <returns>The new balance after the credit.</returns>
        Task<Result<decimal>> CreditAsync(int userId, decimal amount, string referenceId);
    }
     
    /// <summary>
    /// Implementation of <see cref="IWalletService"/>.
    /// <para>
    /// Every operation that modifies the balance goes through <see cref="ApplyAsync"/> –
    /// the only place that writes to <c>Wallets</c> and <c>Transactions</c>.
    /// </para>
    /// </summary>  
    public class WalletService : IWalletService
    {
        private readonly AppDbContext _db;

        public WalletService(AppDbContext db) => _db = db;

        /// <inheritdoc />
        public async Task<Result<WalletDto>> GetWalletAsync(int userId)
        {
            var wallet = await _db.Wallets
                .Where(w => w.UserId == userId)
                .Select(w => new WalletDto { Balance = w.Balance, Currency = w.Currency })
                .FirstOrDefaultAsync();

            if (wallet is null)
                return Result<WalletDto>.Failure("Wallet not found.", 404);

            return Result<WalletDto>.Success(wallet);
        }

        /// <inheritdoc />
        public async Task<Result<PagedResult<TransactionDto>>> GetTransactionsAsync(int userId, int page, int pageSize)
        { 
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var query = _db.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt);

            var total = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    Type = t.TransactionType.Name,
                    Amount = t.Amount,
                    BalanceAfter = t.BalanceAfter,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return Result<PagedResult<TransactionDto>>.Success(new PagedResult<TransactionDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                Total = total
            });
        }

        /// <inheritdoc />
        public async Task<Result<WalletDto>> DepositAsync(int userId, decimal amount)
        {
            if (amount <= 0)
                return Result<WalletDto>.Failure("Amount must be positive.", 400);

            var typeId = await GetTransactionTypeId("Deposit");
            return await ApplyAsync(userId, amount, typeId, null);
        }

        /// <inheritdoc />
        public async Task<Result<WalletDto>> WithdrawAsync(int userId, decimal amount)
        {
            if (amount <= 0)
                return Result<WalletDto>.Failure("Amount must be positive.", 400);

            var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet is null)
                return Result<WalletDto>.Failure("Wallet not found.", 404);

            if (wallet.Balance < amount)
                return Result<WalletDto>.Failure("Insufficient balance.", 400);

            var typeId = await GetTransactionTypeId("Withdraw");
            return await ApplyAsync(userId, -amount, typeId, null);
        }

        /// <inheritdoc />
        public async Task<Result<decimal>> DebitAsync(int userId, decimal amount, string referenceId)
        {
            if (amount <= 0)
                return Result<decimal>.Failure("Amount must be positive.", 400);

            var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet is null)
                return Result<decimal>.Failure("Wallet not found.", 404);

            if (wallet.Balance < amount)
                return Result<decimal>.Failure("Insufficient balance.", 400);

            var typeId = await GetTransactionTypeId("Bet");
            var res = await ApplyAsync(userId, -amount, typeId, referenceId);
            if (!res.IsSuccess)
                return Result<decimal>.Failure(res.Error!.Message, res.Error.StatusCode);

            return Result<decimal>.Success(res.Data!.Balance);
        }

        /// <inheritdoc />
        public async Task<Result<decimal>> CreditAsync(int userId, decimal amount, string referenceId)
        {
            // Ако няма печалба – не пипаме базата, само връщаме текущия баланс
            if (amount <= 0)
                return Result<decimal>.Success(await GetBalanceAsync(userId));

            var typeId = await GetTransactionTypeId("Win");
            var res = await ApplyAsync(userId, amount, typeId, referenceId);
            if (!res.IsSuccess)
                return Result<decimal>.Failure(res.Error!.Message, res.Error.StatusCode);

            return Result<decimal>.Success(res.Data!.Balance);
        }

        // --- helpers ---
         
        /// <summary>
        /// Returns the user's current balance (without projecting to a DTO).
        /// </summary> 
        private async Task<decimal> GetBalanceAsync(int userId)
        {
            return await _db.Wallets
                .Where(w => w.UserId == userId)
                .Select(w => w.Balance)
                .FirstAsync();
        }
         
        /// <summary>
        /// Returns the transaction type ID by name (e.g. <c>"Deposit"</c>, <c>"Bet"</c>, <c>"Win"</c>).
        /// </summary> 
        private async Task<int> GetTransactionTypeId(string name)
        {
            return await _db.TransactionTypes
                .Where(t => t.Name == name)
                .Select(t => t.Id)
                .FirstAsync();
        }

        /// <summary>
        /// Applies an atomic balance change:
        /// <list type="number">
        /// <item>Updates <c>Wallet.Balance</c> and <c>UpdatedAt</c>.</item>
        /// <item>Creates a <c>Transaction</c> record with the type, amount, balance after the operation, and reference.</item>
        /// <item>Saves the changes to the database.</item>
        /// </list>
        /// <para>
        /// This is the only place in the service that writes to <c>Wallets</c> and <c>Transactions</c>.
        /// </para>
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="delta">Balance change (positive for credit, negative for debit).</param>
        /// <param name="typeId">Transaction type ID (from <c>TransactionTypes</c>).</param>
        /// <param name="referenceId">Reference to the round, or <c>null</c> for deposit/withdrawal.</param>
        private async Task<Result<WalletDto>> ApplyAsync(int userId, decimal delta, int typeId, string? referenceId)
        {
            var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet is null)
                return Result<WalletDto>.Failure("Wallet not found.", 404);

            wallet.Balance += delta;
            wallet.UpdatedAt = DateTime.UtcNow;

            _db.Transactions.Add(new Transaction
            {
                UserId = userId,
                TransactionTypeId = typeId,
                Amount = delta,
                BalanceAfter = wallet.Balance,
                ReferenceId = referenceId,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            return Result<WalletDto>.Success(new WalletDto
            {
                Balance = wallet.Balance,
                Currency = wallet.Currency
            });
        }
    }
}
