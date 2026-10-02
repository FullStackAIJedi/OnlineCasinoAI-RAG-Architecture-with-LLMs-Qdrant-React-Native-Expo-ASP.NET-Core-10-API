namespace OnlineCasino.DTOs.Users
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string NickName { get; set; } = null!;
        public string? PofileImage { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Role { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
