namespace OnlineCasino.DTOs.Roulette
{
    public class CurrentBetDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = null!;
        public int? Value { get; set; }
        public decimal Amount { get; set; }
    }
}
