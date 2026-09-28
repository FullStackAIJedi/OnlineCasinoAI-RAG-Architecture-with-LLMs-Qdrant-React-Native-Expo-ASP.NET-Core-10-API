namespace OnlineCasino.DTOs.Blackjack
{
    public class BlackjackStateDto
    {
        public string SessionId { get; set; } = null!;
        public List<string> PlayerHand { get; set; } = new();
        public List<string> DealerHand { get; set; } = new();
        public int PlayerScore { get; set; }
        public int DealerScore { get; set; }
        public string Status { get; set; } = null!;
        public decimal BetAmount { get; set; }
        public decimal WinAmount { get; set; }
        public string? Result { get; set; }
        public decimal Balance { get; set; }
    }
}
