namespace OnlineCasino.Tests.Integration;

using System.Net;
using System.Net.Http.Json;

using Xunit;
using FluentAssertions;

using OnlineCasino.DTOs.Blackjack;
using OnlineCasino.Tests.Common.Fixtures;

public sealed class BlackjackControllerTests : IntegrationTestBase
{
    [Fact]
    public async Task Start_Valid_ReturnsInitialState()
    {
        SetUser(1);

        var response = await PostJsonAsync("/api/games/blackjack/blackjack/start",
            new StartBlackjackRequest { BetAmount = 10 });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var state = await response.Content.ReadFromJsonAsync<BlackjackStateDto>();
        state!.PlayerHand.Should().HaveCount(2);
        state.DealerHand.Should().HaveCount(2);
        state.SessionId.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Start_NonPositiveBet_Returns400()
    {
        SetUser(1);

        var response = await PostJsonAsync("/api/games/blackjack/blackjack/start",
            new StartBlackjackRequest { BetAmount = -5 });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Start_UnknownGame_Returns404()
    {
        SetUser(1);

        var response = await PostJsonAsync("/api/games/blackjack/unknown/start",
            new StartBlackjackRequest { BetAmount = 10 });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Hit_UnknownSession_Returns404()
    {
        SetUser(1);

        var response = await Client.PostAsync("/api/games/blackjack/sessions/missing/hit", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Stand_UnknownSession_Returns404()
    {
        SetUser(1);

        var response = await Client.PostAsync("/api/games/blackjack/sessions/missing/stand", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Double_UnknownSession_Returns404()
    {
        SetUser(1);

        var response = await Client.PostAsync("/api/games/blackjack/sessions/missing/double", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetState_UnknownSession_Returns404()
    {
        SetUser(1);

        var response = await Client.GetAsync("/api/games/blackjack/sessions/missing");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetState_ExistingSession_ReturnsState()
    {
        SetUser(1);
        var start = await PostJsonAsync("/api/games/blackjack/blackjack/start",
            new StartBlackjackRequest { BetAmount = 10 });
        var state = await start.Content.ReadFromJsonAsync<BlackjackStateDto>();

        var response = await Client.GetAsync(
            $"/api/games/blackjack/sessions/{state!.SessionId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Stand_PlayerTurn_FinishesRound()
    {
        SetUser(1);
        var start = await PostJsonAsync("/api/games/blackjack/blackjack/start",
            new StartBlackjackRequest { BetAmount = 10 });
        var state = await start.Content.ReadFromJsonAsync<BlackjackStateDto>();

        if (state!.Status != "PlayerTurn") return;

        var response = await Client.PostAsync(
            $"/api/games/blackjack/sessions/{state.SessionId}/stand", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var finished = await response.Content.ReadFromJsonAsync<BlackjackStateDto>();
        finished!.Status.Should().Be("Finished");
    }

    [Fact]
    public async Task Start_DebitsBalanceImmediately()
    {
        SetUser(1);

        await PostJsonAsync("/api/games/blackjack/blackjack/start",
            new OnlineCasino.DTOs.Blackjack.StartBlackjackRequest { BetAmount = 50 });

        var response = await Client.GetAsync("/api/wallet");
        var wallet = await response.Content.ReadFromJsonAsync<OnlineCasino.DTOs.Wallet.WalletDto>();

        wallet!.Balance.Should().BeLessOrEqualTo(950m);
    }

    [Fact]
    public async Task Start_BetBelowMin_Returns400()
    {
        SetUser(1);

        var response = await PostJsonAsync("/api/games/blackjack/blackjack/start",
            new OnlineCasino.DTOs.Blackjack.StartBlackjackRequest { BetAmount = 1 });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Start_BetAboveMax_Returns400()
    {
        SetUser(1);

        var response = await PostJsonAsync("/api/games/blackjack/blackjack/start",
            new OnlineCasino.DTOs.Blackjack.StartBlackjackRequest { BetAmount = 9999 });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Start_InsufficientBalance_Returns400()
    {
        SetUser(1);
        await PostJsonAsync("/api/wallet/withdraw",
            new OnlineCasino.DTOs.Wallet.MoneyRequest { Amount = 995m });

        var response = await PostJsonAsync("/api/games/blackjack/blackjack/start",
            new OnlineCasino.DTOs.Blackjack.StartBlackjackRequest { BetAmount = 100 });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Stand_RevealsDealerSecondCard()
    {
        SetUser(1);
        var start = await PostJsonAsync("/api/games/blackjack/blackjack/start",
            new OnlineCasino.DTOs.Blackjack.StartBlackjackRequest { BetAmount = 10 });
        var state = await start.Content.ReadFromJsonAsync<OnlineCasino.DTOs.Blackjack.BlackjackStateDto>();

        if (state!.Status != "PlayerTurn") return;

        var response = await Client.PostAsync(
            $"/api/games/blackjack/sessions/{state.SessionId}/stand", null);
        var finished = await response.Content.ReadFromJsonAsync<OnlineCasino.DTOs.Blackjack.BlackjackStateDto>();

        finished!.DealerHand[1].Should().NotBe("?");
    }

    [Fact]
    public async Task Stand_WritesGameRound()
    {
        SetUser(1);
        var start = await PostJsonAsync("/api/games/blackjack/blackjack/start",
            new OnlineCasino.DTOs.Blackjack.StartBlackjackRequest { BetAmount = 10 });
        var state = await start.Content.ReadFromJsonAsync<OnlineCasino.DTOs.Blackjack.BlackjackStateDto>();

        if (state!.Status != "PlayerTurn") return;

        await Client.PostAsync($"/api/games/blackjack/sessions/{state.SessionId}/stand", null);

        var history = await Client.GetAsync("/api/game-history/blackjack");
        var body = await history.Content.ReadFromJsonAsync<OnlineCasino.Common.Models.PagedResult<OnlineCasino.DTOs.History.GameRoundDto>>();

        body!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Hit_OtherUsersSession_Returns404()
    {
        SetUser(1);
        var start = await PostJsonAsync("/api/games/blackjack/blackjack/start",
            new OnlineCasino.DTOs.Blackjack.StartBlackjackRequest { BetAmount = 10 });
        var state = await start.Content.ReadFromJsonAsync<OnlineCasino.DTOs.Blackjack.BlackjackStateDto>();

        SqliteWebApplicationFactory.SetUser(2);
        var response = await Client.PostAsync(
            $"/api/games/blackjack/sessions/{state!.SessionId}/hit", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Start_ThenWallet_DebitedByBetAmount()
    {
        SetUser(1);

        await PostJsonAsync("/api/games/blackjack/blackjack/start",
            new OnlineCasino.DTOs.Blackjack.StartBlackjackRequest { BetAmount = 50 });

        var wallet = await Client.GetAsync("/api/wallet");
        var walletBody = await wallet.Content.ReadFromJsonAsync<OnlineCasino.DTOs.Wallet.WalletDto>();

        walletBody!.Balance.Should().BeLessOrEqualTo(950m);
    }

    [Fact]
    public async Task Stand_ThenHistory_ContainsRound()
    {
        SetUser(1);
        var start = await PostJsonAsync("/api/games/blackjack/blackjack/start",
            new OnlineCasino.DTOs.Blackjack.StartBlackjackRequest { BetAmount = 10 });
        var state = await start.Content.ReadFromJsonAsync<OnlineCasino.DTOs.Blackjack.BlackjackStateDto>();

        if (state!.Status != "PlayerTurn") return;

        await Client.PostAsync($"/api/games/blackjack/sessions/{state.SessionId}/stand", null);

        var history = await Client.GetAsync("/api/game-history/blackjack");
        var body = await history.Content.ReadFromJsonAsync<OnlineCasino.Common.Models.PagedResult<OnlineCasino.DTOs.History.GameRoundDto>>();

        body!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Start_NoAuthCookie_Returns401()
    {
        using var factory = new SqliteWebApplicationFactory(withAuth: false);
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/games/blackjack/blackjack/start",
            new OnlineCasino.DTOs.Blackjack.StartBlackjackRequest { BetAmount = 10 });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Hit_NoAuthCookie_Returns401()
    {
        using var factory = new SqliteWebApplicationFactory(withAuth: false);
        var client = factory.CreateClient();

        var response = await client.PostAsync("/api/games/blackjack/sessions/abc/hit", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Stand_NoAuthCookie_Returns401()
    {
        using var factory = new SqliteWebApplicationFactory(withAuth: false);
        var client = factory.CreateClient();

        var response = await client.PostAsync("/api/games/blackjack/sessions/abc/stand", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetState_NoAuthCookie_Returns401()
    {
        using var factory = new SqliteWebApplicationFactory(withAuth: false);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/games/blackjack/sessions/abc");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
