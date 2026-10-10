namespace OnlineCasino.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using OnlineCasino.DTOs.Wallet;
    using OnlineCasino.Services;

    /// <summary>
    /// Wallet endpoints – balance, transaction history, and demo deposit/withdraw.
    /// <para>
    /// All endpoints operate on the wallet of the currently logged-in user.
    /// The balance is changed <b>only</b> via <see cref="IWalletService"/> – never directly.
    /// </para>
    /// <para>
    /// Deposit and Withdraw are <b>demo</b> – they execute instantly, without a payment provider,
    /// without webhooks, without admin approval. In production, these operations go through
    /// a payment gateway and webhooks.
    /// </para>
    /// </summary>
    [Authorize]
    public class WalletController : BaseApiController
    {
        private readonly IWalletService _wallet;

        /// <summary>
        /// Initializes a new instance of the <see cref="WalletController"/> class.
        /// </summary>
        /// <param name="wallet">The wallet service used for all balance operations.</param>
        public WalletController(IWalletService wallet) => _wallet = wallet;

        /// <summary>
        /// Returns the user's current balance and currency.
        /// <para>
        /// Used by React Native for the header (balance in the top corner)
        /// and for validation before placing a bet.
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
        /// Returns a paginated history of the user's transactions.
        /// <para>
        /// Each transaction contains: type (<c>Deposit</c>, <c>Withdraw</c>, <c>Bet</c>, <c>Win</c>),
        /// amount (with sign), balance after the operation, and timestamp.
        /// </para>
        /// <para>
        /// Sorted by <c>CreatedAt</c> descending (newest first).
        /// </para>
        /// </summary>
        /// <param name="page">Page number (starts at 1). Default: 1.</param>
        /// <param name="pageSize">Number of records per page (1–100). Default: 20.</param>
        [HttpGet("transactions")]
        public async Task<IActionResult> Transactions([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _wallet.GetTransactionsAsync(userId.Value, page, pageSize));
        }

        /// <summary>
        /// Demo deposit – credits the amount instantly, without a payment provider.
        /// <para>
        /// Creates a <c>Transaction</c> with type <c>Deposit</c> and a <c>null</c> reference.
        /// </para>
        /// <para>
        /// In production, this endpoint should create a pending payment and wait for a webhook.
        /// </para>
        /// </summary>
        /// <param name="req">Request containing <c>Amount</c> (must be positive).</param>
        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] MoneyRequest req)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _wallet.DepositAsync(userId.Value, req.Amount));
        }

        /// <summary>
        /// Demo withdraw – debits the amount instantly, if there is sufficient balance.
        /// <para>
        /// Creates a <c>Transaction</c> with type <c>Withdraw</c> and a negative amount.
        /// </para>
        /// <para>
        /// In production, this endpoint should create a pending withdrawal
        /// that goes through Finance approval and a payment provider.
        /// </para>
        /// </summary>
        /// <param name="req">Request containing <c>Amount</c> (must be positive and ≤ the balance).</param>
        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] MoneyRequest req)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized(new { message = "Unauthorized." });
            return AsActionResult(await _wallet.WithdrawAsync(userId.Value, req.Amount));
        }
    }
}