// ReSharper disable CheckNamespace

using System.Runtime.CompilerServices;

namespace System;

public abstract class DisposableBase : IDisposable
{
	private DisposableBag? _disposableBag;
	private Lazy<CancellationTokenSource>? _lazyCancellationTokenSource;

	private long _disposeSignaled;

	protected CancellationToken CancellationToken => LazyCancellationTokenSource.Value.Token;

	public bool IsDisposed => Interlocked.Read(ref _disposeSignaled) != 0;

	private Lazy<CancellationTokenSource> LazyCancellationTokenSource
	{
		get
		{
			_lazyCancellationTokenSource ??= new(() => new CancellationTokenSource(), true);
			return _lazyCancellationTokenSource;
		}
	}

	public DisposableBag DisposableBag
	{
		get
		{
			_disposableBag ??= new DisposableBag();
			return _disposableBag;
		}
	}

	public void Dispose()
	{
		if (!SignalDispose())
		{
			return;
		}

		_disposableBag?.Dispose();
		CancelCancellationTokenSource();
		PreventObjectFinalization();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void CancelCancellationTokenSource()
	{
		if (_lazyCancellationTokenSource is not null && _lazyCancellationTokenSource.IsValueCreated)
		{
			using CancellationTokenSource cancellationTokenSource = _lazyCancellationTokenSource.Value;
			cancellationTokenSource.Cancel(false);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool SignalDispose()
	{
		return Interlocked.CompareExchange(ref _disposeSignaled, 1, 0) != 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void PreventObjectFinalization() =>
		// Take yourself off the finalization queue
		// to prevent finalization from executing a second time.
		GC.SuppressFinalize(this);
}
