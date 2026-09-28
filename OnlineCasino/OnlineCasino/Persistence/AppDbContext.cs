namespace OnlineCasino.Data
{
    using Microsoft.EntityFrameworkCore;

    using OnlineCasino.Entities;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Wallet> Wallets => Set<Wallet>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<TransactionType> TransactionTypes => Set<TransactionType>();
        public DbSet<Game> Games => Set<Game>();
        public DbSet<GameType> GameTypes => Set<GameType>();
        public DbSet<GameRound> GameRounds => Set<GameRound>();
        public DbSet<RoundResult> RoundResults => Set<RoundResult>();
        public DbSet<RouletteBet> RouletteBets => Set<RouletteBet>();
        public DbSet<RouletteBetType> RouletteBetTypes => Set<RouletteBetType>();
        public DbSet<BlackjackSession> BlackjackSessions => Set<BlackjackSession>();
        public DbSet<BlackjackStatus> BlackjackStatuses => Set<BlackjackStatus>();
        public DbSet<SlotSymbol> SlotSymbols => Set<SlotSymbol>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<User>().HasIndex(u => u.Email).IsUnique();
            mb.Entity<User>().HasIndex(u => u.NickName).IsUnique();

            mb.Entity<User>()
                .HasOne(u => u.Role).WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId).OnDelete(DeleteBehavior.Restrict);

            mb.Entity<User>()
                .HasOne(u => u.Wallet).WithOne(w => w.User)
                .HasForeignKey<Wallet>(w => w.UserId).OnDelete(DeleteBehavior.Cascade);

            mb.Entity<Transaction>()
                .HasOne(t => t.TransactionType).WithMany(tt => tt.Transactions)
                .HasForeignKey(t => t.TransactionTypeId).OnDelete(DeleteBehavior.Restrict);

            mb.Entity<Game>()
                .HasOne(g => g.GameType).WithMany(gt => gt.Games)
                .HasForeignKey(g => g.GameTypeId).OnDelete(DeleteBehavior.Restrict);

            mb.Entity<GameRound>()
                .HasOne(r => r.Game).WithMany(g => g.Rounds)
                .HasForeignKey(r => r.GameId).OnDelete(DeleteBehavior.Restrict);

            mb.Entity<GameRound>()
                .HasOne(r => r.RoundResult).WithMany(rr => rr.Rounds)
                .HasForeignKey(r => r.RoundResultId).OnDelete(DeleteBehavior.Restrict);

            mb.Entity<GameRound>().HasIndex(r => r.PublicId).IsUnique();

            mb.Entity<BlackjackSession>().HasIndex(s => s.PublicId).IsUnique();

            mb.Entity<Transaction>().Property(t => t.Amount).HasPrecision(18, 2);
            mb.Entity<Transaction>().Property(t => t.BalanceAfter).HasPrecision(18, 2);
            mb.Entity<Wallet>().Property(w => w.Balance).HasPrecision(18, 2);
            mb.Entity<GameRound>().Property(r => r.BetAmount).HasPrecision(18, 2);
            mb.Entity<GameRound>().Property(r => r.WinAmount).HasPrecision(18, 2);
            mb.Entity<Game>().Property(g => g.MinBet).HasPrecision(18, 2);
            mb.Entity<Game>().Property(g => g.MaxBet).HasPrecision(18, 2);
            mb.Entity<BlackjackSession>().Property(s => s.BetAmount).HasPrecision(18, 2);
            mb.Entity<RouletteBet>().Property(b => b.Amount).HasPrecision(18, 2);
            mb.Entity<SlotSymbol>().Property(s => s.Payout3).HasPrecision(18, 2);
            mb.Entity<SlotSymbol>().Property(s => s.Weight).HasPrecision(18, 2);
        }
    }
}
