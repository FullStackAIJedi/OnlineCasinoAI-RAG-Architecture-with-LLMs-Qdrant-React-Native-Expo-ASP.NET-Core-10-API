namespace OnlineCasino.Entities
{
    public class GameRound
    {
        public int Id { get; set; }
        public string PublicId { get; set; } = Guid.NewGuid().ToString("N");

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int GameId { get; set; }
        public Game Game { get; set; } = null!;

        public int? RoundResultId { get; set; }
        public RoundResult? RoundResult { get; set; }

        public decimal BetAmount { get; set; }
        public decimal WinAmount { get; set; }

        public string? DetailsJson { get; set; }   // game-specific (reels, cards, etc.)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
