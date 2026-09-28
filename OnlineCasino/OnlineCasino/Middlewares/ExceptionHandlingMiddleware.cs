namespace OnlineCasino.Middlewares
{
    using System.Text.Json;

    using OnlineCasino.Models;
 
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            try
            {
                await _next(ctx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");

                ctx.Response.StatusCode = 500;
                ctx.Response.ContentType = "application/json";

                var error = new ErrorResponse(500, "Something went wrong. Please try again.");
                var json = JsonSerializer.Serialize(error, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await ctx.Response.WriteAsync(json);
            }
        }
    }
}
