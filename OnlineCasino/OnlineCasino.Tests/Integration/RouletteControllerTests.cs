namespace OnlineCasino.Tests.Integration;

using System.Net;
using System.Net.Http.Json;

using Xunit;
using FluentAssertions;

using OnlineCasino.DTOs.Roulette;
using OnlineCasino.Tests.Common.Fixtures;

public sealed class RouletteControllerTests : IntegrationTestBase
{
    [Fact]
    public async Task GetBets_Empty_ReturnsEmptyList()
    {
        SetUser(1);

        var response = await Client.GetAsync("/api/games/roulette/roulette/bets");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<List<CurrentBetDto>>();
        list.Should().BeEmpty();
    }

    [Fact]
    public async Task PlaceBet_Valid_ReturnsBet()
    {
        SetUser(1);

        var response = await PostJsonAsync("/api/games/roulette/roulette/bet",
            new PlaceBetRequest { Type = "red", Amount = 10 });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var bet = await response.Content.ReadFromJsonAsync<CurrentBetDto>();
        bet!.Type.Should().Be("red");
        bet.Amount.Should().Be(10);
    }

    [Fact]
    public async Task PlaceBet_InvalidType_Returns400()
    {
        SetUser(1);

        var response = await PostJsonAsync("/api/games/roulette/roulette/bet",
            new PlaceBetRequest { Type = "purple", Amount = 10 });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Spin_NoBets_Returns400()
    {
        SetUser(1);

        var response = await PostJsonAsync("/api/games/roulette/roulette/spin",
            new SpinRouletteRequest { ClientRoundId = "r1" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Spin_WithBet_ReturnsResult()
    {
        SetUser(1);
        await PostJsonAsync("/api/games/roulette/roulette/bet",
            new PlaceBetRequest { Type = "red", Amount = 10 });

        var response = await PostJsonAsync("/api/games/roulette/roulette/spin",
            new SpinRouletteRequest { ClientRoundId = "round-1" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<RouletteSpinResponse>();
        body!.WinningNumber.Should().BeInRange(0, 37);
        body.TotalBet.Should().Be(10);
    }

    [Fact]
    public async Task PlaceBet_MultipleBets_AllListed()
    {
        SetUser(1);

        await PostJsonAsync("/api/games/roulette/roulette/bet",
            new PlaceBetRequest { Type = "red", Amount = 10 });
        await PostJsonAsync("/api/games/roulette/roulette/bet",
            new PlaceBetRequest { Type = "black", Amount = 20 });

        var response = await Client.GetAsync("/api/games/roulette/roulette/bets");
        var list = await response.Content.ReadFromJsonAsync<List<CurrentBetDto>>();

        list!.Should().HaveCount(2);
    }

    [Fact]
    public async Task Spin_AfterPlaceBet_ReturnsResult()
    {
        SetUser(1);
        await PostJsonAsync("/api/games/roulette/roulette/bet",
            new PlaceBetRequest { Type = "red", Amount = 10 });

        var response = await PostJsonAsync("/api/games/roulette/roulette/spin",
            new SpinRouletteRequest { ClientRoundId = "r1" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<RouletteSpinResponse>();
        body!.WinningColor.Should().BeOneOf("red", "black", "green");
        body.TotalBet.Should().Be(10m);
    }

    [Fact]
    public async Task Spin_ClearsBets()
    {
        SetUser(1);
        await PostJsonAsync("/api/games/roulette/roulette/bet",
            new PlaceBetRequest { Type = "red", Amount = 10 });
        await PostJsonAsync("/api/games/roulette/roulette/spin",
            new SpinRouletteRequest { ClientRoundId = "r1" });

        var response = await Client.GetAsync("/api/games/roulette/roulette/bets");
        var list = await response.Content.ReadFromJsonAsync<List<CurrentBetDto>>();

        list.Should().BeEmpty();
    }

    [Fact]
    public async Task Spin_CreatesGameRound()
    {
        SetUser(1);
        await PostJsonAsync("/api/games/roulette/roulette/bet",
            new PlaceBetRequest { Type = "red", Amount = 10 });
        await PostJsonAsync("/api/games/roulette/roulette/spin",
            new SpinRouletteRequest { ClientRoundId = "r1" });

        var response = await Client.GetAsync("/api/game-history/roulette");
        var body = await response.Content
            .ReadFromJsonAsync<OnlineCasino.Common.Models.PagedResult<OnlineCasino.DTOs.History.GameRoundDto>>();

        body!.Items.Should().HaveCount(1);
        body.Items.Single().Game.Should().Be("roulette");
        body.Items.Single().BetAmount.Should().Be(10m);
    }

    [Fact]
    public async Task PlaceBet_NumberOutOfRange_Returns400()
    {
        SetUser(1);

        var response = await PostJsonAsync("/api/games/roulette/roulette/bet",
            new PlaceBetRequest { Type = "number", Value = 99, Amount = 10 });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PlaceBet_DozenOutOfRange_Returns400()
    {
        SetUser(1);

        var response = await PostJsonAsync("/api/games/roulette/roulette/bet",
            new PlaceBetRequest { Type = "dozen", Value = 5, Amount = 10 });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Spin_ThenHistory_ContainsRound()
    {
        SetUser(1);
        await PostJsonAsync("/api/games/roulette/roulette/bet",
            new PlaceBetRequest { Type = "red", Amount = 10 });

        await PostJsonAsync("/api/games/roulette/roulette/spin",
            new SpinRouletteRequest { ClientRoundId = "r1" });

        var history = await Client.GetAsync("/api/game-history/roulette");
        var body = await history.Content.ReadFromJsonAsync<OnlineCasino.Common.Models.PagedResult<DTOs.History.GameRoundDto>>();

        body!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task PlaceBet_AfterSpin_StartsFreshBetsTable()
    {
        SetUser(1);
        await PostJsonAsync("/api/games/roulette/roulette/bet",
            new PlaceBetRequest { Type = "red", Amount = 10 });
        await PostJsonAsync("/api/games/roulette/roulette/spin",
            new SpinRouletteRequest { ClientRoundId = "r1" });

        var response = await Client.GetAsync("/api/games/roulette/roulette/bets");
        var list = await response.Content.ReadFromJsonAsync<List<CurrentBetDto>>();

        list.Should().BeEmpty();
    }
}