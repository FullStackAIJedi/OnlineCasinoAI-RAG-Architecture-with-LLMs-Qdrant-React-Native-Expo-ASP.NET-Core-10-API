namespace OnlineCasino.Entities
{
    public class BlackjackStatus
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;   // "PlayerTurn", "Finished", "Bust"
        public string Name { get; set; } = null!;
    }
}
