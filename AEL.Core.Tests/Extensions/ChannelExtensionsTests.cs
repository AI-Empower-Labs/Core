using System.Threading.Channels;

using AEL.Core.Extensions;

namespace AEL.Core.Tests.Extensions;

public class ChannelExtensionsTests
{
	[Fact]
	public async Task ReadAllDrain_YieldsAllItems_EvenAfterComplete()
	{
		Channel<int> ch = Channel.CreateUnbounded<int>();
		await ch.Writer.WriteAsync(1, TestContext.Current.CancellationToken);
		await ch.Writer.WriteAsync(2, TestContext.Current.CancellationToken);
		ch.Writer.TryComplete();

		List<int> results = [];
		await foreach (int i in ch.ReadAllDrain(cancellationToken: TestContext.Current.CancellationToken))
		{
			results.Add(i);
		}

		Assert.Equal(new[] { 1, 2 }, results);
	}

	[Fact]
	public async Task ReadAllDrainBatch_RespectsMaxBatchSize_AndDrains()
	{
		Channel<int> ch = Channel.CreateUnbounded<int>();
		for (int i = 0; i < 7; i++) await ch.Writer.WriteAsync(i, TestContext.Current.CancellationToken);
		ch.Writer.TryComplete();

		List<ICollection<int>> batches = [];
		await foreach (ICollection<int> b in ch.ReadAllDrainBatch(3, cancellationToken: TestContext.Current.CancellationToken))
		{
			batches.Add(b.ToArray());
		}

		Assert.Equal(3, batches.Count);
		Assert.Equal([0, 1, 2], batches[0]);
		Assert.Equal([3, 4, 5], batches[1]);
		Assert.Equal([6], batches[2]);
	}

	[Fact]
	public async Task ReadAllBatchAggressive_EmitsWhileDataAvailable()
	{
		Channel<int> ch = Channel.CreateUnbounded<int>();
		CancellationTokenSource cts = new();

		Task producer = Task.Run(async () =>
		{
			for (int i = 0; i < 5; i++)
			{
				await ch.Writer.WriteAsync(i, cts.Token);
				await Task.Delay(10, cts.Token);
			}

			ch.Writer.TryComplete();
		}, cts.Token);

		List<int> results = [];
		await foreach (ICollection<int> batch in ch.ReadAllBatchAggressive(2, cts.Token))
		{
			results.AddRange(batch);
		}

		await producer;
		Assert.Equal(new[] { 0, 1, 2, 3, 4 }, results);
	}
}
