namespace AEL.Core.Tests.Extensions;

public sealed class AsyncEnumerableExtensionsTests
{
	[Fact]
	public async Task Batch_SplitsIntoBatches()
	{
		List<ICollection<int>> batches = [];
		await foreach (ICollection<int> batch in Enumerable.Range(0, 5)
			.ToAsyncEnumerable()
			.Batch(2, cancellationToken: TestContext.Current.CancellationToken))
		{
			batches.Add(batch.ToArray());
		}

		Assert.Equal(3, batches.Count);
	}
}
