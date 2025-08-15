namespace AEL.Core.Tests.Extensions;

public class DateTimeExtensionsTests
{
    [Theory]
    [InlineData("2023-01-02", "2023-01-01", "2023-01-03", true, true)]
    [InlineData("2023-01-03", "2023-01-01", "2023-01-03", false, false)]
    public void Between_WorksAsExpected(string date, string from, string to, bool inclusiveTo, bool expected)
    {
        DateTime dt = DateTime.Parse(date);
        DateTime f = DateTime.Parse(from);
        DateTime t = DateTime.Parse(to);
        Assert.Equal(expected, dt.Between(f, t, inclusiveTo));
    }

    [Fact]
    public void Truncate_RemovesSubSecondPrecision()
    {
        DateTimeOffset dt = new(2023, 1, 1, 10, 20, 30, 500, TimeSpan.Zero);
        DateTimeOffset truncated = dt.Truncate(TimeSpan.FromSeconds(1));
        Assert.Equal(new DateTimeOffset(2023, 1, 1, 10, 20, 30, 0, TimeSpan.Zero), truncated);
    }

    [Theory]
    [InlineData(DateTimeResolutionExtensions.DateTimeResolution.Second)]
    [InlineData(DateTimeResolutionExtensions.DateTimeResolution.Minute)]
    public void Floor_FloorsToResolution(DateTimeResolutionExtensions.DateTimeResolution res)
    {
        DateTimeOffset dt = new(2023, 1, 1, 12, 34, 56, 789, TimeSpan.Zero);
        DateTimeOffset floored = dt.Floor(res);
        Assert.True(floored <= dt);
        switch (res)
        {
            case DateTimeResolutionExtensions.DateTimeResolution.Second:
                Assert.Equal(dt.Truncate(TimeSpan.FromSeconds(1)), floored);
                break;
            case DateTimeResolutionExtensions.DateTimeResolution.Minute:
                Assert.Equal(dt.Truncate(TimeSpan.FromMinutes(1)), floored);
                break;
        }
    }

    [Fact]
    public void FromUnixTime_And_ToUnixTime_AreInverse()
    {
        double seconds = 1_650_000_000;
        DateTime dt = seconds.FromUnixTime();
        Assert.Equal(seconds, dt.ToUnixTime());
        Assert.Equal(DateTimeKind.Utc, dt.Kind);
    }

    [Fact]
    public void FromUnixTimeMs_And_ToUnixTimeMs_AreInverse()
    {
        double ms = 1_650_000_000_000;
        DateTime dt = ms.FromUnixTimeMs();
        Assert.Equal(ms, dt.ToUnixTimeMs());
        Assert.Equal(DateTimeKind.Utc, dt.Kind);
    }

    [Fact]
    public void Parse_And_TryParseInvariantUniversal_RespectUtc()
    {
        string input = "2023-01-02T03:04:05Z";
        DateTime dt1 = DateTimeExtensions.Parse(input);
        Assert.Equal(DateTimeKind.Utc, dt1.Kind);
        Assert.True(DateTimeExtensions.TryParseInvariantUniversal(input, out DateTime dt2));
        Assert.Equal(dt1, dt2);
        Assert.False(DateTimeExtensions.TryParseInvariantUniversal("not a date", out _));
    }

    [Fact]
    public void SecondFragment_ReturnsTicksWithinSecond()
    {
        DateTime dt = new DateTime(2000, 1, 1, 0, 0, 1, DateTimeKind.Utc).AddTicks(123);
        Assert.Equal(123, dt.SecondFragment());
    }

    [Fact]
    public void HasExpired_WorksForDateTime()
    {
        DateTime past = DateTime.UtcNow.AddSeconds(-5);
        DateTime future = DateTime.UtcNow.AddSeconds(5);
        Assert.True(past.HasExpired());
        Assert.False(future.HasExpired());
        DateTime? nullable = null;
        Assert.False(nullable.HasExpired(out TimeSpan? ttl));
        Assert.Null(ttl);
    }

    [Fact]
    public void HasExpired_WorksForDateTimeOffset()
    {
        DateTimeOffset? past = DateTimeOffset.UtcNow.AddMinutes(-1);
        DateTimeOffset? future = DateTimeOffset.UtcNow.AddMinutes(1);
        Assert.True(past.HasExpired());
        Assert.False(future.HasExpired());
        DateTimeOffset? nullable = null;
        Assert.False(nullable.HasExpired(out TimeSpan? ttl2));
        Assert.Null(ttl2);
    }
}