namespace OnlineCasino.Tests.Integration;

using System.Net;
using System.Net.Http.Json;

using Xunit;
using FluentAssertions;

using OnlineCasino.DTOs.Slots;
using OnlineCasino.Tests.Common.Fixtures;
 
public sealed class SlotsControllerTests : IntegrationTestBase
{
    [Fact]
    public async Task Spin_Valid_ReturnsReelsAndRound()
    {
        SetUser(1);

        var response = await PostJsonAsync("/api/games/slots/slots/spin",
            new SpinRequest { BetAmount = 10, ClientRoundId = "round-1" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SpinResponse>();
        body!.Reels.Should().HaveCount(3);
        body.BetAmount.Should().Be(10);
    }

    [Fact]
    public async Task Spin_NonPositiveBet_Returns400()
    {
        SetUser(1);

        var response = await PostJsonAsync("/api/games/slots/slots/spin",
            new SpinRequest { BetAmount = 0, ClientRoundId = "r1" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Spin_UnknownGame_Returns404()
    {
        SetUser(1);

        var response = await PostJsonAsync("/api/games/slots/unknown/spin",
            new SpinRequest { BetAmount = 10, ClientRoundId = "r1" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Spin_Slots4_ReturnsFourReels()
    {
        SetUser(1);

        var response = await PostJsonAsync("/api/games/slots/slots4/spin",
            new SpinRequest { BetAmount = 10, ClientRoundId = "r1" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<SpinResponse>();
        body!.Reels.Should().HaveCount(4);
    }

    [Fact]
    public async Task Spin_DeductsBetFromWallet()
    {
        SetUser(1);

        await PostJsonAsync("/api/games/slots/slots/spin",
            new OnlineCasino.DTOs.Slots.SpinRequest { BetAmount = 10, ClientRoundId = "r1" });

        var response = await Client.GetAsync("/api/wallet");
        var wallet = await response.Content.ReadFromJsonAsync<OnlineCasino.DTOs.Wallet.WalletDto>();

        wallet!.Balance.Should().BeLessOrEqualTo(1000m);
    }

    [Fact]
    public async Task Spin_CreatesGameRoundInHistory()
    {
        SetUser(1);

        await PostJsonAsync("/api/games/slots/slots/spin",
            new OnlineCasino.DTOs.Slots.SpinRequest { BetAmount = 10, ClientRoundId = "r1" });

        var response = await Client.GetAsync("/api/game-history/slots");
        var body = await response.Content
            .ReadFromJsonAsync<OnlineCasino.Common.Models.PagedResult<OnlineCasino.DTOs.History.GameRoundDto>>();

        body!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Spin_ResultContainsReels()
    {
        SetUser(1);

        var response = await PostJsonAsync("/api/games/slots/slots/spin",
            new OnlineCasino.DTOs.Slots.SpinRequest { BetAmount = 10, ClientRoundId = "r1" });

        var body = await response.Content.ReadFromJsonAsync<OnlineCasino.DTOs.Slots.SpinResponse>();
        body!.Reels.Should().HaveCount(3);
        body.MatchCount.Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task Spin_BetAboveMax_Returns400()
    {
        SetUser(1);

        var response = await PostJsonAsync("/api/games/slots/slots/spin",
            new OnlineCasino.DTOs.Slots.SpinRequest { BetAmount = 1000, ClientRoundId = "r1" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Spin_InsufficientBalance_Returns400()
    {
        SetUser(1);
        await PostJsonAsync("/api/wallet/withdraw",
            new OnlineCasino.DTOs.Wallet.MoneyRequest { Amount = 995m });

        var response = await PostJsonAsync("/api/games/slots/slots/spin",
            new OnlineCasino.DTOs.Slots.SpinRequest { BetAmount = 100, ClientRoundId = "r1" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Spin_ThenWallet_ReflectsDebitAndPossibleWin()
    {
        SetUser(1);

        var spin = await PostJsonAsync("/api/games/slots/slots/spin",
            new OnlineCasino.DTOs.Slots.SpinRequest { BetAmount = 10, ClientRoundId = "r1" });
        var spinBody = await spin.Content.ReadFromJsonAsync<OnlineCasino.DTOs.Slots.SpinResponse>();

        var wallet = await Client.GetAsync("/api/wallet");
        var walletBody = await wallet.Content.ReadFromJsonAsync<OnlineCasino.DTOs.Wallet.WalletDto>();

        walletBody!.Balance.Should().Be(1000m - 10m + spinBody!.WinAmount);
    }

    [Fact]
    public async Task Spin_ThenHistory_ContainsExactlyOneRound()
    {
        SetUser(1);

        await PostJsonAsync("/api/games/slots/slots/spin",
            new OnlineCasino.DTOs.Slots.SpinRequest { BetAmount = 10, ClientRoundId = "r1" });

        var history = await Client.GetAsync("/api/game-history/slots");
        var body = await history.Content.ReadFromJsonAsync<OnlineCasino.Common.Models.PagedResult<OnlineCasino.DTOs.History.GameRoundDto>>();

        body!.Items.Should().HaveCount(1);
        body.Items[0].Game.Should().Be("slots");
    }

    [Fact]
    public async Task Spin_DisabledGame_Returns404()
    {
        SetUser(1);

        var response = await PostJsonAsync("/api/games/slots/disabled/spin",
            new OnlineCasino.DTOs.Slots.SpinRequest { BetAmount = 10, ClientRoundId = "r1" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Spin_NoAuthCookie_Returns401()
    {
        using var factory = new SqliteWebApplicationFactory(withAuth: false);
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/games/slots/slots/spin",
            new OnlineCasino.DTOs.Slots.SpinRequest
            {
                BetAmount = 10,
                ClientRoundId = "r1"
            });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Spin_MalformedJson_Returns400()
    {
        SetUser(1);

        var content = new StringContent("{ invalid",
            System.Text.Encoding.UTF8, "application/json");
        var response = await Client.PostAsync("/api/games/slots/slots/spin", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Spin_EmptyGameCode_Returns404()
    {
        SetUser(1);

        var response = await PostJsonAsync("/api/games/slots//spin",
            new OnlineCasino.DTOs.Slots.SpinRequest
            {
                BetAmount = 10,
                ClientRoundId = "r1"
            });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}