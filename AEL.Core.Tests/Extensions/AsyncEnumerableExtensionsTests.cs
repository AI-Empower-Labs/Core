namespace AEL.Core.Tests.Extensions;

public class AsyncEnumerableExtensionsTests
{
	[Fact]
	public async Task Batch_SplitsIntoBatches()
	{
		List<ICollection<int>> batches = [];
		await foreach (ICollection<int> batch in Enumerable.Range(0, 5).ToAsyncEnumerable().Batch(2, cancellationToken: TestContext.Current.CancellationToken))
		{
			batches.Add(batch.ToArray());
		}

		Assert.Equal(3, batches.Count);
		Assert.Equal([0, 1], batches[0]);
		Assert.Equal([2, 3], batches[1]);
		Assert.Equal([4], batches[2]);
	}

	[Fact]
	public async Task ForeachParallel_PreservesOrderOfSourceByIndex()
	{
		IEnumerable<int> source = Enumerable.Range(0, 10).ToArray();

		List<int> results = [];
		await foreach (int r in source.ForeachParallel(Work, maxDegreeOfParallelism: 3, cancellationToken: TestContext.Current.CancellationToken))
		{
			results.Add(r);
		}

		Assert.Equal(source.Select(i => i * i).ToArray(), results);
		return;

		async Task<int> Work(int i, CancellationToken ct)
		{
			// reverse sleep so later items finish earlier
			await Task.Delay(5 * (10 - i), ct);
			return i * i;
		}
	}
}
