namespace OnlineCasino.Models
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = "Something went wrong.";

        public ErrorResponse() { }
        public ErrorResponse(int statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }
    }
}
