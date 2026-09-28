namespace OnlineCasino.Entities
{
    public class GameType
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;   // "Slots", "Roulette", "Blackjack"
        public ICollection<Game> Games { get; set; } = new List<Game>();
    }
}
