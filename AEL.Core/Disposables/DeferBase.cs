// ReSharper disable CheckNamespace

using System.Collections.Concurrent;

namespace System;

public abstract class DeferBase<TTask>
{
	private long _disposeSignaled;

	protected readonly ConcurrentStack<TTask> DisposeTasks = new();

	public bool IsDisposed => Interlocked.Read(ref _disposeSignaled) != 0;

	public int Count => DisposeTasks.Count;

	/// <summary>
	/// Removes all pending actions without running them, e.g. to cancel a rollback once an operation succeeds.
	/// </summary>
	public void Dismiss() => DisposeTasks.Clear();

	/// <returns><c>true</c> for the first caller only.</returns>
	protected bool SignalDispose() => Interlocked.CompareExchange(ref _disposeSignaled, 1, 0) == 0;
}
