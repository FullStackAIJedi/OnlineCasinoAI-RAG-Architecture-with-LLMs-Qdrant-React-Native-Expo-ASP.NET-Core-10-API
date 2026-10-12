namespace OnlineCasino.Tests.Integration;

using System.Net;
using System.Net.Http.Json;

using Xunit;
using FluentAssertions;

using OnlineCasino.Common.Models;
using OnlineCasino.DTOs.History;

using OnlineCasino.Tests.Common.Fixtures;
using OnlineCasino.Tests.Common.TestData;

public sealed class GameHistoryControllerTests : IntegrationTestBase
{
    [Fact]
    public async Task Get_Authenticated_ReturnsPagedResult()
    {
        SetUser(1);

        var response = await Client.GetAsync("/api/game-history");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResult<GameRoundDto>>();
        body.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSlots_FilteredByGameCode()
    {
        SetUser(1);

        var response = await Client.GetAsync("/api/game-history/slots");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetRoulette_FilteredByGameCode()
    {
        SetUser(1);

        var response = await Client.GetAsync("/api/game-history/roulette");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetBlackjack_FilteredByGameCode()
    {
        SetUser(1);

        var response = await Client.GetAsync("/api/game-history/blackjack");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetRound_Missing_Returns404()
    {
        SetUser(1);

        var response = await Client.GetAsync("/api/game-history/unknown");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRound_OwnedRound_ReturnsDto()
    {
        SetUser(1);
        var round = TestGameRound.WinningRound with { Id = 0, UserId = 1 };
        await Factory.ExecuteDbAsync(async db =>
        {
            db.GameRounds.Add(round.ToEntity());
            await db.SaveChangesAsync();
        });

        var response = await Client.GetAsync($"/api/game-history/{round.PublicId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await response.Content.ReadFromJsonAsync<GameRoundDto>();
        dto!.RoundId.Should().Be(round.PublicId);
    }

    [Fact]
    public async Task Get_ReturnsOnlyUserRounds()
    {
        SetUser(1);
        await SeedUserAsync(TestUser.Valid with { Id = 2, Email = "u2@t.com", NickName = "u2" });
        await Factory.ExecuteDbAsync(async db =>
        {
            db.GameRounds.Add((TestGameRound.WinningRound with { Id = 0, UserId = 1 }).ToEntity());
            db.GameRounds.Add((TestGameRound.WinningRound with { Id = 0, UserId = 2 }).ToEntity());
            await db.SaveChangesAsync();
        });

        var response = await Client.GetAsync("/api/game-history");
        var body = await response.Content.ReadFromJsonAsync<OnlineCasino.Common.Models.PagedResult<OnlineCasino.DTOs.History.GameRoundDto>>();

        body!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetRound_OtherUsersRound_Returns404()
    {
        SetUser(1);
        await Factory.ExecuteDbAsync(async db =>
        {
            db.Users.Add((TestUser.Valid with
            {
                Id = null,
                Email = "other@test.com",
                NickName = "other"
            }).ToEntity());
            await db.SaveChangesAsync();

            var otherUserId = db.Users.Single(u => u.NickName == "other").Id;

            db.GameRounds.Add((TestGameRound.WinningRound with
            {
                Id = 0,
                UserId = otherUserId
            }).ToEntity());
            await db.SaveChangesAsync();
        });

        var otherRound = await Factory.QueryDbAsync(db =>
            Task.FromResult(db.GameRounds.First()));

        var response = await Client.GetAsync($"/api/game-history/{otherRound.PublicId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_Pagination_ReturnsCorrectPage()
    {
        SetUser(1);
        await Factory.ExecuteDbAsync(async db =>
        {
            for (int i = 0; i < 5; i++)
            {
                db.GameRounds.Add((TestGameRound.WinningRound with
                {
                    Id = 0,
                    UserId = 1,
                    CreatedAt = TestClock.FixedNow.AddMinutes(-i)
                }).ToEntity());
            }
            await db.SaveChangesAsync();
        });

        var response = await Client.GetAsync("/api/game-history?page=2&pageSize=2");
        var body = await response.Content.ReadFromJsonAsync<OnlineCasino.Common.Models.PagedResult<OnlineCasino.DTOs.History.GameRoundDto>>();

        body!.Items.Should().HaveCount(2);
        body.Total.Should().Be(5);
        body.Page.Should().Be(2);
    }

    [Fact]
    public async Task Get_DoesNotReturnOtherUsersRounds()
    {
        SetUser(1);
        await Factory.ExecuteDbAsync(async db =>
        {
            db.Users.Add((TestUser.Valid with { Id = 2, Email = "u2@t.com", NickName = "u2" }).ToEntity());
            db.GameRounds.Add((TestGameRound.WinningRound with { Id = 0, UserId = 1 }).ToEntity());
            db.GameRounds.Add((TestGameRound.WinningRound with { Id = 0, UserId = 2 }).ToEntity());
            await db.SaveChangesAsync();
        });

        var response = await Client.GetAsync("/api/game-history");
        var body = await response.Content.ReadFromJsonAsync<OnlineCasino.Common.Models.PagedResult<OnlineCasino.DTOs.History.GameRoundDto>>();

        body!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetRound_Returns404_WhenUserIsNotOwner_EvenIfRoundExists()
    {
        SetUser(1);

        var otherRound = TestGameRound.WinningRound with
        {
            Id = 0,
            UserId = 1,
            PublicId = Guid.NewGuid().ToString("N")
        };

        await Factory.ExecuteDbAsync(async db =>
        {
            db.GameRounds.Add(otherRound.ToEntity());
            await db.SaveChangesAsync();
        });

        SqliteWebApplicationFactory.SetUser(2);

        var response = await Client.GetAsync($"/api/game-history/{otherRound.PublicId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_NoAuthCookie_Returns401()
    {
        using var factory = new SqliteWebApplicationFactory(withAuth: false);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/game-history");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetRound_NoAuthCookie_Returns401()
    {
        using var factory = new SqliteWebApplicationFactory(withAuth: false);
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/game-history/someid");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_ResponseIsCamelCaseJson()
    {
        SetUser(1);

        var response = await Client.GetAsync("/api/game-history");
        var body = await response.Content.ReadAsStringAsync();

        body.Should().Contain("\"items\"");
        body.Should().Contain("\"pageSize\"");
        body.Should().Contain("\"total\"");
        body.Should().NotContain("\"Items\"");
        body.Should().NotContain("\"PageSize\"");
    }
}
