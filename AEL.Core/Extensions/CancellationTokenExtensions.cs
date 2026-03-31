namespace System.Threading;

public static class CancellationTokenExtensions
{
	extension(CancellationToken cancellationToken)
	{
		public async Task AwaitCancellation()
		{
			await Task.Delay(Timeout.Infinite, cancellationToken).WithSilentCancellation(cancellationToken);
		}
	}
}
