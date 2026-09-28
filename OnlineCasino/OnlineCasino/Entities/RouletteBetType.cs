namespace OnlineCasino.Entities
{
    public class RouletteBetType
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;   // "red", "black", "number", "dozen", ...
        public string Name { get; set; } = null!;
    }
}
