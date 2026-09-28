namespace OnlineCasino.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using OnlineCasino.DTOs.Wallet;
    using OnlineCasino.Services;

    /// <summary>
    /// Wallet endpoints – баланс, история на транзакции и демо deposit/withdraw.
    /// <para>
    /// Всички endpoints работят върху wallet-а на текущо логнатия потребител.
    /// Балансът се променя <b>само</b> през <see cref="IWalletService"/> – никога директно.
    /// </para>
    /// <para>
    /// Deposit и Withdraw са <b>демо</b> – изпълняват се instant, без payment provider,
    /// без webhook, без admin approval. В production тези операции минават през
    /// payment gateway и webhook-ове.
    /// </para>
    /// </summary>
    [Authorize]
    public class WalletController : BaseApiController
    {
        private readonly IWalletService _wallet;

        public WalletController(IWalletService wallet) => _wallet = wallet;

        /// <summary>
        /// Връща текущия баланс и валутата на потребителя.
        /// <para>
        /// Използва се от React Native за header-а (баланс в горния ъгъл)
        /// и за валидация преди залог.
        /// </para>
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _wallet.GetWalletAsync(userId.Value));
        }

        /// <summary>
        /// Връща пагинирана история на транзакциите на потребителя.
        /// <para>
        /// Всяка транзакция съдържа: тип (<c>Deposit</c>, <c>Withdraw</c>, <c>Bet</c>, <c>Win</c>),
        /// сума (със знак), баланс след операцията и timestamp.
        /// </para>
        /// <para>
        /// Сортирани по <c>CreatedAt</c> descending (най-новите първи).
        /// </para>
        /// </summary>
        /// <param name="page">Номер на страница (започва от 1). Default: 1.</param>
        /// <param name="pageSize">Брой записи на страница (1–100). Default: 20.</param>
        [HttpGet("transactions")]
        public async Task<IActionResult> Transactions([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _wallet.GetTransactionsAsync(userId.Value, page, pageSize));
        }

        /// <summary>
        /// Демо депозит – кредитира сумата веднага, без payment provider.
        /// <para>
        /// Създава <c>Transaction</c> с тип <c>Deposit</c> и референция <c>null</c>.
        /// </para>
        /// <para>
        /// В production този endpoint трябва да създава pending payment и да чака webhook.
        /// </para>
        /// </summary>
        /// <param name="req">Заявка с <c>Amount</c> (трябва да е положителна).</param>
        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] MoneyRequest req)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _wallet.DepositAsync(userId.Value, req.Amount));
        }

        /// <summary>
        /// Демо withdraw – дебитира сумата веднага, ако има достатъчен баланс.
        /// <para>
        /// Създава <c>Transaction</c> с тип <c>Withdraw</c> и отрицателна сума.
        /// </para>
        /// <para>
        /// В production този endpoint трябва да създава pending withdrawal,
        /// който минава през Finance approval и payment provider.
        /// </para>
        /// </summary>
        /// <param name="req">Заявка с <c>Amount</c> (трябва да е положителна и ≤ баланса).</param>
        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] MoneyRequest req)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _wallet.WithdrawAsync(userId.Value, req.Amount));
        }
    }
}