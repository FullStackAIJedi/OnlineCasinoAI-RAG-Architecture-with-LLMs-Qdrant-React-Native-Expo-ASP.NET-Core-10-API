namespace OnlineCasino.Tests.Integration;

using System.Net;
using System.Net.Http.Json;

using Xunit;
using FluentAssertions;

using OnlineCasino.Common.Models;
using OnlineCasino.DTOs.Wallet;

using OnlineCasino.Tests.Common.Fixtures;
 
public sealed class WalletControllerTests : IntegrationTestBase
{
    [Fact]
    public async Task Get_Authenticated_ReturnsWallet()
    {
        SetUser(1);

        var response = await Client.GetAsync("/api/wallet");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<WalletDto>();
        body!.Balance.Should().Be(1000m);
        body.Currency.Should().Be("EUR");
    }

    [Fact]
    public async Task Transactions_Authenticated_ReturnsPagedResult()
    {
        SetUser(1);

        var response = await Client.GetAsync("/api/wallet/transactions?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResult<TransactionDto>>();
        body.Should().NotBeNull();
    }

    [Fact]
    public async Task Deposit_Valid_CreditsBalance()
    {
        SetUser(1);

        var response = await PostJsonAsync("/api/wallet/deposit",
            new MoneyRequest { Amount = 250m });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<WalletDto>();
        body!.Balance.Should().Be(1250m);
    }

    [Fact]
    public async Task Deposit_NonPositive_Returns400()
    {
        SetUser(1);

        var response = await PostJsonAsync("/api/wallet/deposit",
            new MoneyRequest { Amount = -10m });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Withdraw_Valid_DebitsBalance()
    {
        SetUser(1);

        var response = await PostJsonAsync("/api/wallet/withdraw",
            new MoneyRequest { Amount = 100m });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<WalletDto>();
        body!.Balance.Should().Be(900m);
    }

    [Fact]
    public async Task Withdraw_Insufficient_Returns400()
    {
        SetUser(1);

        var response = await PostJsonAsync("/api/wallet/withdraw",
            new MoneyRequest { Amount = 99999m });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Deposit_ThenGet_ReflectsNewBalance()
    {
        SetUser(1);

        await PostJsonAsync("/api/wallet/deposit",
            new OnlineCasino.DTOs.Wallet.MoneyRequest { Amount = 500m });

        var response = await Client.GetAsync("/api/wallet");
        var body = await response.Content.ReadFromJsonAsync<OnlineCasino.DTOs.Wallet.WalletDto>();

        body!.Balance.Should().Be(1500m);
    }

    [Fact]
    public async Task Deposit_CreatesTransaction()
    {
        SetUser(1);

        await PostJsonAsync("/api/wallet/deposit",
            new OnlineCasino.DTOs.Wallet.MoneyRequest { Amount = 100m });

        var response = await Client.GetAsync("/api/wallet/transactions");
        var body = await response.Content.ReadFromJsonAsync<OnlineCasino.Common.Models.PagedResult<OnlineCasino.DTOs.Wallet.TransactionDto>>();

        body!.Items.Should().HaveCount(1);
        body.Items[0].Type.Should().Be("Deposit");
        body.Items[0].Amount.Should().Be(100m);
    }

    [Fact]
    public async Task Withdraw_CreatesNegativeTransaction()
    {
        SetUser(1);

        await PostJsonAsync("/api/wallet/withdraw",
            new OnlineCasino.DTOs.Wallet.MoneyRequest { Amount = 200m });

        var response = await Client.GetAsync("/api/wallet/transactions");
        var body = await response.Content.ReadFromJsonAsync<OnlineCasino.Common.Models.PagedResult<OnlineCasino.DTOs.Wallet.TransactionDto>>();

        body!.Items[0].Type.Should().Be("Withdraw");
        body.Items[0].Amount.Should().Be(-200m);
    }

    [Fact]
    public async Task Transactions_RespectsPageSize()
    {
        SetUser(1);
        for (int i = 0; i < 5; i++)
        {
            await PostJsonAsync("/api/wallet/deposit",
                new OnlineCasino.DTOs.Wallet.MoneyRequest { Amount = 10m });
        }

        var response = await Client.GetAsync("/api/wallet/transactions?page=1&pageSize=2");
        var body = await response.Content.ReadFromJsonAsync<OnlineCasino.Common.Models.PagedResult<OnlineCasino.DTOs.Wallet.TransactionDto>>();

        body!.Items.Should().HaveCount(2);
        body.Total.Should().Be(5);
        body.PageSize.Should().Be(2);
    }

    [Fact]
    public async Task Deposit_Zero_Returns400()
    {
        SetUser(1);

        var response = await PostJsonAsync("/api/wallet/deposit",
            new OnlineCasino.DTOs.Wallet.MoneyRequest { Amount = 0m });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Withdraw_Zero_Returns400()
    {
        SetUser(1);

        var response = await PostJsonAsync("/api/wallet/withdraw",
            new OnlineCasino.DTOs.Wallet.MoneyRequest { Amount = 0m });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Withdraw_ExactBalance_Succeeds()
    {
        SetUser(1);

        var response = await PostJsonAsync("/api/wallet/withdraw",
            new OnlineCasino.DTOs.Wallet.MoneyRequest { Amount = 1000m });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<OnlineCasino.DTOs.Wallet.WalletDto>();
        body!.Balance.Should().Be(0m);
    }

    [Fact]
    public async Task Deposit_OnMissingWallet_Returns404()
    {
        SetUser(999);

        var response = await PostJsonAsync("/api/wallet/deposit",
            new OnlineCasino.DTOs.Wallet.MoneyRequest { Amount = 10m });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Deposit_NoAuthCookie_Returns401()
    {
        using var factory = new SqliteWebApplicationFactory(withAuth: false);
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/wallet/deposit",
            new OnlineCasino.DTOs.Wallet.MoneyRequest { Amount = 100m });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Withdraw_NoAuthCookie_Returns401()
    {
        using var factory = new SqliteWebApplicationFactory(withAuth: false);
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/wallet/withdraw",
            new OnlineCasino.DTOs.Wallet.MoneyRequest { Amount = 100m });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Transactions_NoAuthCookie_Returns401()
    {
        using var factory = new SqliteWebApplicationFactory(withAuth: false);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/wallet/transactions");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Deposit_MalformedJson_Returns400()
    {
        SetUser(1);

        var content = new StringContent("{ invalid json", System.Text.Encoding.UTF8, "application/json");
        var response = await Client.PostAsync("/api/wallet/deposit", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

}