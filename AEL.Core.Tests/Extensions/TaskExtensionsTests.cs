using Microsoft.Extensions.Logging;

namespace AEL.Core.Tests.Extensions;

public class TaskExtensionsTests
{
	[Fact]
	public async Task WithCancellation_Cancels()
	{
		CancellationTokenSource cts = new(TimeSpan.FromMilliseconds(50));
		Task delay = Task.Delay(TimeSpan.FromSeconds(10));
		await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await delay.WithCancellation(cts.Token));
	}

	[Fact]
	public async Task WithSilentCancellation_SwallowsCancellation()
	{
		CancellationTokenSource cts = new(TimeSpan.FromMilliseconds(10));
		Task delay = Task.Delay(TimeSpan.FromSeconds(1));
		await delay.WithSilentCancellation(cts.Token);
		Assert.True(cts.IsCancellationRequested);
	}

	[Fact]
	public async Task WithSilentException_ReturnsDefaultOnError()
	{
		Task<int> faulty = Task.FromException<int>(new InvalidOperationException());
		int result = await faulty.WithSilentException(42);
		Assert.Equal(42, result);

		Task faulty2 = Task.FromException(new InvalidOperationException());
		await faulty2.WithSilentException();
	}

	private sealed class TestLogger2 : ILogger
	{
		public Exception? LastEx;
		IDisposable? ILogger.BeginScope<TState>(TState state) => new Dummy();
		public bool IsEnabled(LogLevel logLevel) => true;
		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			LastEx = exception;
		}

		private sealed class Dummy : IDisposable
		{
			public void Dispose()
			{
			}
		}
	}

	[Fact]
	public async Task WithExecuteWithProtection_LogsExceptions()
	{
		TestLogger2 logger = new();
		Task faulty = Task.Run(() => throw new InvalidOperationException());
		await faulty.WithExecuteWithProtection(logger, "msg");
		Assert.NotNull(logger.LastEx);

		TestLogger2 logger2 = new();
		Task<string> faultyT = Task.FromException<string>(new InvalidOperationException());
		string? res = await faultyT.WithExecuteWithProtection(logger2, "msg");
		Assert.Null(res);
		Assert.NotNull(logger2.LastEx);
	}

	[Fact]
	public async Task WithTimeout_ConvertsTimeoutToCancellation()
	{
		Task delay = Task.Delay(200);
		await Assert.ThrowsAsync<OperationCanceledException>(() => delay.WithTimeout(TimeSpan.FromMilliseconds(20)));

		Task<int> delay2 = Task.Run(async () =>
		{
			await Task.Delay(20);
			return 7;
		});
		int r = await delay2.WithTimeout(TimeSpan.FromSeconds(1));
		Assert.Equal(7, r);
	}

	[Fact]
	public void FireAndForget_DoesNotThrow()
	{
		Task.Run(() => throw new Exception()).FireAndForget();
		Task.CompletedTask.FireAndForget();
	}

	[Fact]
	public async Task ToTask_WrapsValue()
	{
		int v = await 5.ToTask();
		Assert.Equal(5, v);
	}
}
