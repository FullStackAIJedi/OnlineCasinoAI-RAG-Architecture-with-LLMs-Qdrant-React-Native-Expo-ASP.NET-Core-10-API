namespace OnlineCasino.DTOs.Users
{
    public class UserListItemDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string NickName { get; set; } = null!;
        public string? PofileImage { get; set; }  
        public string Role { get; set; } = null!;
        public bool IsBlocked { get; set; }
        public decimal Balance { get; set; }
    }
}
