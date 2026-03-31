using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace System.Threading.Tasks;

public static class TaskExtensions
{
	private static readonly Action<Task> s_ignoreTaskContinuation = t => _ = t.Exception;

	/// <param name="task">The task to wrap.</param>
	extension(Task task)
	{
		/// <summary>
		/// Wraps a task with one that will complete as cancelled based on a cancellation token,
		/// allowing someone to await a task but be able to break out early by cancelling the token.
		/// </summary>
		/// <param name="cancellationToken">The token that can be canceled to break out of the await.</param>
		/// <returns>The wrapping task.</returns>
		public Task WithCancellation(CancellationToken cancellationToken)
		{
			return task.WaitAsync(cancellationToken);
		}

		/// <summary>
		/// Wraps a task with one that will complete as cancelled based on a cancellation token,
		/// allowing someone to await a task but be able to break out early by cancelling the token.
		/// </summary>
		/// <param name="cancellationToken">The token that can be canceled to break out of the await.</param>
		/// <returns>The wrapping task.</returns>
		public async Task WithSilentCancellation(CancellationToken cancellationToken = default)
		{
			try
			{
				await task.WaitAsync(cancellationToken);
			}
			catch (OperationCanceledException)
			{
				// Ignore
			}
		}

		/// <summary>
		/// Awaits the specified task and suppresses any exceptions that occur, optionally supporting cancellation.
		/// </summary>
		/// <param name="cancellationToken">The token that can be canceled to break out of the await.</param>
		/// <returns>The wrapping task.</returns>
		public async Task WithSilentException(CancellationToken cancellationToken = default)
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

		public async Task WithExceptionProtection(ILogger logger,
			string message,
			CancellationToken cancellationToken = default)
		{
			try
			{
				await task.WaitAsync(cancellationToken);
			}
			catch (OperationCanceledException)
			{
				// Ignore
			}
			catch (Exception exception)
			{
				// ReSharper disable once TemplateIsNotCompileTimeConstantProblem
				logger.LogError(exception, message);
			}
		}

		public async Task WithTimeout(TimeSpan timeout,
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
	}

	/// <param name="task">The task to wrap.</param>
	/// <typeparam name="T">The type of value returned by the task.</typeparam>
	extension<T>(Task<T> task)
	{
		/// <summary>
		/// Awaits the specified task and suppresses any exceptions that occur, optionally supporting cancellation.
		/// </summary>
		/// <param name="defaultValue"></param>
		/// <param name="cancellationToken">The token that can be canceled to break out of the await.</param>
		/// <returns>The wrapping task.</returns>
		public async Task<T> WithSilentException(T defaultValue, CancellationToken cancellationToken = default)
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

		public async Task<T?> WithSilentCancellation(CancellationToken cancellationToken = default)
		{
			try
			{
				return await task.WaitAsync(cancellationToken);
			}
			catch (OperationCanceledException)
			{
				// Ignore
			}

			return default;
		}

		/// <summary>
		/// Wraps a task with one that will complete as canceled based on a cancellation token,
		/// allowing someone to await a task but be able to break out early by cancelling the token.
		/// </summary>
		/// <param name="cancellationToken">The token that can be canceled to break out of the await.</param>
		/// <returns>The wrapping task.</returns>
		public Task<T> WithCancellation(CancellationToken cancellationToken)
		{
			return task.WaitAsync(cancellationToken);
		}

		public async Task<T> WithTimeout(TimeSpan timeout,
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
	}

	extension<T>(Task<T> task) where T : class
	{
		public async Task<T?> WithExceptionProtection(ILogger logger,
			string message,
			CancellationToken cancellationToken = default)
		{
			try
			{
				return await task.WaitAsync(cancellationToken);
			}
			catch (OperationCanceledException)
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

	extension<T>(T result)
	{
		public Task<T> ToTask()
		{
			return Task.FromResult(result);
		}
	}
}
