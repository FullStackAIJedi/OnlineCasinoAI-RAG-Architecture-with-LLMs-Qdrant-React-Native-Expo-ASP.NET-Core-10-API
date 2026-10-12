namespace OnlineCasino.Tests.Common.TestData;

public static class TestClock
{
    public static readonly DateTime FixedNow = new(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);
    public static readonly DateTimeOffset FixedNowOffset = new(FixedNow, TimeSpan.Zero);

    public static TimeProvider Provider => new FixedTimeProvider(FixedNowOffset);

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
