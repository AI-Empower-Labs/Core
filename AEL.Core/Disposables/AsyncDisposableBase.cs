// ReSharper disable CheckNamespace

using System.Runtime.CompilerServices;

namespace System;

public abstract class AsyncDisposableBase : IAsyncDisposable
{
	private AsyncDisposableBag? _disposableBag;
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

	public AsyncDisposableBag DisposableBag
	{
		get
		{
			_disposableBag ??= new AsyncDisposableBag();
			return _disposableBag;
		}
	}

	public async ValueTask DisposeAsync()
	{
		if (!SignalDispose())
		{
			return;
		}

		await DisposeBag();
		CancelCancellationTokenSource();
		SuppressFinalize();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private async Task DisposeBag()
	{
		if (_disposableBag is not null)
		{
			await _disposableBag.DisposeAsync();
		}
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
	private void SuppressFinalize()
	{
		// Take yourself off the finalization queue
		// to prevent finalization from executing a second time.
		GC.SuppressFinalize(this);
	}
}
