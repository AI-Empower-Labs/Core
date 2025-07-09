using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

#pragma warning disable VSTHRD003

namespace AEL.Core;

public abstract partial class AsyncBackgroundService : AsyncDisposableBase, IHostedService
{
	private readonly CancellationTokenSource _stoppingCts = new();
	private Task? _executingTask;
	private readonly ILogger _logger;

	protected AsyncBackgroundService(ILogger logger)
	{
		_logger = logger;
		DisposableBag.Add(() => _stoppingCts.Cancel());
	}

	/// <summary>
	///     Triggered when the application host is ready to start the service.
	/// </summary>
	/// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
	public virtual Task StartAsync(CancellationToken cancellationToken)
	{
		LogStart(_logger);
		_executingTask = ExecuteAsync(_stoppingCts.Token)
			.WithExecuteWithProtection(_logger, "Background service execution exception", cancellationToken: cancellationToken);
		return Task.CompletedTask;
	}

	/// <summary>
	///     Triggered when the application host is performing a graceful shutdown.
	/// </summary>
	/// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
	public virtual async Task StopAsync(CancellationToken cancellationToken)
	{
		if (_executingTask is null)
		{
			return;
		}

		try
		{
			await _stoppingCts.CancelAsync();
		}
		finally
		{
			await ShutdownService(_executingTask, cancellationToken);
		}
	}

	private async Task ShutdownService(Task executingTask, CancellationToken cancellationToken)
	{
		try
		{
			LogStop(_logger);
			await executingTask
				.WithTimeout(TimeSpan.FromSeconds(10), cancellationToken: cancellationToken);
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			// Ignore
		}
		catch (Exception exception)
		{
			LogServiceException(_logger, exception);
		}
	}

	protected virtual Task ExecuteAsync(CancellationToken stoppingToken)
	{
		return Task.CompletedTask;
	}

	[LoggerMessage(LogLevel.Information, "Starting")]
	private static partial void LogStart(ILogger logger);

	[LoggerMessage(LogLevel.Information, "Stopping")]
	private static partial void LogStop(ILogger logger);

	[LoggerMessage(LogLevel.Error, "Execution exception")]
	private static partial void LogServiceException(ILogger logger, Exception exception);
}
