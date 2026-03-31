namespace AEL.Core.Tests.Extensions;

public class EnumerableExtensionsTests
{
	[Fact]
	public void WhereNotNull_FiltersNulls()
	{
		IEnumerable<string?> items = ["a", null, "b"];
		IEnumerable<string> result = items.WhereNotNull();
		Assert.Equal(new[] { "a", "b" }, result.ToArray());
	}

	[Fact]
	public async Task WhereNotNull_Async_FiltersNulls()
	{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		async IAsyncEnumerable<string?> GetItems()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
		{
			yield return "x";
			yield return null;
			yield return "y";
		}

		IAsyncEnumerable<string> filtered = GetItems().WhereNotNull();
		List<string> list = [];
		await foreach (string s in filtered)
		{
			list.Add(s);
		}

		Assert.Equal(new[] { "x", "y" }, list);
	}

	[Fact]
	public void JoinOperator_BasicJoin()
	{
		IEnumerable<string> items = ["a", "b", "c"];
		string result = items * ",";
		Assert.Equal("a,b,c", result);
	}

	[Fact]
	public void JoinOperator_EmptySequence_ReturnsEmptyString()
	{
		IEnumerable<string> items = [];
		string result = items * ",";
		Assert.Equal(string.Empty, result);
	}

	[Fact]
	public void JoinOperator_SingleElement_NoSeparatorInserted()
	{
		IEnumerable<string> items = ["only"];
		string result = items * "|";
		Assert.Equal("only", result);
	}

	[Fact]
	public void JoinOperator_MultiCharacterSeparator()
	{
		IEnumerable<string> items = ["x", "y"];
		string result = items * " - ";
		Assert.Equal("x - y", result);
	}

	[Fact]
	public async Task ForEachParallel_ExecutesInParallel()
	{
		// Arrange
		IEnumerable<int> items = Enumerable.Range(1, 10);
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
		Assert.All(items, i => Assert.Contains(i * 2, results));
	}

	[Fact]
	public async Task ForEachParallel_CancellationToken_AbortsExecution()
	{
		// Arrange
		IEnumerable<int> items = Enumerable.Range(1, 100);
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
