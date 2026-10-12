namespace OnlineCasino.Tests.Unit.Middlewares;

using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
 
using Moq;
using Xunit;
using FluentAssertions;

using OnlineCasino.Middlewares;
using OnlineCasino.Models;

public sealed class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task Invoke_NoException_PassesThrough()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        var logger = Mock.Of<ILogger<ExceptionHandlingMiddleware>>();
        var nextCalled = false;

        var mw = new ExceptionHandlingMiddleware(
            _ => { nextCalled = true; return Task.CompletedTask; }, logger);

        await mw.InvokeAsync(ctx);

        nextCalled.Should().BeTrue();
        ctx.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Invoke_Exception_Returns500JsonErrorResponse()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        var logger = Mock.Of<ILogger<ExceptionHandlingMiddleware>>();

        var mw = new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException("boom"), logger);

        await mw.InvokeAsync(ctx);

        ctx.Response.StatusCode.Should().Be(500);
        ctx.Response.ContentType.Should().Be("application/json");

        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        var err = JsonSerializer.Deserialize<ErrorResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        err!.StatusCode.Should().Be(500);
        err.Message.Should().Contain("Something went wrong");
    }

    [Fact]
    public async Task Invoke_UsesCamelCaseJsonPolicy()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        var logger = Mock.Of<ILogger<ExceptionHandlingMiddleware>>();

        var mw = new ExceptionHandlingMiddleware(
            _ => throw new Exception("x"), logger);

        await mw.InvokeAsync(ctx);

        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(ctx.Response.Body).ReadToEndAsync();

        json.Should().Contain("\"statusCode\"");
        json.Should().Contain("\"message\"");
    }

    [Fact]
    public async Task Invoke_LogsException()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();

        var loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        var mw = new ExceptionHandlingMiddleware(
            _ => throw new Exception("boom"), loggerMock.Object);

        await mw.InvokeAsync(ctx);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Invoke_DifferentExceptionTypes_AllReturn500()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        var logger = Mock.Of<ILogger<ExceptionHandlingMiddleware>>();

        var mw = new ExceptionHandlingMiddleware(
            _ => throw new ArgumentException("arg"), logger);

        await mw.InvokeAsync(ctx);

        ctx.Response.StatusCode.Should().Be(500);
    }
}