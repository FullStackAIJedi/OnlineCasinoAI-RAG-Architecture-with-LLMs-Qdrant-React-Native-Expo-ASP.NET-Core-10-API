namespace OnlineCasino.DTOs.Auth
{
    public class SignUpRequest
    {
        public string Email { get; set; } = null!;
        public string NickName { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
