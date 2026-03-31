// ReSharper disable once CheckNamespace

using Microsoft.Extensions.Logging;

namespace System.Threading.Tasks;

public static class ValueTaskExtensions
{
	/// <param name="task">The task to wrap.</param>
	extension(ValueTask task)
	{
		public async ValueTask WithExceptionProtection(ILogger logger,
			string message,
			CancellationToken cancellationToken = default)
		{
			await task
				.AsTask()
				.WithExceptionProtection(logger, message, cancellationToken);
		}
		/// <summary>
		/// Awaits the specified task and suppresses any exceptions that occur, optionally supporting cancellation.
		/// </summary>
		/// <param name="cancellationToken">The token that can be canceled to break out of the await.</param>
		/// <returns>The wrapping task.</returns>
		public async ValueTask WithSilentException(CancellationToken cancellationToken = default)
		{
			await task.AsTask().WithSilentException(cancellationToken);
		}
	}

	/// <param name="task">The task to wrap.</param>
	extension<T>(ValueTask<T> task)
	{
		public async ValueTask<T?> WithSilentCancellation(CancellationToken cancellationToken = default)
		{
			try
			{
				return await task.AsTask().WaitAsync(cancellationToken);
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
			{
				// Ignore
			}

			return default;
		}

		/// <summary>
		/// Awaits the specified task and suppresses any exceptions that occur, optionally supporting cancellation.
		/// </summary>
		/// <param name="defaultValue"></param>
		/// <param name="cancellationToken">The token that can be canceled to break out of the await.</param>
		/// <returns>The wrapping task.</returns>
		public async ValueTask<T> WithSilentException(T defaultValue, CancellationToken cancellationToken = default)
		{
			return await task
				.AsTask()
				.WithSilentException(defaultValue, cancellationToken);
		}
	}
}
