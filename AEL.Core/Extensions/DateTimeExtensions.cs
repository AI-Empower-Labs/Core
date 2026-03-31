using System.Globalization;

// ReSharper disable once CheckNamespace
namespace System;

public static class DateTimeExtensions
{
	extension(DateTime dateTime)
	{
		public bool Between(DateTime from, DateTime to, bool inclusiveTo)
		{
			if (inclusiveTo)
			{
				return dateTime >= from && dateTime <= to;
			}

			return dateTime >= from && dateTime < to;
		}

		public DateTime Truncate(TimeSpan timeSpan) =>
			timeSpan == TimeSpan.Zero
				? dateTime
				: dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));

		public DateTime Floor(DateTimeResolutionExtensions.DateTimeResolution resolution)
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

		public int SecondFragment()
		{
			int fraction = (int)(dateTime.Ticks % 10000000);
			return fraction;
		}
	}

	extension(DateTimeOffset dateTime)
	{
		public bool Between(DateTimeOffset from, DateTimeOffset to, bool inclusiveTo)
		{
			if (inclusiveTo)
			{
				return dateTime >= from && dateTime <= to;
			}

			return dateTime >= from && dateTime < to;
		}

		public DateTimeOffset Truncate(TimeSpan timeSpan)
		{
			return timeSpan == TimeSpan.Zero
				? dateTime
				: dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
		}

		public DateTimeOffset Floor(DateTimeResolutionExtensions.DateTimeResolution resolution)
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
	}

	private static readonly TimeSpan s_oneMillisecond = TimeSpan.FromMilliseconds(1);
	private static readonly TimeSpan s_oneSecond = TimeSpan.FromSeconds(1);
	private static readonly TimeSpan s_oneMinute = TimeSpan.FromMinutes(1);
	private static readonly TimeSpan s_oneHour = TimeSpan.FromHours(1);
	private static readonly TimeSpan s_oneDay = TimeSpan.FromDays(1);

	public static DateTime Parse(string input)
	{
		return DateTime.Parse(input, CultureInfo.InvariantCulture,
			DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
	}

	public static bool TryParseInvariantUniversal(string input, out DateTime dt)
	{
		return DateTime.TryParse(input, CultureInfo.InvariantCulture,
			DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out dt);
	}
}
