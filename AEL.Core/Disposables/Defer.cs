// ReSharper disable CheckNamespace

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace System;

/// <summary>
/// Provides a Go-like defer pattern for executing clean-up actions when exiting a scope.
/// Registered actions are executed in LIFO (last-in, first-out) order upon disposal.
/// </summary>
public sealed class Defer : IDisposable
{
	private long _disposeSignaled;
	private readonly ConcurrentStack<Action> _disposeTasks = new();

	/// <summary>
	/// Gets a value indicating whether the defer instance has been disposed.
	/// </summary>
	public bool IsDisposed => Interlocked.Read(ref _disposeSignaled) != 0;

	/// <summary>
	/// Gets the number of pending deferred actions.
	/// </summary>
	public int Count => _disposeTasks.Count;

	/// <summary>
	/// Initializes a new, empty instance of the <see cref="Defer"/> class acting as a deferral scope.
	/// </summary>
	public Defer()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Defer"/> class with a single action to execute upon disposal.
	/// </summary>
	/// <param name="action">The action to defer until disposal.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
	public Defer(Action action)
	{
		ArgumentNullException.ThrowIfNull(action);
		_disposeTasks.Push(action);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Defer"/> class with a disposable resource to dispose upon disposal.
	/// </summary>
	/// <param name="disposable">The disposable resource to dispose.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="disposable"/> is null.</exception>
	public Defer(IDisposable disposable)
	{
		ArgumentNullException.ThrowIfNull(disposable);
		_disposeTasks.Push(disposable.Dispose);
	}

	/// <summary>
	/// Adds an action to be executed when this scope is disposed.
	/// </summary>
	/// <param name="action">The action to defer.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
	public void Add(Action action)
	{
		ArgumentNullException.ThrowIfNull(action);
		_disposeTasks.Push(action);
	}

	/// <summary>
	/// Adds a disposable object whose disposal will be executed when this scope is disposed.
	/// </summary>
	/// <param name="disposable">The disposable resource to defer.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="disposable"/> is null.</exception>
	public void Add(IDisposable disposable)
	{
		ArgumentNullException.ThrowIfNull(disposable);
		_disposeTasks.Push(disposable.Dispose);
	}


	/// <summary>
	/// Clears all pending deferred actions without executing them.
	/// </summary>
	public void Clear()
	{
		_disposeTasks.Clear();
	}

	/// <summary>
	/// Dismisses all pending deferred actions without executing them. Alias for <see cref="Clear"/>.
	/// </summary>
	public void Dismiss() => Clear();

	/// <summary>
	/// Executes all deferred actions in reverse order of addition (LIFO).
	/// If any actions throw exceptions, the remaining actions are still executed and all exceptions are aggregated.
	/// </summary>
	public void Dispose()
	{
		if (!SignalDispose())
		{
			return;
		}

		HandleDisposeTasks();
		GC.SuppressFinalize(this);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool SignalDispose()
	{
		return Interlocked.CompareExchange(ref _disposeSignaled, 1, 0) != 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void HandleDisposeTasks()
	{
		List<Exception>? exceptions = null;
		while (_disposeTasks.TryPop(out Action? disposable))
		{
			try
			{
				disposable.Invoke();
			}
			catch (Exception e)
			{
				exceptions ??= [];
				exceptions.Add(e);
			}
		}

		if (exceptions is not null)
		{
			throw new AggregateException(exceptions);
		}
	}

	#region Static Factory Methods

	/// <summary>
	/// Creates a deferred clean-up action that will execute when the returned <see cref="Defer"/> is disposed.
	/// </summary>
	/// <param name="action">The action to defer.</param>
	/// <returns>A disposable <see cref="Defer"/> instance.</returns>
	public static Defer Action(Action action) => new(action);

	/// <summary>
	/// Creates a deferred clean-up action with state to avoid closure allocations.
	/// </summary>
	/// <typeparam name="T">The type of the state object.</typeparam>
	/// <param name="state">The state passed to the action.</param>
	/// <param name="action">The action to defer.</param>
	/// <returns>A disposable <see cref="Defer"/> instance.</returns>
	public static Defer Action<T>(T state, Action<T> action)
	{
		ArgumentNullException.ThrowIfNull(action);
		return new Defer(() => action(state));
	}

	/// <summary>
	/// Creates a deferred clean-up action that will execute when the returned <see cref="Defer"/> is disposed.
	/// Alias for <see cref="Action(Action)"/>.
	/// </summary>
	/// <param name="action">The action to defer.</param>
	/// <returns>A disposable <see cref="Defer"/> instance.</returns>
	public static Defer Run(Action action) => new(action);

	/// <summary>
	/// Creates a deferred clean-up action that will execute when the returned <see cref="Defer"/> is disposed.
	/// Alias for <see cref="Action(Action)"/>.
	/// </summary>
	/// <param name="action">The action to defer.</param>
	/// <returns>A disposable <see cref="Defer"/> instance.</returns>
	public static Defer Create(Action action) => new(action);

	/// <summary>
	/// Defers the disposal of the specified disposable object until the returned <see cref="Defer"/> is disposed.
	/// </summary>
	/// <param name="disposable">The disposable object to defer.</param>
	/// <returns>A disposable <see cref="Defer"/> instance.</returns>
	public static Defer Disposable(IDisposable disposable) => new(disposable);

	/// <summary>
	/// Creates a new deferral scope that can register multiple actions.
	/// </summary>
	/// <returns>A new <see cref="Defer"/> scope.</returns>
	public static Defer Scope() => new();

	/// <summary>
	/// Creates a new deferral scope and outputs a delegate to register deferred actions.
	/// </summary>
	/// <param name="defer">A delegate to register actions in this scope.</param>
	/// <returns>A new <see cref="Defer"/> scope.</returns>
	/// <example>
	/// <code>
	/// using var _ = Defer.Scope(out var defer);
	/// defer(() => cleanup1());
	/// defer(() => cleanup2());
	/// </code>
	/// </example>
	public static Defer Scope(out Action<Action> defer)
	{
		Defer instance = new();
		defer = instance.Add;
		return instance;
	}

	/// <summary>
	/// Creates a deferred asynchronous clean-up action.
	/// </summary>
	/// <param name="action">The asynchronous action to defer.</param>
	/// <returns>An asynchronous disposable <see cref="AsyncDefer"/> instance.</returns>
	public static AsyncDefer Async(Func<Task> action) => new(action);

	/// <summary>
	/// Creates a deferred asynchronous clean-up action with cancellation support.
	/// </summary>
	/// <param name="action">The asynchronous action to defer.</param>
	/// <returns>An asynchronous disposable <see cref="AsyncDefer"/> instance.</returns>
	public static AsyncDefer Async(Func<CancellationToken, Task> action) => new(action);

	/// <summary>
	/// Creates a deferred asynchronous clean-up action with state.
	/// </summary>
	/// <typeparam name="T">The type of the state object.</typeparam>
	/// <param name="state">The state passed to the action.</param>
	/// <param name="action">The asynchronous action to defer.</param>
	/// <returns>An asynchronous disposable <see cref="AsyncDefer"/> instance.</returns>
	public static AsyncDefer Async<T>(T state, Func<T, CancellationToken, Task> action) => AsyncDefer.Action(state, action);

	/// <summary>
	/// Defers the asynchronous disposal of the specified resource.
	/// </summary>
	/// <param name="disposable">The asynchronous disposable to defer.</param>
	/// <returns>An asynchronous disposable <see cref="AsyncDefer"/> instance.</returns>
	public static AsyncDefer Async(IAsyncDisposable disposable) => new(disposable);

	/// <summary>
	/// Defers the disposal of the specified synchronous disposable in an asynchronous context.
	/// </summary>
	/// <param name="disposable">The synchronous disposable to defer.</param>
	/// <returns>An asynchronous disposable <see cref="AsyncDefer"/> instance.</returns>
	public static AsyncDefer Async(IDisposable disposable) => new(disposable);

	/// <summary>
	/// Creates a new asynchronous deferral scope.
	/// </summary>
	/// <returns>A new <see cref="AsyncDefer"/> scope.</returns>
	public static AsyncDefer AsyncScope() => new();

	/// <summary>
	/// Creates a new asynchronous deferral scope and outputs a delegate to register asynchronous deferred actions.
	/// </summary>
	/// <param name="defer">A delegate to register asynchronous actions in this scope.</param>
	/// <returns>A new <see cref="AsyncDefer"/> scope.</returns>
	public static AsyncDefer AsyncScope(out Action<Func<Task>> defer) => AsyncDefer.Scope(out defer);

	#endregion
}
