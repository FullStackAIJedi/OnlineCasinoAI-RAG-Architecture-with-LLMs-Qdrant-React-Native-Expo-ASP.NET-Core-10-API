namespace OnlineCasino.Tests.Unit.Services;

using Xunit;
using FluentAssertions;

using OnlineCasino.Services;
using OnlineCasino.Tests.Common.Fixtures;

public sealed class GameServiceTests
{
    [Fact]
    public async Task GetAll_OnlyEnabledGames()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        var svc = new GameService(db);

        var result = await svc.GetAllAsync();

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(4);
        result.Data.Should().NotContain(g => g.Code == "disabled");
    }

    [Fact]
    public async Task GetByCode_Existing_ReturnsDto()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        var svc = new GameService(db);

        var result = await svc.GetByCodeAsync("slots");

        result.IsSuccess.Should().BeTrue();
        result.Data!.Code.Should().Be("slots");
        result.Data.Type.Should().Be("Slots");
    }

    [Fact]
    public async Task GetByCode_DisabledGame_StillReturns()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        var svc = new GameService(db);

        var result = await svc.GetByCodeAsync("disabled");

        result.IsSuccess.Should().BeTrue();
        result.Data!.Enabled.Should().BeFalse();
    }

    [Fact]
    public async Task GetByCode_Missing_Returns404()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        var svc = new GameService(db);

        var result = await svc.GetByCodeAsync("unknown");

        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetByCode_Roulette_ReturnsRouletteType()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        var svc = new GameService(db);

        var result = await svc.GetByCodeAsync("roulette");

        result.Data!.Type.Should().Be("Roulette");
    }

    [Fact]
    public async Task GetByCode_Blackjack_ReturnsBlackjackType()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        var svc = new GameService(db);

        var result = await svc.GetByCodeAsync("blackjack");

        result.Data!.Type.Should().Be("Blackjack");
    }

    [Fact]
    public async Task GetAll_ReturnsAllExpectedCodes()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        var svc = new GameService(db);

        var result = await svc.GetAllAsync();

        result.Data.Select(g => g.Code).Should().BeEquivalentTo(
            new[] { "slots", "slots4", "roulette", "blackjack" });
    }

    [Fact]
    public async Task GetAll_ReturnsExpectedMinMax()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        var svc = new GameService(db);

        var result = await svc.GetAllAsync();
        var slots = result.Data.Single(g => g.Code == "slots");

        slots.MinBet.Should().Be(1m);
        slots.MaxBet.Should().Be(100m);
    }

    [Fact]
    public async Task GetByCode_CaseSensitive_Returns404()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        var svc = new GameService(db);

        var result = await svc.GetByCodeAsync("SLOTS");

        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetByCode_EmptyString_Returns404()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        var svc = new GameService(db);

        var result = await svc.GetByCodeAsync("");

        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetAll_ExcludesDisabledGames()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        var svc = new GameService(db);

        var result = await svc.GetAllAsync();

        result.Data.Select(g => g.Code).Should().NotContain("disabled");
    }

    [Fact]
    public async Task GetByCode_IncludesDisabledGames()
    {
        var db = await TestDbFactory.CreateSeededAsync();
        var svc = new GameService(db);

        var result = await svc.GetByCodeAsync("disabled");

        result.IsSuccess.Should().BeTrue();
        result.Data!.Enabled.Should().BeFalse();
    }
}