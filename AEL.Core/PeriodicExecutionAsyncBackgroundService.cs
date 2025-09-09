using System.Diagnostics;

using Microsoft.Extensions.Logging;

namespace AEL.Core;

public abstract class PeriodicExecutionAsyncBackgroundService(
	bool startImmediately,
	TimeSpan period,
	ILogger logger) : AsyncBackgroundService(logger)
{
	private readonly ILogger _logger = logger;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		if (startImmediately)
		{
			_logger.LogInformation("Service executing at startup");
			await ExecutePeriodicServiceTask();
		}

		while (!stoppingToken.IsCancellationRequested)
		{
			await Task.Delay(period, stoppingToken).WithSilentCancellation(cancellationToken: stoppingToken);
			if (stoppingToken.IsCancellationRequested) continue;
			_logger.LogInformation("Service execution after waiting period {Period}", period);
			await ExecutePeriodicServiceTask();
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
