// ReSharper disable CheckNamespace

using System.Runtime.CompilerServices;

namespace System;

public abstract class AsyncDisposableBase : IAsyncDisposable
{
	private AsyncDisposableBag? _disposableBuilder;
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
			_disposableBuilder ??= new AsyncDisposableBag();
			return _disposableBuilder;
		}
	}

	public async ValueTask DisposeAsync()
	{
		if (!SignalDispose())
		{
			return;
		}

		await Cleanup();
		await DisposeBuilder();
		CancelCancellationTokenSource();
		SuppressFinalize();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private async Task DisposeBuilder()
	{
		if (_disposableBuilder is not null)
		{
			await _disposableBuilder.DisposeAsync();
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

	/// <summary>
	///     Do cleanup here
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected virtual Task Cleanup()
	{
		return Task.CompletedTask;
	}
}
