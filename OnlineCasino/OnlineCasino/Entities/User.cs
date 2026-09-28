namespace OnlineCasino.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string NickName { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public Wallet Wallet { get; set; } = null!;
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<GameRound> GameRounds { get; set; } = new List<GameRound>();
    }
}
