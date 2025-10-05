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

		while (!stoppingToken.IsCancellationRequested)
		{
			DateTimeOffset? nextOccurence = cronExpression.GetNextOccurrence(DateTimeOffset.UtcNow, TimeZoneInfo.Utc);
			if (nextOccurence is null)
			{
				_logger.LogInformation("No next execution occurence, exiting.");
				return;
			}

			TimeSpan timeToWait = nextOccurence.Value - DateTimeOffset.UtcNow;
			await Task.Delay(timeToWait, stoppingToken).WithSilentCancellation(cancellationToken: stoppingToken);
			if (stoppingToken.IsCancellationRequested) continue;
			using IDisposable? scope = _logger.BeginScope("Occurence: {Occurence}", nextOccurence);
			_logger.LogInformation("Service execution");
			await ExecutePeriodicServiceTask();
			_logger.LogInformation("Service execution finished");
		}

		return;

		async Task ExecutePeriodicServiceTask()
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				await Task.Run(() => ExecutePeriodically(stoppingToken), stoppingToken)
					.WithExceptionProtection(_logger, "Service execution failed!", cancellationToken: stoppingToken);
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
