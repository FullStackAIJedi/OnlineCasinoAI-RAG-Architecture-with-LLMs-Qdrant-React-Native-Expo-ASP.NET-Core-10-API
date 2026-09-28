namespace OnlineCasino.DTOs.Games
{
    public class GameDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Type { get; set; } = null!;
        public bool Enabled { get; set; }
        public decimal MinBet { get; set; }
        public decimal MaxBet { get; set; }
    }
}
