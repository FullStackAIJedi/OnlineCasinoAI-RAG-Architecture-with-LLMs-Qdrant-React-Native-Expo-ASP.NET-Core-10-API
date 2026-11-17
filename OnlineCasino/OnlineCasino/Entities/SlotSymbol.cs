namespace OnlineCasino.Entities
{
    public class SlotSymbol
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;   // "cherry", "lemon", "seven", ...
        public string Name { get; set; } = null!;
        public decimal Payout3 { get; set; }       
        public decimal Payout4 { get; set; }
        public decimal Payout5 { get; set; }
        public decimal Weight { get; set; }        
    }
}
