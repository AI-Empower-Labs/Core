namespace AEL.Core.Tests.Extensions;

public class ValueTaskExtensionsTests
{
	[Fact]
	public async Task WithSilentCancellation_ValueTask_SwallowsCancellation()
	{
		CancellationTokenSource cts = new(TimeSpan.FromMilliseconds(10));
		Task<int> vt = Task.Run(async () =>
		{
			await Task.Delay(100, cts.Token);
			return 1;
		});
		int? res = await vt.WithSilentCancellation(cts.Token);
		Assert.NotEqual(1, res);
	}

	[Fact]
	public async Task WithSilentException_ValueTask_SwallowsException()
	{
		ValueTask task = new(Task.FromException(new InvalidOperationException()));
		await task.WithSilentException();

		ValueTask<int> vt = new(Task.FromException<int>(new InvalidOperationException()));
		int r = await vt.WithSilentException(5);
		Assert.Equal(5, r);
	}
}
