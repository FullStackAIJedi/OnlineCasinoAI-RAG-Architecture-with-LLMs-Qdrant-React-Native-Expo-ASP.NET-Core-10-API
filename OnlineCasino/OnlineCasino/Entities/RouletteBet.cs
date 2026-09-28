namespace OnlineCasino.Entities
{
    public class RouletteBet
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RouletteBetTypeId { get; set; }
        public RouletteBetType BetType { get; set; } = null!;

        public int? NumberValue { get; set; }   // за number bet
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
