namespace OnlineCasino.Entities
{
    public class BlackjackSession
    {
        public int Id { get; set; }
        public string PublicId { get; set; } = Guid.NewGuid().ToString("N");

        public int UserId { get; set; }
        public int GameId { get; set; }
        public decimal BetAmount { get; set; }

        public string PlayerHand { get; set; } = "";  // "A♠,7♥"
        public string DealerHand { get; set; } = "";
        public int PlayerScore { get; set; }
        public int DealerScore { get; set; }

        public int StatusId { get; set; }
        public BlackjackStatus Status { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? FinishedAt { get; set; }
    }
}
