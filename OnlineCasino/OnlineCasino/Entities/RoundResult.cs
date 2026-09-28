namespace OnlineCasino.Entities
{
    public class RoundResult
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;   // "Win", "Lose", "Push"
        public ICollection<GameRound> Rounds { get; set; } = new List<GameRound>();
    }
}
