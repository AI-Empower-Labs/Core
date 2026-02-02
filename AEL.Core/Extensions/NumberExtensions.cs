using System.Numerics;

namespace System;

public static class NumberExtensions
{
	public static bool Between<T>(this T value, T from, T to, bool inclusiveTo = true) where T : INumber<T>
	{
		if (inclusiveTo)
		{
			return value >= from && value <= to;
		}

		return value >= from && value < to;
	}
}
