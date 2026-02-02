using AEL.Core.Extensions;

namespace AEL.Core.Tests.Extensions;

public sealed class NumberBetweenTests
{
	[Theory]
	[InlineData(5, 1, 10, true, true)]
	[InlineData(10, 1, 10, true, true)]
	[InlineData(10, 1, 10, false, false)]
	public void Between_Generic_WorksForIntegers(int value, int from, int to, bool inclusiveTo, bool expected)
	{
		Assert.Equal(expected, value.Between(from, to, inclusiveTo));
	}

	[Theory]
	[InlineData(1.5, 1.0, 2.0, true, true)]
	[InlineData(2.0, 1.0, 2.0, false, false)]
	public void Between_Generic_WorksForDoubles(double value, double from, double to, bool inclusiveTo, bool expected)
	{
		Assert.Equal(expected, value.Between(from, to, inclusiveTo));
	}
}
