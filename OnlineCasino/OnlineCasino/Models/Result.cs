namespace OnlineCasino.Models
{
    public class Result<T>
    {
        public T? Data { get; set; }
        public bool IsSuccess { get; set; }
        public ErrorResponse? Error { get; set; }

        public static Result<T> Success(T data) =>
            new() { IsSuccess = true, Data = data };

        public static Result<T> Failure(string message, int statusCode = 500) =>
            new()
            {
                IsSuccess = false,
                Error = new ErrorResponse(statusCode, message)
            };
    }
}
