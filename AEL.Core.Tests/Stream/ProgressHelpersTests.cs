using AEL.Core.Stream;

namespace AEL.Core.Tests.Stream;

public sealed class ProgressHelpersTests
{
	[Fact]
	public void CalculateProgress_CalculatesCorrectRates()
	{
		// 1000 bytes transferred in 1 second, total 2000 bytes
		(double percent, double bitsPerSec, double bytesPerSec) =
			ProgressHelpers.CalculateProgress(1000, 0, 2000, TimeSpan.FromSeconds(1));

		Assert.Equal(50.0, percent);
		Assert.Equal(1000.0, bytesPerSec);
		Assert.Equal(8000.0, bitsPerSec);
	}

	[Fact]
	public void CalculateProgress_ZeroTimeOrTotal_DoesNotThrowDivideByZero()
	{
		(double percent, double bitsPerSec, double bytesPerSec) =
			ProgressHelpers.CalculateProgress(100, 0, 0, TimeSpan.Zero);

		Assert.Equal(0.0, percent);
		Assert.Equal(0.0, bytesPerSec);
		Assert.Equal(0.0, bitsPerSec);
	}
}
