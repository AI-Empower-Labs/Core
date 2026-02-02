namespace AEL.Core.Extensions;

public static class CancellationTokenExtensions
{
	public static async Task AwaitCancellation(this CancellationToken cancellationToken)
	{
		await Task.Delay(Timeout.Infinite, cancellationToken).WithSilentCancellation(cancellationToken);
	}
}
