namespace OnlineCasino.DTOs.Slots
{
    public class Spin4Request
    { 
        public decimal BetAmount { get; set; } 
        public string ClientRoundId { get; set; } = null!;
    }
     
    public class Spin4Response
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
