namespace OnlineCasino.Entities
{
    public class TransactionType
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;  // "Deposit", "Withdraw", "Bet", "Win"
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
