namespace OnlineCasino.DTOs.Roulette
{
    public class RouletteSpinResponse
    {
        public string RoundId { get; set; } = null!;
        public int WinningNumber { get; set; }
        public string WinningColor { get; set; } = null!;
        public decimal TotalBet { get; set; }
        public decimal TotalWin { get; set; }
        public decimal Balance { get; set; }
        public string Result { get; set; } = null!;
    }
}
