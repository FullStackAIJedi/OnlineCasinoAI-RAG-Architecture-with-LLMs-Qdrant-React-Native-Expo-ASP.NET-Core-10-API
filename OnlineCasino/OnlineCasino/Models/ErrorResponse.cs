namespace OnlineCasino.Models
{
    /// <summary>
    /// Represents an error returned by the API.
    /// <para>
    /// Used inside <see cref="Result{T}"/> to carry both a human-readable message
    /// and the HTTP status code that should be returned to the client.
    /// Serialized directly into the response body on failure.
    /// </para>
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// The HTTP status code associated with the error
        /// (e.g. <c>400</c>, <c>401</c>, <c>403</c>, <c>404</c>, <c>409</c>, <c>500</c>).
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// A human-readable error message describing what went wrong.
        /// Defaults to <c>"Something went wrong."</c> when not explicitly set.
        /// </summary>
        public string Message { get; set; } = "Something went wrong.";

        /// <summary>
        /// Initializes a new empty instance of the <see cref="ErrorResponse"/> class.
        /// <para>
        /// Required for JSON deserialization and for object initializers.
        /// </para>
        /// </summary>
        public ErrorResponse() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorResponse"/> class
        /// with the specified status code and message.
        /// </summary>
        /// <param name="statusCode">The HTTP status code associated with the error.</param>
        /// <param name="message">A human-readable error message.</param>
        public ErrorResponse(int statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }
    }
}
