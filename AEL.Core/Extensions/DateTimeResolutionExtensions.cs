// ReSharper disable once CheckNamespace
namespace System;

public static class DateTimeResolutionExtensions
{
	public static TimeSpan ToTimeSpan(this DateTimeResolution dateTimeResolution)
	{
		return dateTimeResolution switch
		{
			DateTimeResolution.Millisecond => TimeSpan.FromMilliseconds(1),
			DateTimeResolution.Second => TimeSpan.FromSeconds(1),
			DateTimeResolution.Minute => TimeSpan.FromMinutes(1),
			DateTimeResolution.Hour => TimeSpan.FromHours(1),
			DateTimeResolution.Day => TimeSpan.FromDays(1),
			DateTimeResolution.Month => TimeSpan.FromDays(30),
			DateTimeResolution.Year => TimeSpan.FromDays(365),
			_ => throw new ArgumentOutOfRangeException(nameof(dateTimeResolution), dateTimeResolution, null)
		};
	}

	/// <summary>
	///     Represents the resolution with which to floor or ceil. This could be to ceil to the nearest hour, day, month or
	///     year
	/// </summary>
	[Flags]
	public enum DateTimeResolution
	{
		Millisecond = 0,
		Second = 1,
		Minute = 2,
		Hour = 4,
		Day = 8,
		Month = 16,
		Year = 32
	}
}
