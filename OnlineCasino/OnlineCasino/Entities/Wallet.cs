namespace OnlineCasino.Entities
{
    public class Wallet
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public decimal Balance { get; set; }
        public string Currency { get; set; } = "EUR";
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
