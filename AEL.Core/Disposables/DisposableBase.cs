// ReSharper disable CheckNamespace

namespace System;

public abstract class DisposableBase : IDisposable
{
	private Defer? _disposableBag;
	private Lazy<CancellationTokenSource>? _lazyCancellationTokenSource;
	private long _disposeSignaled;

	protected CancellationToken CancellationToken => LazyCancellationTokenSource.Value.Token;

	public bool IsDisposed => Interlocked.Read(ref _disposeSignaled) != 0;

	private Lazy<CancellationTokenSource> LazyCancellationTokenSource => _lazyCancellationTokenSource ??= new Lazy<CancellationTokenSource>(() => new CancellationTokenSource(), true);

	public Defer DisposableBag => _disposableBag ??= new Defer();

	public void Dispose()
	{
		if (!SignalDispose())
		{
			return;
		}

		try
		{
			_disposableBag?.Dispose();
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
