// ReSharper disable CheckNamespace

using System.Runtime.CompilerServices;

namespace System;

/// <summary>
/// Provides a Go-like defer pattern for executing clean-up actions when exiting a scope.
/// Registered actions are executed in LIFO (last-in, first-out) order upon disposal.
/// </summary>
public class Defer : DeferBase<Action>, IDisposable, IAsyncDisposable
{
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
		DisposeTasks.Push(action);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Defer"/> class with a disposable resource to dispose upon disposal.
	/// </summary>
	/// <param name="disposable">The disposable resource to dispose.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="disposable"/> is null.</exception>
	public Defer(IDisposable disposable)
	{
		ArgumentNullException.ThrowIfNull(disposable);
		DisposeTasks.Push(disposable.Dispose);
	}

	/// <summary>
	/// Adds an action to be executed when this scope is disposed.
	/// </summary>
	/// <param name="action">The action to defer.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
	public void Add(Action action)
	{
		ArgumentNullException.ThrowIfNull(action);
		DisposeTasks.Push(action);
	}

	/// <summary>
	/// Adds a parameterized action with state to be executed when this scope is disposed.
	/// </summary>
	/// <typeparam name="T">The type of the state object.</typeparam>
	/// <param name="action">The action to defer.</param>
	/// <param name="state">The optional state passed to the action.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
	public void Add<T>(Action<T> action, T state = default!)
	{
		ArgumentNullException.ThrowIfNull(action);
		DisposeTasks.Push(() => action(state));
	}

	/// <summary>
	/// Adds a parameterized action with state to be executed when this scope is disposed.
	/// </summary>
	/// <typeparam name="T">The type of the state object.</typeparam>
	/// <param name="state">The state passed to the action.</param>
	/// <param name="action">The action to defer.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
	public void Add<T>(T state, Action<T> action) => Add(action, state);

	/// <summary>
	/// Adds a disposable object whose disposal will be executed when this scope is disposed.
	/// </summary>
	/// <param name="disposable">The disposable resource to defer.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="disposable"/> is null.</exception>
	public void Add(IDisposable disposable)
	{
		ArgumentNullException.ThrowIfNull(disposable);
		DisposeTasks.Push(disposable.Dispose);
	}


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

