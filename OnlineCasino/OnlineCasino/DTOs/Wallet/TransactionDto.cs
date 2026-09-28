namespace OnlineCasino.DTOs.Wallet
{
    public class TransactionDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = null!;
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
