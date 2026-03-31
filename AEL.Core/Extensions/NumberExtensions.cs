using System.Numerics;

namespace System;

public static class NumberExtensions
{
	extension<T>(T value) where T : INumber<T>
	{
		public bool Between(T from, T to, bool inclusiveTo = true)
		{
			if (inclusiveTo)
			{
				return value >= from && value <= to;
			}

			return value >= from && value < to;
		}
	}
}
