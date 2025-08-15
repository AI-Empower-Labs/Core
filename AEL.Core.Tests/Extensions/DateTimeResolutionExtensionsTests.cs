namespace AEL.Core.Tests.Extensions;

public class DateTimeResolutionExtensionsTests
{
	[Theory]
	[InlineData(DateTimeResolutionExtensions.DateTimeResolution.Millisecond)]
	[InlineData(DateTimeResolutionExtensions.DateTimeResolution.Second)]
	[InlineData(DateTimeResolutionExtensions.DateTimeResolution.Minute)]
	[InlineData(DateTimeResolutionExtensions.DateTimeResolution.Hour)]
	[InlineData(DateTimeResolutionExtensions.DateTimeResolution.Day)]
	[InlineData(DateTimeResolutionExtensions.DateTimeResolution.Month)]
	[InlineData(DateTimeResolutionExtensions.DateTimeResolution.Year)]
	public void ToTimeSpan_MapsCorrectly(DateTimeResolutionExtensions.DateTimeResolution res)
	{
		TimeSpan expected = res switch
		{
			DateTimeResolutionExtensions.DateTimeResolution.Millisecond => TimeSpan.FromMilliseconds(1),
			DateTimeResolutionExtensions.DateTimeResolution.Second => TimeSpan.FromSeconds(1),
			DateTimeResolutionExtensions.DateTimeResolution.Minute => TimeSpan.FromMinutes(1),
			DateTimeResolutionExtensions.DateTimeResolution.Hour => TimeSpan.FromHours(1),
			DateTimeResolutionExtensions.DateTimeResolution.Day => TimeSpan.FromDays(1),
			DateTimeResolutionExtensions.DateTimeResolution.Month => TimeSpan.FromDays(30),
			DateTimeResolutionExtensions.DateTimeResolution.Year => TimeSpan.FromDays(365),
			_ => throw new ArgumentOutOfRangeException()
		};
		Assert.Equal(expected, res.ToTimeSpan());
	}
}
