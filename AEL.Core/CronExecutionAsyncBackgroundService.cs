using System.Diagnostics;

using Cronos;

using Microsoft.Extensions.Logging;

namespace AEL.Core;

public abstract class CronExecutionAsyncBackgroundService(
	CronExpression cronExpression,
	ILogger logger,
	bool executeImmediately = false) : AsyncBackgroundService(logger)
{
	private readonly ILogger _logger = logger;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		if (executeImmediately)
		{
			_logger.LogInformation("Service execution");
			await ExecutePeriodicServiceTask();
			_logger.LogInformation("Service execution finished");
		}

		DateTimeOffset? lastOccurence = null;
		while (!stoppingToken.IsCancellationRequested)
		{
			DateTimeOffset? nextOccurence = cronExpression.GetNextOccurrence(DateTimeOffset.UtcNow, TimeZoneInfo.Utc);
			if (nextOccurence is null)
			{
				_logger.LogInformation("No next execution occurence, exiting.");
				return;
			}

			if (lastOccurence is not null && lastOccurence == nextOccurence)
			{
				await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
				continue;
			}

			TimeSpan maxDelay = TimeSpan.FromDays(24);
			while (nextOccurence.Value > DateTimeOffset.UtcNow && !stoppingToken.IsCancellationRequested)
			{
				TimeSpan timeToWait = nextOccurence.Value - DateTimeOffset.UtcNow;
				if (timeToWait <= TimeSpan.Zero)
				{
					break;
				}

				TimeSpan delay = timeToWait > maxDelay ? maxDelay : timeToWait;
				await Task.Delay(delay, stoppingToken);
			}

			lastOccurence = nextOccurence;

			if (stoppingToken.IsCancellationRequested) break;
			IDisposable? scope = _logger.BeginScope("Occurence: {Occurence}", nextOccurence);
			await using Defer _ = Disposables.Defer(scope, static s => s?.Dispose());
			_logger.LogInformation("Service execution");
			await ExecutePeriodicServiceTask();
			_logger.LogInformation("Service execution finished");
		}

		return;

		async Task ExecutePeriodicServiceTask()
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			await using Defer _ = Disposables.Defer((stopwatch, _logger), static state =>
			{
				state.stopwatch.Stop();
				state._logger.LogInformation("Service execution took {Time}", state.stopwatch.Elapsed);
			});

			await Task.Run(() => ExecutePeriodically(stoppingToken)
				.WithExceptionProtection(_logger, "Service execution failed!", cancellationToken: stoppingToken), stoppingToken);
		}
	}

	protected abstract Task ExecutePeriodically(CancellationToken cancellationToken);
}
