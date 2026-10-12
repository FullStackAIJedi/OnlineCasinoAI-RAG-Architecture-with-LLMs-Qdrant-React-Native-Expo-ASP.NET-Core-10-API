namespace OnlineCasino.Tests.Unit.Services;

using Microsoft.EntityFrameworkCore;

using Xunit;
using FluentAssertions;

using OnlineCasino.Data;
using OnlineCasino.Entities;
using OnlineCasino.Services;

using OnlineCasino.Tests.Common.Fixtures;
using OnlineCasino.Tests.Common.TestData;
 
public sealed class WalletServiceTests
{
    private static async Task<(AppDbContext db, WalletService svc)> ArrangeAsync(decimal balance = 1000m)
    {
        var db = await TestDbFactory.CreateSeededAsync();
        db.Users.Add(TestUser.WithBalance(balance).ToEntity());
        await db.SaveChangesAsync();
        return (db, new WalletService(db));
    }

    [Fact]
    public async Task Debit_TwoConcurrentRequests_NeitherExceedsBalance()
    {
        var (db, svc) = await ArrangeAsync(100m);

        var t1 = svc.DebitAsync(1, 60m, "r1");
        var t2 = svc.DebitAsync(1, 60m, "r2");

        var results = await Task.WhenAll(t1, t2);

        results.Count(r => r.IsSuccess).Should().Be(1);
        (await db.Wallets.FindAsync(1))!.Balance.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public async Task GetWallet_Existing_ReturnsDto()
    {
        var (_, svc) = await ArrangeAsync(500m);

        var result = await svc.GetWalletAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Balance.Should().Be(500m);
        result.Data.Currency.Should().Be("EUR");
    }

    [Fact]
    public async Task GetWallet_Missing_Returns404()
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.GetWalletAsync(999);

        result.Error!.StatusCode.Should().Be(404);
    }

    [Theory]
    [InlineData(0, 0, 1, 20)]
    [InlineData(-1, -1, 1, 20)]
    [InlineData(1, 101, 1, 20)]
    [InlineData(3, 50, 3, 50)]
    public async Task GetTransactions_NormalizesPaging(int page, int size, int expectedPage, int expectedSize)
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.GetTransactionsAsync(1, page, size);

