// ReSharper disable CheckNamespace

namespace System;

public abstract class AsyncDisposableBase : IAsyncDisposable
{
	private AsyncDefer? _disposableBag;
	private Lazy<CancellationTokenSource>? _lazyCancellationTokenSource;
	private long _disposeSignaled;

	protected CancellationToken CancellationToken => LazyCancellationTokenSource.Value.Token;

	public bool IsDisposed => Interlocked.Read(ref _disposeSignaled) != 0;

	private Lazy<CancellationTokenSource> LazyCancellationTokenSource => _lazyCancellationTokenSource ??= new Lazy<CancellationTokenSource>(() => new CancellationTokenSource(), true);

	public AsyncDefer DisposableBag => _disposableBag ??= new AsyncDefer();

	public async ValueTask DisposeAsync()
	{
		if (!SignalDispose())
		{
			return;
		}

		try
		{
			if (_disposableBag is not null)
			{
				using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(10));
				await _disposableBag.DisposeAsync(timeout.Token);
			}
		}
		finally
		{
			CancelCancellationTokenSource();
			GC.SuppressFinalize(this);
		}
	}

	private void CancelCancellationTokenSource()
	{
		if (_lazyCancellationTokenSource is not null && _lazyCancellationTokenSource.IsValueCreated)
		{
			using CancellationTokenSource cancellationTokenSource = _lazyCancellationTokenSource.Value;
			cancellationTokenSource.Cancel(false);
		}
	}

	private bool SignalDispose() => Interlocked.CompareExchange(ref _disposeSignaled, 1, 0) == 0;
}