	/// <summary>
	/// Asynchronously executes all deferred actions.
	/// </summary>
	/// <returns>A completed task representing the asynchronous disposal.</returns>
	public ValueTask DisposeAsync()
	{
		Dispose();
		return ValueTask.CompletedTask;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void HandleDisposeTasks()
	{
		List<Exception>? exceptions = null;
		while (DisposeTasks.TryPop(out Action? disposable))
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
	/// Creates a deferred clean-up action with state to avoid closure allocations.
	/// </summary>
	/// <typeparam name="T">The type of the state object.</typeparam>
	/// <param name="state">The state passed to the action.</param>
	/// <param name="action">The action to defer.</param>
	/// <returns>A disposable <see cref="Defer"/> instance.</returns>
	public static Defer Run<T>(T state, Action<T> action) => Action(state, action);

	/// <summary>
	/// Creates a deferred clean-up action that will execute when the returned <see cref="Defer"/> is disposed.
	/// Alias for <see cref="Action(Action)"/>.
	/// </summary>
	/// <param name="action">The action to defer.</param>
	/// <returns>A disposable <see cref="Defer"/> instance.</returns>
	public static Defer Create(Action action) => new(action);

	/// <summary>
	/// Creates a deferred clean-up action with state to avoid closure allocations.
	/// </summary>
	/// <typeparam name="T">The type of the state object.</typeparam>
	/// <param name="state">The state passed to the action.</param>
	/// <param name="action">The action to defer.</param>
	/// <returns>A disposable <see cref="Defer"/> instance.</returns>
	public static Defer Create<T>(T state, Action<T> action) => Action(state, action);

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
	/// <returns>An asynchronous disposable <see cref="DeferAsync"/> instance.</returns>
	public static DeferAsync Async(Func<Task> action) => new(action);

	/// <summary>
	/// Creates a deferred asynchronous clean-up action with cancellation support.
	/// </summary>
	/// <param name="action">The asynchronous action to defer.</param>
	/// <returns>An asynchronous disposable <see cref="DeferAsync"/> instance.</returns>
	public static DeferAsync Async(Func<CancellationToken, Task> action) => new(action);

	/// <summary>
	/// Creates a deferred asynchronous clean-up action with state.
	/// </summary>
	/// <typeparam name="T">The type of the state object.</typeparam>
	/// <param name="state">The state passed to the action.</param>
	/// <param name="action">The asynchronous action to defer.</param>
	/// <returns>An asynchronous disposable <see cref="DeferAsync"/> instance.</returns>
	public static DeferAsync Async<T>(T state, Func<T, CancellationToken, Task> action)
	{
		ArgumentNullException.ThrowIfNull(action);
		return new DeferAsync(ct => action(state, ct));
	}

	/// <summary>
	/// Creates a deferred asynchronous clean-up action with optional state.
	/// </summary>
	/// <typeparam name="T">The type of the state object.</typeparam>
	/// <param name="action">The asynchronous action to defer.</param>
	/// <param name="state">The optional state passed to the action.</param>
	/// <returns>An asynchronous disposable <see cref="DeferAsync"/> instance.</returns>
	public static DeferAsync Async<T>(Func<T, CancellationToken, Task> action, T state = default!) => Async(state, action);

	/// <summary>
	/// Creates a deferred asynchronous clean-up action with state.
	/// </summary>
	/// <typeparam name="T">The type of the state object.</typeparam>
	/// <param name="state">The state passed to the action.</param>
	/// <param name="action">The asynchronous action to defer.</param>
	/// <returns>An asynchronous disposable <see cref="DeferAsync"/> instance.</returns>
	public static DeferAsync Async<T>(T state, Func<T, Task> action)
	{
		ArgumentNullException.ThrowIfNull(action);
		return new DeferAsync(async () => await action(state));
	}

	/// <summary>
	/// Creates a deferred asynchronous clean-up action with optional state.
	/// </summary>
	/// <typeparam name="T">The type of the state object.</typeparam>
	/// <param name="action">The asynchronous action to defer.</param>
	/// <param name="state">The optional state passed to the action.</param>
	/// <returns>An asynchronous disposable <see cref="DeferAsync"/> instance.</returns>
	public static DeferAsync Async<T>(Func<T, Task> action, T state = default!) => Async(state, action);

	/// <summary>
	/// Creates a deferred synchronous clean-up action with state in an asynchronous context.
	/// </summary>
	/// <typeparam name="T">The type of the state object.</typeparam>
	/// <param name="state">The state passed to the action.</param>
	/// <param name="action">The action to defer.</param>
	/// <returns>An asynchronous disposable <see cref="DeferAsync"/> instance.</returns>
	public static DeferAsync Async<T>(T state, Action<T> action)
	{
		ArgumentNullException.ThrowIfNull(action);
		return new DeferAsync(() => action(state));
	}

	/// <summary>
	/// Creates a deferred synchronous clean-up action with optional state in an asynchronous context.
	/// </summary>
	/// <typeparam name="T">The type of the state object.</typeparam>
	/// <param name="action">The action to defer.</param>
	/// <param name="state">The optional state passed to the action.</param>
	/// <returns>An asynchronous disposable <see cref="DeferAsync"/> instance.</returns>
	public static DeferAsync Async<T>(Action<T> action, T state = default!) => Async(state, action);

	/// <summary>
	/// Defers the asynchronous disposal of the specified resource.
	/// </summary>
	/// <param name="disposable">The asynchronous disposable to defer.</param>
	/// <returns>An asynchronous disposable <see cref="DeferAsync"/> instance.</returns>
	public static DeferAsync Async(IAsyncDisposable disposable) => new(disposable);

	/// <summary>
	/// Defers the disposal of the specified synchronous disposable in an asynchronous context.
	/// </summary>
	/// <param name="disposable">The synchronous disposable to defer.</param>
	/// <returns>An asynchronous disposable <see cref="DeferAsync"/> instance.</returns>
	public static DeferAsync Async(IDisposable disposable) => new(disposable);

	/// <summary>
	/// Creates a new asynchronous deferral scope.
	/// </summary>
	/// <returns>A new <see cref="DeferAsync"/> scope.</returns>
	public static DeferAsync AsyncScope() => new();

	/// <summary>
	/// Creates a new asynchronous deferral scope and outputs a delegate to register asynchronous deferred actions.
	/// </summary>
	/// <param name="defer">A delegate to register asynchronous actions in this scope.</param>
	/// <returns>A new <see cref="DeferAsync"/> scope.</returns>
	public static DeferAsync AsyncScope(out Action<Func<Task>> defer)
	{
		DeferAsync instance = new();
		defer = instance.Add;
		return instance;
	}

	#endregion
}
