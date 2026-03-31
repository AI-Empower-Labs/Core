namespace System;

public static class TimeSpanExtensions
{
	extension(TimeSpan value)
	{
		public bool Between(TimeSpan from, TimeSpan to, bool inclusiveTo = true)
		{
			if (inclusiveTo)
			{
				return value >= from && value <= to;
			}

			return value >= from && value < to;
		}
	}
}
