namespace OnlineCasino.DTOs.Auth
{
    public class AuthUserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string NickName { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
