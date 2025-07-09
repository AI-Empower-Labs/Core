using System.Globalization;

// ReSharper disable once CheckNamespace
namespace System;

public static class DateTimeExtensions
{
	public const long TicksPerMs = TimeSpan.TicksPerSecond / 1000;
	public const long UnixEpoch = 621355968000000000L;

	/// <summary>
	///     The number of ticks per microsecond.
	/// </summary>
	public const int TicksPerMicrosecond = 10;

	/// <summary>
	///     The number of ticks per Nanosecond.
	/// </summary>
	public const int NanosecondsPerTick = 100;

	private static readonly DateTime s_unixEpochDateTime = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	private static DateTime? s_utcNow; // For unit testing purposes

	public static DateTime UtcNow
	{
		get => s_utcNow ?? DateTime.UtcNow;
		internal set => s_utcNow = value;
	}

	public static bool Between(this DateTime dateTime, DateTime from, DateTime to, bool inclusiveTo)
	{
		if (inclusiveTo)
		{
			return dateTime >= from && dateTime <= to;
		}

		return dateTime >= from && dateTime < to;
	}

	public static bool Between(this DateTimeOffset dateTime, DateTimeOffset from, DateTimeOffset to, bool inclusiveTo)
	{
		if (inclusiveTo)
		{
			return dateTime >= from && dateTime <= to;
		}

		return dateTime >= from && dateTime < to;
	}

	public static DateTimeOffset Truncate(this DateTimeOffset dateTime, TimeSpan timeSpan)
	{
		return timeSpan == TimeSpan.Zero
			? dateTime
			: dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
	}

	public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan)
	{
		return timeSpan == TimeSpan.Zero
			? dateTime
			: dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
	}

	private static readonly TimeSpan s_oneMillisecond = TimeSpan.FromMilliseconds(1);
	private static readonly TimeSpan s_oneSecond = TimeSpan.FromSeconds(1);
	private static readonly TimeSpan s_oneMinute = TimeSpan.FromMinutes(1);
	private static readonly TimeSpan s_oneHour = TimeSpan.FromHours(1);
	private static readonly TimeSpan s_oneDay = TimeSpan.FromDays(1);

	public static DateTimeOffset Floor(this DateTimeOffset dateTime, DateTimeResolutionExtensions.DateTimeResolution resolution)
	{
		return resolution switch
		{
			DateTimeResolutionExtensions.DateTimeResolution.Millisecond => dateTime.Truncate(s_oneMillisecond),
			DateTimeResolutionExtensions.DateTimeResolution.Second => dateTime.Truncate(s_oneSecond),
			DateTimeResolutionExtensions.DateTimeResolution.Minute => dateTime.Truncate(s_oneMinute),
			DateTimeResolutionExtensions.DateTimeResolution.Hour => dateTime.Truncate(s_oneHour),
			DateTimeResolutionExtensions.DateTimeResolution.Day => dateTime.Truncate(s_oneDay),
			DateTimeResolutionExtensions.DateTimeResolution.Month => new DateTimeOffset(dateTime.Year, dateTime.Month, 1, 0, 0, 0, dateTime.Offset),
			DateTimeResolutionExtensions.DateTimeResolution.Year => new DateTimeOffset(dateTime.Year, 1, 1, 0, 0, 0, dateTime.Offset),
			_ => throw new NotSupportedException(resolution.ToString())
		};
	}

	public static DateTime Floor(this DateTime dateTime, DateTimeResolutionExtensions.DateTimeResolution resolution)
	{
		return resolution switch
		{
			DateTimeResolutionExtensions.DateTimeResolution.Millisecond => dateTime.Truncate(s_oneMillisecond),
			DateTimeResolutionExtensions.DateTimeResolution.Second => dateTime.Truncate(s_oneSecond),
			DateTimeResolutionExtensions.DateTimeResolution.Minute => dateTime.Truncate(s_oneMinute),
			DateTimeResolutionExtensions.DateTimeResolution.Hour => dateTime.Truncate(s_oneHour),
			DateTimeResolutionExtensions.DateTimeResolution.Day => dateTime.Truncate(s_oneDay),
			_ => throw new NotSupportedException(resolution.ToString())
		};
	}

	public static DateTime FromUnixTime(this double unixTime)
	{
		return s_unixEpochDateTime + TimeSpan.FromSeconds(unixTime);
	}

	public static DateTime FromUnixTimeMs(this double msSince1970)
	{
		long ticks = (long)(UnixEpoch + msSince1970 * TicksPerMs);
		return new DateTime(ticks, DateTimeKind.Utc).ToUniversalTime();
	}

	public static DateTime Parse(string input)
	{
		return DateTime.Parse(input, CultureInfo.InvariantCulture,
			DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
	}

	public static long ToUnixTime(this DateTime dateTime)
	{
		long epoch = (dateTime.ToUniversalTime().Ticks - UnixEpoch) / TimeSpan.TicksPerSecond;
		return epoch;
	}

	public static long ToUnixTimeMs(this DateTime dateTime)
	{
		long epoch = (dateTime.Ticks - UnixEpoch) / TicksPerMs;
		return epoch;
	}

	public static bool TryParseInvariantUniversal(string input, out DateTime dt)
	{
		return DateTime.TryParse(input, CultureInfo.InvariantCulture,
			DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out dt);
	}

	public static int SecondFragment(this DateTime self)
	{
		int fraction = (int)(self.Ticks % 10000000);
		return fraction;
	}

	public static bool HasExpired(this DateTime? expiresAt,
		DateTime? now = null)
	{
		return HasExpired(expiresAt, out TimeSpan? _, now);
	}

	public static bool HasExpired(this DateTimeOffset? expiresAt,
		DateTimeOffset? now = null)
	{
		return HasExpired(expiresAt, out TimeSpan? _, now);
	}

	public static bool HasExpired(this DateTime? expiresAt,
		out TimeSpan? timeToLive,
		DateTime? now = null)
	{
		if (expiresAt.HasValue)
		{
			timeToLive = expiresAt.Value.Subtract(now.GetValueOrDefault(DateTime.UtcNow));
			return timeToLive <= TimeSpan.Zero;
		}

		timeToLive = null;
		return false;
	}

	public static bool HasExpired(this DateTimeOffset? expiresAt,
		out TimeSpan? timeToLive,
		DateTimeOffset? now = null)
	{
		if (expiresAt.HasValue)
		{
			timeToLive = expiresAt.Value.Subtract(now.GetValueOrDefault(DateTime.UtcNow));
			return timeToLive <= TimeSpan.Zero;
		}

		timeToLive = null;
		return false;
	}

	public static bool HasExpired(this DateTime expiresAt,
		DateTime? now = null)
	{
		return HasExpired(expiresAt, out TimeSpan? _, now);
	}

	public static bool HasExpired(this DateTime expiresAt,
		out TimeSpan? timeToLive,
		DateTime? now = null)
	{
		timeToLive = expiresAt.Subtract(now.GetValueOrDefault(DateTime.UtcNow));
		return timeToLive <= TimeSpan.Zero;
	}

	public static bool HasExpired(this DateTimeOffset expiresAt,
		out TimeSpan? timeToLive,
		DateTimeOffset? now = null)
	{
		timeToLive = expiresAt.Subtract(now.GetValueOrDefault(DateTime.UtcNow));
		return timeToLive <= TimeSpan.Zero;
	}
}
