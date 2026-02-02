namespace System;

public static class TimeSpanExtensions
{
	public static bool Between(this TimeSpan value, TimeSpan from, TimeSpan to, bool inclusiveTo = true)
	{
		if (inclusiveTo)
		{
			return value >= from && value <= to;
		}

		return value >= from && value < to;
	}
}
