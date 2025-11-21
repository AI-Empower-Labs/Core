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

			TimeSpan timeToWait = nextOccurence.Value - DateTimeOffset.UtcNow;
			if (timeToWait <= TimeSpan.Zero)
			{
				await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
				continue;
			}

			lastOccurence = nextOccurence;
			await Task.Delay(timeToWait, stoppingToken);

			if (stoppingToken.IsCancellationRequested) break;
			IDisposable? scope = _logger.BeginScope("Occurence: {Occurence}", nextOccurence);
			try
			{
				_logger.LogInformation("Service execution");
				await ExecutePeriodicServiceTask()
					.WithExceptionProtection(_logger, "Service execution failed!", cancellationToken: stoppingToken);
				_logger.LogInformation("Service execution finished");
			}
			finally
			{
				scope?.Dispose();
			}
		}

		return;

		async Task ExecutePeriodicServiceTask()
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				await ExecutePeriodically(stoppingToken);
			}
			finally
			{
				stopwatch.Stop();
				_logger.LogInformation("Service execution took {Time}", stopwatch.Elapsed);
			}
		}
	}

	protected abstract Task ExecutePeriodically(CancellationToken cancellationToken);
}
