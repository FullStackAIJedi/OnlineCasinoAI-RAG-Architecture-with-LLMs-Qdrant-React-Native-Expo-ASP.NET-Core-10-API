namespace OnlineCasino.Tests.Unit.Models;

using Xunit;
using FluentAssertions;

using OnlineCasino.Common.Models;
 
public sealed class PagedResultTests
{
    [Fact]
    public void NewInstance_HasEmptyItems_AndZeroValues()
    {
        var p = new PagedResult<int>();
        p.Items.Should().NotBeNull().And.BeEmpty();
        p.Page.Should().Be(0);
        p.PageSize.Should().Be(0);
        p.Total.Should().Be(0);
    }

    [Fact]
    public void CanStoreValues()
    {
        var p = new PagedResult<string>
        {
            Items = new List<string> { "x", "y" },
            Page = 3,
            PageSize = 2,
            Total = 10
        };

        p.Items.Should().HaveCount(2);
        p.Page.Should().Be(3);
        p.PageSize.Should().Be(2);
        p.Total.Should().Be(10);
    }
}