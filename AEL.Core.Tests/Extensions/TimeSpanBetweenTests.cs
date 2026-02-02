namespace AEL.Core.Tests.Extensions;

public sealed class TimeSpanBetweenTests
{
	[Fact]
	public void BetweenWorksForTimeSpan()
	{
		TimeSpan value = TimeSpan.FromMilliseconds(1);
		TimeSpan from = TimeSpan.FromMilliseconds(1);
		TimeSpan to = TimeSpan.FromMilliseconds(10);
		bool inclusiveTo = true;
		Assert.True(value.Between(from, to, inclusiveTo));
	}
}
