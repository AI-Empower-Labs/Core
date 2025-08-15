using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace AEL.Core.Extensions;

public static class ChannelExtensions
{
	public static async IAsyncEnumerable<T> ReadAllDrain<T>(
		this Channel<T> channel,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		while (await channel.Reader
			.WaitToReadAsync(cancellationToken)
			.WithSilentCancellation(cancellationToken))
		{
			while (channel.Reader.TryRead(out T? item))
			{
				yield return item;
			}
		}

		// Drain queue
		while (channel.Reader.TryRead(out T? item))
		{
			yield return item;
		}
	}

	public static async IAsyncEnumerable<ICollection<T>> ReadAllDrainBatch<T>(
		this Channel<T> channel,
		int maxBatchSize = 10,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		while (await channel.Reader
			.WaitToReadAsync(cancellationToken)
			.WithSilentCancellation(cancellationToken))
		{
			foreach (ICollection<T> batch in MakeBatches())
			{
				yield return batch;
			}
		}

		// Empty queue
		foreach (ICollection<T> batch in MakeBatches())
		{
			yield return batch;
		}

		yield break;

		IEnumerable<ICollection<T>> MakeBatches()
		{
			List<T> batch = new(maxBatchSize);
			while (channel.Reader.TryRead(out T? item))
			{
				batch.Add(item);
				if (batch.Count >= maxBatchSize)
				{
					yield return batch;
					batch.Clear();
				}
			}

			if (batch.Count > 0)
			{
				yield return batch;
			}
		}
	}

	public static async IAsyncEnumerable<ICollection<T>> ReadAllBatchAggressive<T>(
		this Channel<T> channel,
		int maxBatchSize = 10,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		List<T> batch = new(maxBatchSize);
		while (await channel.Reader
			.WaitToReadAsync(cancellationToken)
			.WithSilentCancellation(cancellationToken))
		{
			batch.Clear();
			while (channel.Reader.TryRead(out T? item))
			{
				batch.Add(item);
				if (batch.Count >= maxBatchSize)
				{
					yield return batch;
					batch.Clear();
				}
			}

			if (batch.Count > 0)
			{
				yield return batch;
			}
		}

		batch.Clear();
		while (channel.Reader.TryRead(out T? item))
		{
			batch.Add(item);
			if (batch.Count >= maxBatchSize)
			{
				yield return batch;
				batch.Clear();
			}
		}

		if (batch.Count > 0)
		{
			yield return batch;
		}
	}
}
