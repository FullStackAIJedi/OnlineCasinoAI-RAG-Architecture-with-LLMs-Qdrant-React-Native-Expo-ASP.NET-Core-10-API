namespace OnlineCasino.Models
{
    /// <summary>
    /// Represents the outcome of an operation that can either succeed with data
    /// or fail with an error message and status code.
    /// <para>
    /// Used throughout the services layer to avoid throwing exceptions for
    /// expected failures (e.g. validation errors, not-found resources) and to
    /// carry a proper HTTP status code to the controller.
    /// </para>
    /// </summary>
    /// <typeparam name="T">
    /// The type of the payload returned on success (e.g. <c>SpinResponse</c>,
    /// <c>GameDto</c>, <c>List&lt;CurrentBetDto&gt;</c>, or a tuple).
    /// </typeparam>
    public class Result<T>
    {
        /// <summary>
        /// The payload of a successful operation.
        /// <c>null</c> when <see cref="IsSuccess"/> is <c>false</c>.
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Indicates whether the operation completed successfully.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Details about the failure.
        /// <c>null</c> when <see cref="IsSuccess"/> is <c>true</c>.
        /// </summary>
        public ErrorResponse? Error { get; set; }

        /// <summary>
        /// Creates a successful result carrying the given payload.
        /// </summary>
        /// <param name="data">The payload to return to the caller.</param>
        /// <returns>A <see cref="Result{T}"/> with <see cref="IsSuccess"/> set to <c>true</c>.</returns>
        public static Result<T> Success(T data) =>
            new() { IsSuccess = true, Data = data };

        /// <summary>
        /// Creates a failed result with an error message and HTTP status code.
        /// </summary>
        /// <param name="message">A human-readable error message.</param>
        /// <param name="statusCode">
        /// The HTTP status code that should be returned to the client.
        /// Defaults to <c>500</c> (Internal Server Error) when not specified;
        /// callers typically pass a more specific code such as <c>400</c>, <c>401</c>, <c>403</c>, <c>404</c>, or <c>409</c>.
        /// </param>
        /// <returns>A <see cref="Result{T}"/> with <see cref="IsSuccess"/> set to <c>false</c> and <see cref="Error"/> populated.</returns>
        public static Result<T> Failure(string message, int statusCode = 500) =>
            new()
            {
                IsSuccess = false,
                Error = new ErrorResponse(statusCode, message)
            };
    }
}
