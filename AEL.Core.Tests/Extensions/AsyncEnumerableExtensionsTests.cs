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

	[Fact]
	public async Task ForEachParallel_ExecutesInParallel()
	{
		// Arrange
		IAsyncEnumerable<int> items = Enumerable.Range(1, 10).ToAsyncEnumerable();
		int maxParallelism = 3;
		int concurrentCount = 0;
		int maxConcurrentCount = 0;
		object lockObj = new();

		// Act
		List<int> results = [];
		await foreach (int item in items.ForEachParallel(async (i, ct) =>
			{
				lock (lockObj)
				{
					concurrentCount++;
					maxConcurrentCount = Math.Max(maxConcurrentCount, concurrentCount);
				}

				await Task.Delay(10, ct);

				lock (lockObj)
				{
					concurrentCount--;
				}

				return i * 2;
			}, maxParallelism, TestContext.Current.CancellationToken))
		{
			results.Add(item);
		}

		// Assert
		Assert.Equal(10, results.Count);
		Assert.True(maxConcurrentCount <= maxParallelism, $"Max concurrent count {maxConcurrentCount} exceeded max parallelism {maxParallelism}");
		Assert.All(Enumerable.Range(1, 10), i => Assert.Contains(i * 2, results));
	}

	[Fact]
	public async Task ForEachParallel_CancellationToken_AbortsExecution()
	{
		// Arrange
		IAsyncEnumerable<int> items = Enumerable.Range(1, 100).ToAsyncEnumerable();
		using CancellationTokenSource cts = new();
		int executedCount = 0;

		// Act & Assert
		await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
		{
			await foreach (int _ in items.ForEachParallel(async (_, ct) =>
				{
					Interlocked.Increment(ref executedCount);
					await Task.Delay(100, ct);
					return 1;
				}, cancellationToken: cts.Token))
			{
				await cts.CancelAsync();
			}
		});

		Assert.True(executedCount < 100);
	}
}
