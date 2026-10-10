namespace OnlineCasino.Middlewares
{
    using System.Text.Json;

    using OnlineCasino.Models;

    /// <summary>
    /// Global exception-handling middleware.
    /// <para>
    /// Wraps the entire request pipeline in a try/catch block.
    /// If any downstream middleware, controller, or service throws an unhandled
    /// exception, this middleware:
    /// <list type="number">
    ///   <item>Logs the exception via <see cref="ILogger{TCategoryName}"/>.</item>
    ///   <item>Sets the HTTP status code to <c>500 Internal Server Error</c>.</item>
    ///   <item>Writes a JSON-serialized <see cref="ErrorResponse"/> to the response body.</item>
    /// </list>
    /// </para>
    /// <para>
    /// The JSON output uses camelCase property naming to match the rest of the API.
    /// </para>
    /// <para>
    /// Register this middleware early in the pipeline (before routing, authentication,
    /// and endpoints) so it can catch exceptions from all downstream components.
    /// </para>
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware delegate in the request pipeline.</param>
        /// <param name="logger">The logger used to record unhandled exceptions.</param>
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Executes the middleware: invokes the next delegate and catches any unhandled exception.
        /// <para>
        /// On success, the response produced by downstream middleware is passed through unchanged.
        /// On failure, the exception is logged and a <c>500</c> JSON error response is written to the client.
        /// </para>
        /// </summary>
        /// <param name="ctx">The current <see cref="HttpContext"/> for the request.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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
