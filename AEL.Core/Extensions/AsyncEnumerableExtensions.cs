using System.Threading.Channels;

using AEL.Core.Extensions;
// ReSharper disable once CheckNamespace
using System.Runtime.CompilerServices;

namespace System.Linq;

public static class AsyncEnumerableExtensions
{
	extension<T>(IAsyncEnumerable<T?> enumerable) where T : notnull
	{
		public async IAsyncEnumerable<T> WhereNotNull()
		{
			await foreach (T? t in enumerable)
			{
				if (t is not null)
				{
					yield return t;
				}
			}
		}
	}

	extension<T>(IAsyncEnumerable<T> enumerable)
	{
		public async IAsyncEnumerable<ICollection<T>> Batch(int batchSize,
			[EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			Channel<T> channel = Channel.CreateBounded<T>(batchSize);
			Task producerTask = Producer();
			await foreach (ICollection<T> collection in channel.ReadAllBatch(batchSize, cancellationToken))
			{
				yield return collection;
			}

			await producerTask;

			async Task Producer()
			{
				try
				{
					await foreach (T t in enumerable.WithCancellation(cancellationToken))
					{
						await channel.Writer.WriteAsync(t, cancellationToken);
					}
				}
				catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
				{
					// Ignore
				}
				catch (Exception ex)
				{
					channel.Writer.TryComplete(ex);
				}
				finally
				{
					channel.Writer.TryComplete();
				}
			}
		}

		public async IAsyncEnumerable<ICollection<T>> BatchWithDrain(int batchSize,
			[EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			Channel<T> channel = Channel.CreateBounded<T>(batchSize);
			Task producerTask = Producer();
			await foreach (ICollection<T> collection in channel.ReadAllBatchDrain(batchSize, cancellationToken))
			{
				yield return collection;
			}

			await producerTask;

			async Task Producer()
			{
				try
				{
					await foreach (T t in enumerable.WithCancellation(cancellationToken))
					{
						await channel.Writer.WriteAsync(t, cancellationToken);
					}
				}
				catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
				{
					// Ignore
				}
				catch (Exception ex)
				{
					channel.Writer.TryComplete(ex);
				}
				finally
				{
					channel.Writer.TryComplete();
				}
			}
		}
	}
}