        result.Data!.Page.Should().Be(expectedPage);
        result.Data.PageSize.Should().Be(expectedSize);
    }

    [Fact]
    public async Task GetTransactions_OrdersDescending()
    {
        var (db, svc) = await ArrangeAsync();
        db.Transactions.AddRange(
            new Transaction
            {
                UserId = 1,
                TransactionTypeId = TestIds.DepositTypeId,
                Amount = 10,
                BalanceAfter = 1010,
                CreatedAt = TestClock.FixedNow.AddMinutes(-5)
            },
            new Transaction
            {
                UserId = 1,
                TransactionTypeId = TestIds.DepositTypeId,
                Amount = 20,
                BalanceAfter = 1030,
                CreatedAt = TestClock.FixedNow
            });
        await db.SaveChangesAsync();

        var result = await svc.GetTransactionsAsync(1, 1, 20);

        result.Data!.Items.Should().HaveCount(2);
        result.Data.Items[0].Amount.Should().Be(20);
        result.Data.Total.Should().Be(2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Deposit_NonPositive_Returns400(decimal amount)
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.DepositAsync(1, amount);

        result.Error!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Deposit_Valid_CreditsAndLogsTransaction()
    {
        var (db, svc) = await ArrangeAsync(100m);

        var result = await svc.DepositAsync(1, 50m);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Balance.Should().Be(150m);

        var tx = await db.Transactions.SingleAsync();
        tx.TransactionTypeId.Should().Be(TestIds.DepositTypeId);
        tx.Amount.Should().Be(50m);
        tx.ReferenceId.Should().BeNull();
    }

    [Fact]
    public async Task Deposit_MissingWallet_Returns404()
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.DepositAsync(999, 10);

        result.Error!.StatusCode.Should().Be(404);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Withdraw_NonPositive_Returns400(decimal amount)
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.WithdrawAsync(1, amount);

        result.Error!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Withdraw_MissingWallet_Returns404()
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.WithdrawAsync(999, 10);

        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Withdraw_Insufficient_Returns400()
    {
        var (_, svc) = await ArrangeAsync(10m);

        var result = await svc.WithdrawAsync(1, 50);

        result.Error!.StatusCode.Should().Be(400);
        result.Error.Message.Should().Contain("Insufficient");
    }

    [Fact]
    public async Task Withdraw_Valid_DebitsBalance()
    {
        var (_, svc) = await ArrangeAsync(100m);

        var result = await svc.WithdrawAsync(1, 30m);

        result.Data!.Balance.Should().Be(70m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Debit_NonPositive_Returns400(decimal amount)
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.DebitAsync(1, amount, "ref");

        result.Error!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Debit_MissingWallet_Returns404()
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.DebitAsync(999, 10, "ref");

        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Debit_Insufficient_Returns400()
    {
        var (_, svc) = await ArrangeAsync(5m);

        var result = await svc.DebitAsync(1, 10, "ref");

        result.Error!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Debit_Valid_ReturnsNewBalanceAndWritesBetTx()
    {
        var (db, svc) = await ArrangeAsync(100m);

        var result = await svc.DebitAsync(1, 25m, "round-42");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(75m);

        var tx = await db.Transactions.SingleAsync();
        tx.TransactionTypeId.Should().Be(TestIds.BetTypeId);
        tx.Amount.Should().Be(-25m);
        tx.ReferenceId.Should().Be("round-42");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Credit_NonPositive_ReturnsCurrentBalanceWithoutTx(decimal amount)
    {
        var (db, svc) = await ArrangeAsync(77m);

        var result = await svc.CreditAsync(1, amount, "ref");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(77m);
        (await db.Transactions.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Credit_Positive_AddsBalanceAndWritesWinTx()
    {
        var (db, svc) = await ArrangeAsync(50m);

        var result = await svc.CreditAsync(1, 30m, "round-9");

        result.Data.Should().Be(80m);

        var tx = await db.Transactions.SingleAsync();
        tx.TransactionTypeId.Should().Be(TestIds.WinTypeId);
        tx.Amount.Should().Be(30m);
        tx.ReferenceId.Should().Be("round-9");
    }

    [Fact]
    public async Task GetWallet_ExposesCurrencyEUR()
    {
        var (_, svc) = await ArrangeAsync(500m);
        var result = await svc.GetWalletAsync(1);

        result.Data!.Currency.Should().Be("EUR");
    }

    [Fact]
    public async Task GetTransactions_EmptyList_ReturnsZeroTotal()
    {
        var (_, svc) = await ArrangeAsync();

        var result = await svc.GetTransactionsAsync(1, 1, 20);

        result.Data!.Items.Should().BeEmpty();
        result.Data.Total.Should().Be(0);
    }

    [Fact]
    public async Task GetTransactions_ProjectedFieldsAreCorrect()
    {
        var (db, svc) = await ArrangeAsync();
        db.Transactions.Add(new OnlineCasino.Entities.Transaction
        {
            UserId = 1,
            TransactionTypeId = TestIds.DepositTypeId,
            Amount = 50m,
            BalanceAfter = 1050m,
            CreatedAt = TestClock.FixedNow,
            ReferenceId = "ref-1"
        });
        await db.SaveChangesAsync();

        var result = await svc.GetTransactionsAsync(1, 1, 20);

        result.Data!.Items[0].Type.Should().Be("Deposit");
        result.Data.Items[0].Amount.Should().Be(50m);
        result.Data.Items[0].BalanceAfter.Should().Be(1050m);
    }

    [Fact]
    public async Task GetTransactions_OnlyReturnsOwnTransactions()
    {
        var (db, svc) = await ArrangeAsync();
        db.Users.Add((TestUser.Valid with { Id = 2, Email = "u2@t.com", NickName = "u2" }).ToEntity());
        await db.SaveChangesAsync();

        db.Transactions.AddRange(
            new OnlineCasino.Entities.Transaction { UserId = 1, TransactionTypeId = 1, Amount = 10, BalanceAfter = 1010 },
            new OnlineCasino.Entities.Transaction { UserId = 2, TransactionTypeId = 1, Amount = 20, BalanceAfter = 1020 });
        await db.SaveChangesAsync();

        var result = await svc.GetTransactionsAsync(1, 1, 20);

        result.Data!.Items.Should().HaveCount(1);
        result.Data.Items[0].Amount.Should().Be(10);
    }

    [Fact]
    public async Task Deposit_SetsUpdatedAtOnWallet()
    {
        var (db, svc) = await ArrangeAsync(100m);
        var before = (await db.Wallets.FindAsync(1))!.UpdatedAt;

        await svc.DepositAsync(1, 10m);

        var after = (await db.Wallets.FindAsync(1))!.UpdatedAt;
        after.Should().BeAfter(before);
    }

    [Fact]
    public async Task Deposit_MultipleAccumulate()
    {
        var (db, svc) = await ArrangeAsync(0m);

        await svc.DepositAsync(1, 100m);
        await svc.DepositAsync(1, 50m);
        var result = await svc.DepositAsync(1, 25m);

        result.Data!.Balance.Should().Be(175m);
        (await db.Transactions.CountAsync()).Should().Be(3);
    }

    [Fact]
    public async Task Withdraw_ExactBalance_Succeeds()
    {
        var (_, svc) = await ArrangeAsync(100m);

        var result = await svc.WithdrawAsync(1, 100m);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Balance.Should().Be(0m);
    }

    [Fact]
    public async Task Withdraw_CreatesNegativeTransaction()
    {
        var (db, svc) = await ArrangeAsync(100m);

        await svc.WithdrawAsync(1, 30m);

        var tx = await db.Transactions.SingleAsync();
        tx.Amount.Should().Be(-30m);
        tx.TransactionTypeId.Should().Be(TestIds.WithdrawTypeId);
    }

    [Fact]
    public async Task Debit_SetsBalanceAfterInTransaction()
    {
        var (db, svc) = await ArrangeAsync(100m);

        await svc.DebitAsync(1, 25m, "ref");

        var tx = await db.Transactions.SingleAsync();
        tx.BalanceAfter.Should().Be(75m);
    }

    [Fact]
    public async Task Credit_SetsBalanceAfterInTransaction()
    {
        var (db, svc) = await ArrangeAsync(50m);

        await svc.CreditAsync(1, 30m, "ref");

        var tx = await db.Transactions.SingleAsync();
        tx.BalanceAfter.Should().Be(80m);
    }

    [Fact]
    public async Task DebitThenCredit_NetZero_BalanceRestored()
    {
        var (_, svc) = await ArrangeAsync(100m);

        await svc.DebitAsync(1, 50m, "r");
        var result = await svc.CreditAsync(1, 50m, "r");

        result.Data.Should().Be(100m);
    }

    [Fact]
    public async Task Withdraw_ExactBalance_LeavesZero_NotNegative()
    {
        var (db, svc) = await ArrangeAsync(100m);

        var result = await svc.WithdrawAsync(1, 100m);

        result.Data!.Balance.Should().Be(0m);
        (await db.Wallets.FindAsync(1))!.Balance.Should().Be(0m);
    }

    [Fact]
    public async Task Debit_BalanceAfterInTransaction_MatchesActualBalance()
    {
        var (db, svc) = await ArrangeAsync(100m);

        await svc.DebitAsync(1, 25m, "r");

        var tx = await db.Transactions.SingleAsync();
        var wallet = await db.Wallets.FindAsync(1);

        tx.BalanceAfter.Should().Be(wallet!.Balance);
        tx.BalanceAfter.Should().Be(75m);
    }
      
    [Fact]
    public async Task Credit_BalanceAfterInTransaction_MatchesActualBalance()
    {
        var (db, svc) = await ArrangeAsync(50m);

        await svc.CreditAsync(1, 30m, "r");

        var tx = await db.Transactions.SingleAsync();
        tx.BalanceAfter.Should().Be(80m);
        tx.Amount.Should().Be(30m);
    }

    [Fact]
    public async Task DepositAndWithdraw_TransactionChain_BalanceAfterIsMonotonic()
    {
        var (db, svc) = await ArrangeAsync(0m);

        await svc.DepositAsync(1, 100m);
        await svc.WithdrawAsync(1, 30m);
        await svc.DepositAsync(1, 50m);
        await svc.WithdrawAsync(1, 20m);

        var txs = await db.Transactions.OrderBy(t => t.Id).ToListAsync();
        txs.Select(t => t.BalanceAfter).Should().Equal(100m, 70m, 120m, 100m);
    }

    [Fact]
    public async Task DepositAndWithdraw_TransactionChain_BalanceAfterIsSequential()
    {
        var (db, svc) = await ArrangeAsync(0m);

        await svc.DepositAsync(1, 100m);
        await svc.WithdrawAsync(1, 30m);
        await svc.DepositAsync(1, 50m);
        await svc.WithdrawAsync(1, 20m);

        var txs = await db.Transactions.OrderBy(t => t.Id).ToListAsync();
        txs.Select(t => t.BalanceAfter).Should().Equal(100m, 70m, 120m, 100m);
    }

    [Fact]
    public async Task Wallet_UpdatedAt_ChangesOnEveryMutation()
    {
        var (db, svc) = await ArrangeAsync(100m);
        var t0 = (await db.Wallets.FindAsync(1))!.UpdatedAt;

        await Task.Delay(5);
        await svc.DepositAsync(1, 10m);
        var t1 = (await db.Wallets.FindAsync(1))!.UpdatedAt;

        await Task.Delay(5);
        await svc.WithdrawAsync(1, 5m);
        var t2 = (await db.Wallets.FindAsync(1))!.UpdatedAt;

        t1.Should().BeAfter(t0);
        t2.Should().BeAfter(t1);
    }

    [Fact]
    public async Task DepositAndDebit_DoNotAffectOtherUsersWallets()
    {
        var (db, svc) = await ArrangeAsync(100m);
        db.Users.Add((TestUser.Valid with { Id = 2, Email = "u2@t.com", NickName = "u2" }).ToEntity());
        await db.SaveChangesAsync();

        await svc.DepositAsync(1, 500m);
        await svc.DebitAsync(1, 50m, "r");

        var other = await db.Wallets.FindAsync(2);
        other!.Balance.Should().Be(1000m);
    }

    [Fact]
    public async Task Debit_WhenEqualBalance_SucceedsAndLeavesZero()
    {
        var (db, svc) = await ArrangeAsync(50m);

        var result = await svc.DebitAsync(1, 50m, "r");

        result.IsSuccess.Should().BeTrue();
        (await db.Wallets.FindAsync(1))!.Balance.Should().Be(0m);
    }

    [Fact]
    public async Task DepositThenDebit_BalanceAfter_ReflectsEachOperationChronologically()
    {
        var (db, svc) = await ArrangeAsync(0m);

        await svc.DepositAsync(1, 200m);
        await svc.DebitAsync(1, 30m, "bet-1");
        await svc.CreditAsync(1, 60m, "win-1");
        await svc.WithdrawAsync(1, 100m);

        var txs = await db.Transactions
            .OrderBy(t => t.Id)
            .Select(t => new { t.TransactionTypeId, t.Amount, t.BalanceAfter, t.ReferenceId })
            .ToListAsync();

        txs[0].TransactionTypeId.Should().Be(TestIds.DepositTypeId);
        txs[0].Amount.Should().Be(200m);
        txs[0].BalanceAfter.Should().Be(200m);
        txs[1].TransactionTypeId.Should().Be(TestIds.BetTypeId);
        txs[1].ReferenceId.Should().Be("bet-1");
        txs[1].BalanceAfter.Should().Be(170m);
        txs[2].TransactionTypeId.Should().Be(TestIds.WinTypeId);
        txs[2].ReferenceId.Should().Be("win-1");
        txs[2].BalanceAfter.Should().Be(230m);
        txs[3].TransactionTypeId.Should().Be(TestIds.WithdrawTypeId);
        txs[3].BalanceAfter.Should().Be(130m);
    }

    [Fact]
    public async Task Credit_ZeroWin_DoesNotWriteTransaction()
    {
        var (db, svc) = await ArrangeAsync(100m);

        var result = await svc.CreditAsync(1, 0m, "round-x");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(100m);
        (await db.Transactions.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Debit_NegativeBalance_Impossible_AfterManyWithdrawals()
    {
        var (db, svc) = await ArrangeAsync(100m);

        for (int i = 0; i < 20; i++)
        {
            await svc.WithdrawAsync(1, 10m);
        }

        (await db.Wallets.FindAsync(1))!.Balance.Should().Be(0m);
        var final = await svc.WithdrawAsync(1, 1m);
        final.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Deposit_UpdatesWalletUpdatedAt()
    {
        var (db, svc) = await ArrangeAsync(100m);
        var before = (await db.Wallets.FindAsync(1))!.UpdatedAt;
        await Task.Delay(10);

        await svc.DepositAsync(1, 10m);

        var after = (await db.Wallets.FindAsync(1))!.UpdatedAt;
        after.Should().BeAfter(before);
    }

    [Fact]
    public async Task Deposit_MultipleCallsInParallel_NoLostUpdate()
    {
        var (db, svc) = await ArrangeAsync(0m);

        var tasks = Enumerable.Range(1, 20)
            .Select(i => svc.DepositAsync(1, 5m))
            .ToArray();

        await Task.WhenAll(tasks);

        (await db.Wallets.FindAsync(1))!.Balance.Should().Be(100m);
        (await db.Transactions.CountAsync()).Should().Be(20);
    }

    [Fact]
    public async Task GetTransactions_Page2WithNoResults_ReturnsEmptyWithCorrectTotal()
    {
        var (db, svc) = await ArrangeAsync(100m);
        await svc.DepositAsync(1, 10m);

        var result = await svc.GetTransactionsAsync(1, page: 2, pageSize: 20);

        result.Data!.Items.Should().BeEmpty();
        result.Data.Total.Should().Be(1);
        result.Data.Page.Should().Be(2);
    }

    [Fact]
    public async Task Deposit_TenParallelDeposits_NoLostUpdate()
    {
        var (db, svc) = await ArrangeAsync(0m);

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => svc.DepositAsync(1, 100m))
            .ToArray();

        await Task.WhenAll(tasks);

        var wallet = await db.Wallets.FindAsync(1);
        wallet!.Balance.Should().Be(1000m);

        (await db.Transactions.CountAsync()).Should().Be(10);
    }
}
