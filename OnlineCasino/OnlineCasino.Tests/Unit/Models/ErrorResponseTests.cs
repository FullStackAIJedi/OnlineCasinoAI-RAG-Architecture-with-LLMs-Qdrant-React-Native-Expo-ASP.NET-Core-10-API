namespace OnlineCasino.Tests.Unit.Models;

using Xunit;
using FluentAssertions;

using OnlineCasino.Models;
 
public sealed class ErrorResponseTests
{
    [Fact]
    public void DefaultConstructor_HasDefaultMessage()
    {
        var e = new ErrorResponse();
        e.Message.Should().Be("Something went wrong.");
        e.StatusCode.Should().Be(0);
    }

    [Fact]
    public void ParameterizedConstructor_SetsBothFields()
    {
        var e = new ErrorResponse(404, "not found");
        e.StatusCode.Should().Be(404);
        e.Message.Should().Be("not found");
    }
}
