namespace OnlineCasino.Services
{
    using Microsoft.EntityFrameworkCore;

    using OnlineCasino.Common.Models;
    using OnlineCasino.Data;
    using OnlineCasino.DTOs.Wallet;
    using OnlineCasino.Entities;
    using OnlineCasino.Models;

    /// <summary>
    /// Wallet service – баланс, транзакции и атомарни операции по wallet-а.
    /// <para>
    /// Всички промени по баланса минават през <see cref="ApplyAsync"/>, който:
    /// <list type="bullet">
    ///   <item>обновява <c>Wallet.Balance</c>,</item>
    ///   <item>записва ред в <c>Transactions</c> с типа, сумата и баланса след операцията,</item>
    ///   <item>записва <c>UpdatedAt</c> timestamp.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Никога не се променя баланс директно от контролер – винаги през този service.
    /// </para>
    /// </summary>
    public interface IWalletService
    {
        /// <summary>
        /// Връща текущия баланс и валутата на потребителя.
        /// </summary>
        /// <param name="userId">Id на потребителя (от JWT).</param>
        Task<Result<WalletDto>> GetWalletAsync(int userId);

        /// <summary>
        /// Връща пагинирана история на транзакциите на потребителя.
        /// <para>
        /// Сортирани по <c>CreatedAt</c> descending (най-новите първи).
        /// </para>
        /// <para>
        /// Пагинацията се нормализира: <c>page &lt; 1 → 1</c>, <c>pageSize &lt; 1 или &gt; 100 → 20</c>.
        /// </para>
        /// </summary>
        /// <param name="userId">Id на потребителя.</param>
        /// <param name="page">Номер на страница (започва от 1).</param>
        /// <param name="pageSize">Брой записи на страница (1–100).</param>
        Task<Result<PagedResult<TransactionDto>>> GetTransactionsAsync(int userId, int page, int pageSize);

        /// <summary>
        /// Демо депозит – кредитира сумата веднага.
        /// <para>
        /// Създава <c>Transaction</c> с тип <c>Deposit</c> и референция <c>null</c>.
        /// </para>
        /// <para>
        /// В production трябва да създава pending payment и да чака webhook.
        /// </para>
        /// </summary>
        /// <param name="userId">Id на потребителя.</param>
        /// <param name="amount">Сумата (трябва да е положителна).</param>
        Task<Result<WalletDto>> DepositAsync(int userId, decimal amount);

        /// <summary>
        /// Демо withdraw – дебитира сумата веднага, ако има достатъчен баланс.
        /// <para>
        /// Създава <c>Transaction</c> с тип <c>Withdraw</c> и отрицателна сума.
        /// </para>
        /// <para>
        /// В production трябва да създава pending withdrawal, който минава
        /// през Finance approval и payment provider.
        /// </para>
        /// </summary>
        /// <param name="userId">Id на потребителя.</param>
        /// <param name="amount">Сумата (трябва да е положителна и ≤ баланса).</param>
        Task<Result<WalletDto>> WithdrawAsync(int userId, decimal amount);

        /// <summary>
        /// Вътрешен метод – дебитира сума от wallet-а с тип <c>Bet</c>.
        /// <para>Използва се от game services (Slots, Roulette, Blackjack) при залагане.</para>
        /// <para>
        /// Проверява дали балансът е достатъчен. Връща новия баланс при успех.
        /// </para>
        /// </summary>
        /// <param name="userId">Id на потребителя.</param>
        /// <param name="amount">Сумата за дебитиране (положителна).</param>
        /// <param name="referenceId">Референция към рунда (напр. <c>ClientRoundId</c> или <c>PublicId</c>).</param>
        /// <returns>Новият баланс след дебита.</returns>
        Task<Result<decimal>> DebitAsync(int userId, decimal amount, string referenceId);

        /// <summary>
        /// Вътрешен метод – кредитира сума към wallet-а с тип <c>Win</c>.
        /// <para>Използва се от game services при печалба.</para>
        /// <para>
        /// Ако <paramref name="amount"/> ≤ 0 – не прави нищо и връща текущия баланс
        /// (за удобство на клиентите, които викат <c>CreditAsync</c> дори при 0 печалба).
        /// </para>
        /// </summary>
        /// <param name="userId">Id на потребителя.</param>
        /// <param name="amount">Сумата за кредитиране (може да е 0).</param>
        /// <param name="referenceId">Референция към рунда.</param>
        /// <returns>Новият баланс след кредита.</returns>
        Task<Result<decimal>> CreditAsync(int userId, decimal amount, string referenceId);
    }

    /// <summary>
    /// Имплементация на <see cref="IWalletService"/>.
    /// <para>
    /// Всяка операция, която променя баланса, минава през <see cref="ApplyAsync"/> –
    /// единственото място, което пише в <c>Wallets</c> и <c>Transactions</c>.
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
            // Нормализация на пагинацията
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
        /// Връща текущия баланс на потребителя (без projection към DTO).
        /// </summary>
        private async Task<decimal> GetBalanceAsync(int userId)
        {
            return await _db.Wallets
                .Where(w => w.UserId == userId)
                .Select(w => w.Balance)
                .FirstAsync();
        }

        /// <summary>
        /// Връща Id на тип транзакция по име (напр. <c>"Deposit"</c>, <c>"Bet"</c>, <c>"Win"</c>).
        /// </summary>
        private async Task<int> GetTransactionTypeId(string name)
        {
            return await _db.TransactionTypes
                .Where(t => t.Name == name)
                .Select(t => t.Id)
                .FirstAsync();
        }

        /// <summary>
        /// Прилага атомарна промяна на баланса:
        /// <list type="number">
        ///   <item>Обновява <c>Wallet.Balance</c> и <c>UpdatedAt</c>.</item>
        ///   <item>Създава <c>Transaction</c> ред с типа, сумата, баланса след операцията и референцията.</item>
        ///   <item>Записва промените в базата.</item>
        /// </list>
        /// <para>
        /// Това е единственото място в service-а, което пише в <c>Wallets</c> и <c>Transactions</c>.
        /// </para>
        /// </summary>
        /// <param name="userId">Id на потребителя.</param>
        /// <param name="delta">Промяна на баланса (положителна за кредит, отрицателна за дебит).</param>
        /// <param name="typeId">Id на типа транзакция (от <c>TransactionTypes</c>).</param>
        /// <param name="referenceId">Референция към рунда или <c>null</c> за deposit/withdraw.</param>
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
