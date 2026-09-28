namespace OnlineCasino.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int TransactionTypeId { get; set; }
        public TransactionType TransactionType { get; set; } = null!;

        public decimal Amount { get; set; }          // + credit, - debit
        public decimal BalanceAfter { get; set; }
        public string? ReferenceId { get; set; }     // roundId / etc.
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
