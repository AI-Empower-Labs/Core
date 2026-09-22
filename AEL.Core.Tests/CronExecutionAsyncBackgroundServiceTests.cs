namespace AEL.Core.Tests;

using Cronos;
using Microsoft.Extensions.Logging;
using Xunit;

public sealed class CronExecutionAsyncBackgroundServiceTests
{
    private sealed class TestLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }
    }

    private sealed class TestCronService(
        CronExpression expression,
        ILogger logger,
        bool executeImmediately = false)
        : CronExecutionAsyncBackgroundService(expression, logger, executeImmediately)
    {
        public TaskCompletionSource ExecutedTcs { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int ExecutionCount { get; private set; }

        protected override Task ExecutePeriodically(CancellationToken cancellationToken)
        {
            ExecutionCount++;
            ExecutedTcs.TrySetResult();
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task ExecuteImmediately_ExecutesPeriodicTaskOnStartup()
    {
        // Scheduled once a year on Jan 1
        CronExpression annual = CronExpression.Parse("0 0 1 1 *");
        TestCronService service = new(annual, new TestLogger(), executeImmediately: true);

        using CancellationTokenSource cts = new();
        await service.StartAsync(cts.Token);

        await service.ExecutedTcs.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Assert.Equal(1, service.ExecutionCount);

        await service.StopAsync(cts.Token);
    }

    [Fact]
    public async Task FarFutureOccurrence_DoesNotThrowArgumentOutOfRangeException()
    {
        // Scheduled once a year, delay could be up to 365 days (> 49.7 day limit for Task.Delay without chunking)
        CronExpression annual = CronExpression.Parse("0 0 1 1 *");
        TestCronService service = new(annual, new TestLogger(), executeImmediately: false);

        using CancellationTokenSource cts = new();
        // Starting the service enters ExecuteAsync and calculates the delay
        await service.StartAsync(cts.Token);

        // Give it time to enter the delay loop
        await Task.Delay(50, TestContext.Current.CancellationToken);

        // Stopping should cancel delay cleanly without ArgumentOutOfRangeException
        await service.StopAsync(cts.Token);
        Assert.Equal(0, service.ExecutionCount);
    }
}
