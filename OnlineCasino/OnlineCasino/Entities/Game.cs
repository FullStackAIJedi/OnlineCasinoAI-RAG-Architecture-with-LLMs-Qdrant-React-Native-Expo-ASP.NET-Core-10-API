namespace OnlineCasino.Entities
{
    public class Game
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;   // "slots", "roulette", "blackjack"
        public string Name { get; set; } = null!;
        public int GameTypeId { get; set; }
        public GameType GameType { get; set; } = null!;

        public bool Enabled { get; set; } = true;
        public decimal MinBet { get; set; }
        public decimal MaxBet { get; set; }
        public int ReelsCount { get; set; }

        public ICollection<GameRound> Rounds { get; set; } = new List<GameRound>();
    }
}
