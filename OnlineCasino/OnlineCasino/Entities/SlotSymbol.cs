namespace OnlineCasino.Entities
{
    public class SlotSymbol
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;   // "cherry", "lemon", "seven", ...
        public string Name { get; set; } = null!;
        public decimal Payout3 { get; set; }        // множител при 3 еднакви
        public decimal Weight { get; set; }         // тегло за RNG
    }
}
