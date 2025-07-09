// ReSharper disable once CheckNamespace

namespace System.Threading.Tasks;

public static class ValueTaskExtensions
{
	public static async ValueTask<T?> WithSilentCancellation<T>(this ValueTask<T> task, CancellationToken cancellationToken = default)
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
	/// <param name="task">The task to wrap.</param>
	/// <param name="cancellationToken">The token that can be canceled to break out of the await.</param>
	/// <returns>The wrapping task.</returns>
	public static async ValueTask WithSilentException(this ValueTask task, CancellationToken cancellationToken = default)
	{
		await task.AsTask().WithSilentException(cancellationToken);
	}

	/// <summary>
	/// Awaits the specified task and suppresses any exceptions that occur, optionally supporting cancellation.
	/// </summary>
	/// <param name="task">The task to wrap.</param>
	/// <param name="defaultValue"></param>
	/// <param name="cancellationToken">The token that can be canceled to break out of the await.</param>
	/// <returns>The wrapping task.</returns>
	public static async ValueTask<T> WithSilentException<T>(this ValueTask<T> task, T defaultValue, CancellationToken cancellationToken = default)
	{
		return await task
			.AsTask()
			.WithSilentException(defaultValue, cancellationToken);
	}
}
