// ReSharper disable CheckNamespace

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace System;

/// <summary>
/// Provides a base implementation for thread-safe deferral scopes and disposable bags.
/// </summary>
/// <typeparam name="TTask">The type of clean-up task held in the scope.</typeparam>
public abstract class DeferBase<TTask>
{
	private long _disposeSignaled;

	protected readonly ConcurrentStack<TTask> DisposeTasks = new();

	/// <summary>
	/// Gets a value indicating whether the instance has been disposed.
	/// </summary>
	public bool IsDisposed => Interlocked.Read(ref _disposeSignaled) != 0;

	/// <summary>
	/// Gets the number of pending deferred actions.
	/// </summary>
	public int Count => DisposeTasks.Count;

	/// <summary>
	/// Clears all pending deferred actions without executing them.
	/// </summary>
	public void Clear() => DisposeTasks.Clear();

	/// <summary>
	/// Dismisses all pending deferred actions without executing them. Alias for <see cref="Clear"/>.
	/// </summary>
	public void Dismiss() => Clear();

	/// <summary>
	/// Signals disposal once in a thread-safe manner.
	/// </summary>
	/// <returns><c>true</c> if disposal was transitioned from undisposed to disposed; otherwise, <c>false</c>.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected bool SignalDispose()
	{
		return Interlocked.CompareExchange(ref _disposeSignaled, 1, 0) != 1;
	}
}
