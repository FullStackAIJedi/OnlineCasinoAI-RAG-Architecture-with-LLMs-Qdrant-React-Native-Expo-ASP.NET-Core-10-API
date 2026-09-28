namespace OnlineCasino.DTOs.Roulette
{
    public class PlaceBetRequest
    {
        public string Type { get; set; } = null!;   // "red", "number", ...
        public int? Value { get; set; }             // за number/dozen
        public decimal Amount { get; set; }
    }
}
