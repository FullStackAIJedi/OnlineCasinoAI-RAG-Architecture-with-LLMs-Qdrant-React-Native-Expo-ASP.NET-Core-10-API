namespace OnlineCasino.Tests.Unit.Models;

using Xunit;
using FluentAssertions;

using OnlineCasino.Models;

public sealed class ResultTests
{
    [Fact]
    public void Success_CarriesData_AndNoError()
    {
        var result = Result<int>.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(42);
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_DefaultStatusCode_Is500()
    {
        var result = Result<string>.Failure("oops");

        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Error!.StatusCode.Should().Be(500);
        result.Error.Message.Should().Be("oops");
    }

    [Theory]
    [InlineData(400, "bad")]
    [InlineData(401, "unauth")]
    [InlineData(403, "forbidden")]
    [InlineData(404, "missing")]
    [InlineData(409, "conflict")]
    public void Failure_CarriesProvidedStatusCode(int status, string msg)
    {
        var result = Result<object>.Failure(msg, status);

        result.Error!.StatusCode.Should().Be(status);
        result.Error.Message.Should().Be(msg);
    }

    [Fact]
    public void Success_WithNullData_StillSuccess()
    {
        var result = Result<string?>.Success(null);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeNull();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Success_WithReferenceType_CarriesInstance()
    {
        var obj = new { Id = 1, Name = "test" };
        var result = Result<object>.Success(obj);

        result.Data.Should().BeSameAs(obj);
    }

    [Fact]
    public void Failure_EmptyMessage_StillFails()
    {
        var result = Result<object>.Failure("", 400);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be("");
    }

    [Theory]
    [InlineData(422, "unprocessable")]
    [InlineData(500, "server error")]
    public void Failure_CarriesAdditionalStatusCodes(int status, string msg)
    {
        var result = Result<object>.Failure(msg, status);

        result.Error!.StatusCode.Should().Be(status);
        result.Error.Message.Should().Be(msg);
    }
}
