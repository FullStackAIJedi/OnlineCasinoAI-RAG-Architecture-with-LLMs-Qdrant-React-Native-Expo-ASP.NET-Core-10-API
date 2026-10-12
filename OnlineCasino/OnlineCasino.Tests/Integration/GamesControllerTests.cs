namespace OnlineCasino.Tests.Integration;

using System.Net;
using System.Net.Http.Json;

using Xunit;
using FluentAssertions;

using OnlineCasino.DTOs.Games;
using OnlineCasino.Tests.Common.Fixtures;

public sealed class GamesControllerTests : IntegrationTestBase
{
    [Fact]
    public async Task GetAll_Authenticated_ReturnsOnlyEnabled()
    {
        SetUser(1);

        var response = await Client.GetAsync("/api/games");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<List<GameDto>>();
        list!.Should().HaveCount(4);
        list.Should().NotContain(g => g.Code == "disabled");
    }

    [Fact]
    public async Task GetByCode_Existing_ReturnsGame()
    {
        SetUser(1);

        var response = await Client.GetAsync("/api/games/slots");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var game = await response.Content.ReadFromJsonAsync<GameDto>();
        game!.Code.Should().Be("slots");
    }

    [Fact]
    public async Task GetByCode_Missing_Returns404()
    {
        SetUser(1);

        var response = await Client.GetAsync("/api/games/unknown");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByCode_Slots4_ReturnsFourReelsConfig()
    {
        SetUser(1);

        var response = await Client.GetAsync("/api/games/slots4");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var game = await response.Content.ReadFromJsonAsync<OnlineCasino.DTOs.Games.GameDto>();
        game!.Code.Should().Be("slots4");
    }

    [Fact]
    public async Task GetAll_ContainsAllTypes()
    {
        SetUser(1);

        var response = await Client.GetAsync("/api/games");
        var list = await response.Content.ReadFromJsonAsync<List<OnlineCasino.DTOs.Games.GameDto>>();

        list!.Select(g => g.Type).Distinct().Should().BeEquivalentTo(
            new[] { "Slots", "Roulette", "Blackjack" });
    }

    [Fact]
    public async Task GetByCode_DisabledGame_ReturnsDisabledStatus()
    {
        SetUser(1);

        var response = await Client.GetAsync("/api/games/disabled");
        var game = await response.Content.ReadFromJsonAsync<OnlineCasino.DTOs.Games.GameDto>();

        game!.Enabled.Should().BeFalse();
    }

    [Fact]
    public async Task GetAll_ResponseDoesNotContainReelsCount()
    {
        SetUser(1);

        var response = await Client.GetAsync("/api/games");
        var body = await response.Content.ReadAsStringAsync();

        body.Should().NotContain("reelsCount");
    }

    [Fact]
    public async Task GetByCode_Response_DoesNotExposeInternalId()
    {
        SetUser(1);

        var response = await Client.GetAsync("/api/games/slots");
        var body = await response.Content.ReadAsStringAsync();

        body.Should().Contain("\"code\":\"slots\"");
        body.Should().Contain("\"enabled\":true");
    }

    [Fact]
    public async Task GetAll_NoAuthCookie_Returns401()
    {
        using var factory = new SqliteWebApplicationFactory(withAuth: false);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/games");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetByCode_NoAuthCookie_Returns401()
    {
        using var factory = new SqliteWebApplicationFactory(withAuth: false);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/games/slots");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
