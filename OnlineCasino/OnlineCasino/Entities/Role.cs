namespace OnlineCasino.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;   // "User", "Admin"
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
