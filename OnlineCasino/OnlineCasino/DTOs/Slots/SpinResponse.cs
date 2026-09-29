namespace OnlineCasino.DTOs.Slots
{
    public class SpinResponse
    {
        public string RoundId { get; set; } = null!;
        public List<string> Reels { get; set; } = new();
        public decimal BetAmount { get; set; }
        public decimal WinAmount { get; set; }
        public decimal Balance { get; set; }
        public string Result { get; set; } = null!;
        public int MatchCount { get; set; }
    }
}
