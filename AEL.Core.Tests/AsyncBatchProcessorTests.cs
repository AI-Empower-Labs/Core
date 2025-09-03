using System.Threading.Channels;

using Microsoft.Extensions.Logging;

namespace AEL.Core.Tests;

public sealed class AsyncBatchProcessorTests
{
	private sealed class TestAsyncBatchProcessor<TIn, TOut> : AsyncBatchProcessor<TIn, TOut>
	{
		private readonly Func<IEnumerable<TIn>, CancellationToken, Task<TOut[]>> _func;

		public TestAsyncBatchProcessor(
			int batchSize,
			Func<IEnumerable<TIn>, CancellationToken, Task<TOut[]>> func,
			ILogger logger,
			int? capacity,
			BoundedChannelFullMode fullMode)
			: base(batchSize, logger, capacity, fullMode)
		{
			_func = func ?? throw new ArgumentNullException(nameof(func));
		}

		protected override ValueTask<TOut[]> ExecuteBatchProcess(ICollection<TIn> inputs, CancellationToken cancellationToken)
			=> new(_func(inputs, cancellationToken));

		public new Task<TOut> Process(TIn value, CancellationToken cancellationToken = default)
			=> base.Process(value, cancellationToken);
	}

	private sealed class TestLogger : ILogger
	{
		public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
		public bool IsEnabled(LogLevel logLevel) => true;
		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
			Func<TState, Exception?, string> formatter)
		{
			// No-op in tests
		}
	}

	[Fact]
	public async Task Processes_Items_In_Batches_And_Preserves_Order()
	{
		List<int[]> seenBatches = [];
		ILogger logger = new TestLogger();

		Task<int[]> Func(IEnumerable<int> ins, CancellationToken ct)
		{
			int[] arr = ins.ToArray();
			seenBatches.Add(arr);
			return Task.FromResult(arr.Select(i => i * 2).ToArray());
		}

		await using TestAsyncBatchProcessor<int, int> proc = new(
			batchSize: 3,
			func: Func,
			logger: logger,
			capacity: null,
			fullMode: BoundedChannelFullMode.Wait);

		await proc.StartAsync(TestContext.Current.CancellationToken);

		int[] inputs = Enumerable.Range(1, 7).ToArray();
		Task<int>[] tasks = inputs
			.Select(x => proc.Process(x, TestContext.Current.CancellationToken))
			.ToArray();

		int[] results = await Task.WhenAll(tasks);

		// Validate results map 1:1 in the same order
		Assert.Equal(inputs.Select(x => x * 2).ToArray(), results);

		int[] outputs = seenBatches.SelectMany(b => b).ToArray();
		Assert.Equal(inputs.ToArray(), outputs);

		await proc.StopAsync(TestContext.Current.CancellationToken);
	}

	[Fact]
	public async Task Individual_Process_Cancellation_Is_Respected_And_Filtered_From_Batch()
	{
		List<int[]> seenBatches = [];
		ILogger logger = new TestLogger();

		Task<int[]> Func(IEnumerable<int> ins, CancellationToken ct)
		{
			int[] arr = ins.ToArray();
			seenBatches.Add(arr);
			return Task.FromResult(arr.Select(x => x).ToArray());
		}

		await using TestAsyncBatchProcessor<int, int> proc = new(
			batchSize: 10,
			func: Func,
			logger: logger,
			capacity: null,
			fullMode: BoundedChannelFullMode.Wait);

		// Queue a task with a token we'll cancel before starting the service
		CancellationTokenSource cts = new();
		Task<int> canceled = proc.Process(42, cts.Token);
		await cts.CancelAsync();

		// Add two more that should be processed
		Task<int> ok1 = proc.Process(1, TestContext.Current.CancellationToken);
		Task<int> ok2 = proc.Process(2, TestContext.Current.CancellationToken);

		// Now start the service so it will drain the channel, observing the canceled item
		await proc.StartAsync(TestContext.Current.CancellationToken);

		// Then complete so the service drains and exits cleanly
		proc.Complete();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await canceled);
		int[] ok = await Task.WhenAll(ok1, ok2);
		Assert.Equal(new[] { 1, 2 }, ok);

		// Ensure the canceled item was filtered out from the function inputs
		Assert.Contains(seenBatches, b => b.SequenceEqual([1, 2]));
		await proc.StopAsync(TestContext.Current.CancellationToken);
	}

	[Fact]
	public async Task Output_Count_Mismatch_Faults_All_Pending_Tasks()
	{
		ILogger logger = new TestLogger();

		Task<int[]> Func(IEnumerable<int> ins, CancellationToken ct)
		{
			// Purposefully return wrong length
			return Task.FromResult(Array.Empty<int>());
		}

		await using TestAsyncBatchProcessor<int, int> proc = new(
			batchSize: 5,
			func: Func,
			logger: logger,
			capacity: null,
			fullMode: BoundedChannelFullMode.Wait);

		await proc.StartAsync(TestContext.Current.CancellationToken);

		Task<int> t1 = proc.Process(10, TestContext.Current.CancellationToken);
		Task<int> t2 = proc.Process(11, TestContext.Current.CancellationToken);

		proc.Complete();

		Exception ex1 = await Assert.ThrowsAsync<InvalidOperationException>(async () => await t1);
		Exception ex2 = await Assert.ThrowsAsync<InvalidOperationException>(async () => await t2);
		Assert.Contains("Batch output count mismatch", ex1.Message);
		Assert.Contains("Batch output count mismatch", ex2.Message);

		await proc.StopAsync(TestContext.Current.CancellationToken);
	}

	[Fact]
	public async Task StopAsync_Cancels_InFlight_Batch_With_Stopping_Token()
	{
		ILogger logger = new TestLogger();

		Task<int[]> Func(IEnumerable<int> ins, CancellationToken ct)
		{
			// Emulate long-running work that will be canceled by StopAsync
			return Task.Run(async () =>
			{
				await Task.Delay(Timeout.Infinite, ct);
				return ins.ToArray();
			});
		}

		await using TestAsyncBatchProcessor<int, int> proc = new(
			batchSize: 2,
			func: Func,
			logger: logger,
			capacity: null,
			fullMode: BoundedChannelFullMode.Wait);

		await proc.StartAsync(TestContext.Current.CancellationToken);

		Task<int> t1 = proc.Process(100, TestContext.Current.CancellationToken);
		Task<int> t2 = proc.Process(200, TestContext.Current.CancellationToken);

		// Give it a moment to start executing the batch
		await Task.Delay(50, TestContext.Current.CancellationToken);

		await proc.StopAsync(TestContext.Current.CancellationToken);

		await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await t1);
		await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await t2);
	}
}
