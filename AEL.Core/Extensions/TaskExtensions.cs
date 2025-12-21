using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace System.Threading.Tasks;

public static class TaskExtensions
{
	private static readonly Action<Task> s_ignoreTaskContinuation = t => _ = t.Exception;

	/// <summary>
	/// Wraps a task with one that will complete as canceled based on a cancellation token,
	/// allowing someone to await a task but be able to break out early by cancelling the token.
	/// </summary>
	/// <typeparam name="T">The type of value returned by the task.</typeparam>
	/// <param name="task">The task to wrap.</param>
	/// <param name="cancellationToken">The token that can be canceled to break out of the await.</param>
	/// <returns>The wrapping task.</returns>
	public static Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
	{
		return task.WaitAsync(cancellationToken);
	}

	/// <summary>
	/// Wraps a task with one that will complete as cancelled based on a cancellation token,
	/// allowing someone to await a task but be able to break out early by cancelling the token.
	/// </summary>
	/// <param name="task">The task to wrap.</param>
	/// <param name="cancellationToken">The token that can be canceled to break out of the await.</param>
	/// <returns>The wrapping task.</returns>
	public static Task WithCancellation(this Task task, CancellationToken cancellationToken)
	{
		return task.WaitAsync(cancellationToken);
	}

	/// <summary>
	/// Wraps a task with one that will complete as cancelled based on a cancellation token,
	/// allowing someone to await a task but be able to break out early by cancelling the token.
	/// </summary>
	/// <param name="task">The task to wrap.</param>
	/// <param name="cancellationToken">The token that can be canceled to break out of the await.</param>
	/// <returns>The wrapping task.</returns>
	public static async Task WithSilentCancellation(this Task task, CancellationToken cancellationToken = default)
	{
		try
		{
			await task.WaitAsync(cancellationToken);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			// Ignore
		}
	}

	/// <summary>
	/// Awaits the specified task and suppresses any exceptions that occur, optionally supporting cancellation.
	/// </summary>
	/// <param name="task">The task to wrap.</param>
	/// <param name="cancellationToken">The token that can be canceled to break out of the await.</param>
	/// <returns>The wrapping task.</returns>
	public static async Task WithSilentException(this Task task, CancellationToken cancellationToken = default)
	{
		try
		{
			await task.WaitAsync(cancellationToken);
		}
		catch
		{
			// Ignore
		}
	}

	/// <summary>
	/// Awaits the specified task and suppresses any exceptions that occur, optionally supporting cancellation.
	/// </summary>
	/// <param name="task">The task to wrap.</param>
	/// <param name="defaultValue"></param>
	/// <param name="cancellationToken">The token that can be canceled to break out of the await.</param>
	/// <returns>The wrapping task.</returns>
	public static async Task<T> WithSilentException<T>(this Task<T> task, T defaultValue, CancellationToken cancellationToken = default)
	{
		try
		{
			return await task.WaitAsync(cancellationToken);
		}
		catch
		{
			return defaultValue;
		}
	}

	public static async Task<T?> WithSilentCancellation<T>(this Task<T> task, CancellationToken cancellationToken = default)
	{
		try
		{
			return await task.WaitAsync(cancellationToken);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			// Ignore
		}

		return default;
	}

	public static async Task WithExceptionProtection(this Task task,
		ILogger logger,
		string message,
		CancellationToken cancellationToken = default)
	{
		try
		{
			await task.WaitAsync(cancellationToken);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			// Ignore
		}
		catch (Exception exception)
		{
			// ReSharper disable once TemplateIsNotCompileTimeConstantProblem
			logger.LogError(exception, message);
		}
	}

	public static async Task<T?> WithExceptionProtection<T>(this Task<T> task,
		ILogger logger,
		string message,
		CancellationToken cancellationToken = default) where T : class
	{
		try
		{
			return await task.WaitAsync(cancellationToken);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			// Ignore
		}
		catch (Exception exception)
		{
			// ReSharper disable once TemplateIsNotCompileTimeConstantProblem
			logger.LogError(exception, message);
		}

		return null;
	}

	public static async ValueTask WithExceptionProtection(this ValueTask task,
		ILogger logger,
		string message,
		CancellationToken cancellationToken = default)
	{
		await task
			.AsTask()
			.WithExceptionProtection(logger, message, cancellationToken);
	}

	public static async Task<T> WithTimeout<T>(this Task<T> task,
		TimeSpan timeout,
		CancellationToken cancellationToken = default)
	{
		try
		{
			return await task.WaitAsync(timeout, cancellationToken);
		}
		catch (TimeoutException)
		{
			throw new OperationCanceledException();
		}
	}

	public static async Task WithTimeout(this Task task,
		TimeSpan timeout,
		bool continueOnCapturedContext = true,
		CancellationToken cancellationToken = default)
	{
		try
		{
			await task.WaitAsync(timeout, cancellationToken).ConfigureAwait(continueOnCapturedContext);
		}
		catch (TimeoutException)
		{
			throw new OperationCanceledException();
		}
	}

#pragma warning disable CA1030 // Use events where appropriate
	public static void FireAndForget(this Task task)
#pragma warning restore CA1030 // Use events where appropriate
	{
		// Fire and forget
		if (task.IsCompleted)
		{
			_ = task.Exception;
		}
		else
		{
			_ = task.ContinueWith(
				s_ignoreTaskContinuation,
				CancellationToken.None,
				TaskContinuationOptions.NotOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously,
				TaskScheduler.Default);
		}
	}

	public static Task<T> ToTask<T>(this T result)
	{
		return Task.FromResult(result);
	}
}
