namespace OnlineCasino.Tests.Common.TestData;
using OnlineCasino.Entities;

public sealed record TestUser
{
    public int? Id { get; init; }
    public string Email { get; init; } = "test@test.com";
    public string NickName { get; init; } = "tester";
    public string Password { get; init; } = "Password123!";
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? ProfileImage { get; init; }
    public int RoleId { get; init; } = TestIds.UserRoleId;
    public string RoleName { get; init; } = "User";
    public bool IsBlocked { get; init; }
    public TestWallet Wallet { get; init; } = TestWallet.Standard;
    public DateTime CreatedAt { get; init; } = TestClock.FixedNow;

    public static TestUser Valid => new();

    public static TestUser Admin => new()
    {
        RoleId = TestIds.AdminRoleId,
        RoleName = "Admin",
        Email = "admin@test.com",
        NickName = "admin"
    };

    public static TestUser BlockedAccount => new() { IsBlocked = true };
    public static TestUser EmptyWallet => new() { Wallet = TestWallet.EmptyBalance };
    public static TestUser HighBalanceWallet => new() { Wallet = TestWallet.HighBalance };

    public static TestUser WithBalance(decimal balance) =>
        new() { Wallet = TestWallet.Standard with { Balance = balance } };

    public User ToEntity()
    {
        var effectiveId = Id ?? 0;

        return new User
        {
            Id = effectiveId,
            Email = Email,
            NickName = NickName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password),
            PofileImage = ProfileImage,
            FirstName = FirstName,
            LastName = LastName,
            IsBlocked = IsBlocked,
            CreatedAt = CreatedAt,
            RoleId = RoleId,
            Wallet = Wallet.ToEntity(effectiveId)
        };
    }
}
