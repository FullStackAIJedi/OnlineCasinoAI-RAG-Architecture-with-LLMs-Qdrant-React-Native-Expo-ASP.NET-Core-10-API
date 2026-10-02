namespace OnlineCasino.DTOs.Users
{
    public class UpdateProfileRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PofileImage { get; set; } = null!;
    }
}
