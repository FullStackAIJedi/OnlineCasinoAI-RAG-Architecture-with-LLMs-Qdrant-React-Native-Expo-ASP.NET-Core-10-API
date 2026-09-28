namespace OnlineCasino.DTOs.Slots
{
    public class SpinRequest
    {
        public decimal BetAmount { get; set; }
        public string ClientRoundId { get; set; } = null!;
    }
}
