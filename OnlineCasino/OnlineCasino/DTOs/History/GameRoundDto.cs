namespace OnlineCasino.DTOs.History
{
    public class GameRoundDto
    {
        public string RoundId { get; set; } = null!;
        public string Game { get; set; } = null!;
        public decimal BetAmount { get; set; }
        public decimal WinAmount { get; set; }
        public string? Result { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
