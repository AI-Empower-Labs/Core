// ReSharper disable CheckNamespace

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace System;

/// <summary>
/// Provides an asynchronous Go-like defer pattern for executing clean-up actions when exiting an async scope.
/// Registered actions are executed in LIFO (last-in, first-out) order upon asynchronous disposal.
/// </summary>
public sealed class AsyncDefer : IAsyncDisposable
{
	private long _disposeSignaled;
	private readonly ConcurrentStack<Func<CancellationToken, Task>> _disposeTasks = new();

	/// <summary>
	/// Gets a value indicating whether the defer instance has been disposed.
	/// </summary>
	public bool IsDisposed => Interlocked.Read(ref _disposeSignaled) != 0;

	/// <summary>
	/// Gets the number of pending deferred actions.
	/// </summary>
	public int Count => _disposeTasks.Count;

	/// <summary>
	/// Initializes a new, empty instance of the <see cref="AsyncDefer"/> class acting as an async deferral scope.
	/// </summary>
	public AsyncDefer()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="AsyncDefer"/> class with an asynchronous action to execute upon disposal.
	/// </summary>
	/// <param name="asyncAction">The asynchronous action to defer.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="asyncAction"/> is null.</exception>
	public AsyncDefer(Func<Task> asyncAction)
	{
		ArgumentNullException.ThrowIfNull(asyncAction);
		_disposeTasks.Push(_ => asyncAction());
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="AsyncDefer"/> class with an asynchronous action accepting a cancellation token.
	/// </summary>
	/// <param name="asyncAction">The asynchronous action to defer.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="asyncAction"/> is null.</exception>
	public AsyncDefer(Func<CancellationToken, Task> asyncAction)
	{
		ArgumentNullException.ThrowIfNull(asyncAction);
		_disposeTasks.Push(asyncAction);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="AsyncDefer"/> class with a synchronous action to execute upon disposal.
	/// </summary>
	/// <param name="action">The action to defer.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
	public AsyncDefer(Action action)
	{
		ArgumentNullException.ThrowIfNull(action);
		_disposeTasks.Push(_ =>
		{
			action();
			return Task.CompletedTask;
		});
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="AsyncDefer"/> class with an asynchronous disposable resource.
	/// </summary>
	/// <param name="disposable">The asynchronous disposable to defer.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="disposable"/> is null.</exception>
	public AsyncDefer(IAsyncDisposable disposable)
	{
		ArgumentNullException.ThrowIfNull(disposable);
		_disposeTasks.Push(_ => disposable.DisposeAsync().AsTask());
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="AsyncDefer"/> class with a synchronous disposable resource.
	/// </summary>
	/// <param name="disposable">The synchronous disposable to defer.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="disposable"/> is null.</exception>
	public AsyncDefer(IDisposable disposable)
	{
		ArgumentNullException.ThrowIfNull(disposable);
		_disposeTasks.Push(_ =>
		{
			disposable.Dispose();
			return Task.CompletedTask;
		});
	}

	/// <summary>
	/// Adds an asynchronous action to be executed when this scope is asynchronously disposed.
	/// </summary>
	/// <param name="asyncAction">The asynchronous action to defer.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="asyncAction"/> is null.</exception>
	public void Add(Func<Task> asyncAction)
	{
		ArgumentNullException.ThrowIfNull(asyncAction);
		_disposeTasks.Push(_ => asyncAction());
	}

	/// <summary>
	/// Adds an asynchronous action with cancellation support to be executed when this scope is asynchronously disposed.
	/// </summary>
	/// <param name="asyncAction">The asynchronous action to defer.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="asyncAction"/> is null.</exception>
	public void Add(Func<CancellationToken, Task> asyncAction)
	{
		ArgumentNullException.ThrowIfNull(asyncAction);
		_disposeTasks.Push(asyncAction);
	}

	/// <summary>
	/// Adds a synchronous action to be executed when this scope is asynchronously disposed.
	/// </summary>
	/// <param name="action">The action to defer.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> is null.</exception>
	public void Add(Action action)
	{
		ArgumentNullException.ThrowIfNull(action);
		_disposeTasks.Push(_ =>
		{
			action();
			return Task.CompletedTask;
		});
	}

	/// <summary>
	/// Adds an asynchronous disposable object to be disposed when this scope is asynchronously disposed.
	/// </summary>
	/// <param name="disposable">The asynchronous disposable to defer.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="disposable"/> is null.</exception>
	public void Add(IAsyncDisposable disposable)
	{
		ArgumentNullException.ThrowIfNull(disposable);
		_disposeTasks.Push(_ => disposable.DisposeAsync().AsTask());
	}

	/// <summary>
	/// Adds a synchronous disposable object to be disposed when this scope is asynchronously disposed.
	/// </summary>
	/// <param name="disposable">The synchronous disposable to defer.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="disposable"/> is null.</exception>
	public void Add(IDisposable disposable)
	{
		ArgumentNullException.ThrowIfNull(disposable);
		_disposeTasks.Push(_ =>
		{
			disposable.Dispose();
			return Task.CompletedTask;
		});
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
	/// Asynchronously releases the resources used by this instance.
	/// </summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	public ValueTask DisposeAsync()
	{
		return DisposeAsync(CancellationToken.None);
	}

	/// <summary>
	/// Asynchronously releases the resources used by this instance with cancellation support.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token that may be passed to deferred tasks.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	public async ValueTask DisposeAsync(CancellationToken cancellationToken)
	{
		if (!SignalDispose())
		{
			return;
		}

		await HandleDisposeTasks(cancellationToken);
		GC.SuppressFinalize(this);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool SignalDispose()
	{
		return Interlocked.CompareExchange(ref _disposeSignaled, 1, 0) != 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private async Task HandleDisposeTasks(CancellationToken cancellationToken)
	{
		List<Exception>? exceptions = null;
		while (_disposeTasks.TryPop(out Func<CancellationToken, Task>? disposable))
		{
			try
			{
				await disposable(cancellationToken);
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
	/// Creates an asynchronous deferred clean-up action that will execute when the returned <see cref="AsyncDefer"/> is disposed.
	/// </summary>
	/// <param name="asyncAction">The asynchronous action to defer.</param>
	/// <returns>An asynchronous disposable <see cref="AsyncDefer"/> instance.</returns>
	public static AsyncDefer Action(Func<Task> asyncAction) => new(asyncAction);

	/// <summary>
	/// Creates an asynchronous deferred clean-up action with cancellation support.
	/// </summary>
	/// <param name="asyncAction">The asynchronous action to defer.</param>
	/// <returns>An asynchronous disposable <see cref="AsyncDefer"/> instance.</returns>
	public static AsyncDefer Action(Func<CancellationToken, Task> asyncAction) => new(asyncAction);

	/// <summary>
	/// Creates a deferred asynchronous clean-up action with state to avoid closure allocations.
	/// </summary>
	/// <typeparam name="T">The type of the state object.</typeparam>
	/// <param name="state">The state passed to the action.</param>
	/// <param name="asyncAction">The asynchronous action to defer.</param>
	/// <returns>An asynchronous disposable <see cref="AsyncDefer"/> instance.</returns>
	public static AsyncDefer Action<T>(T state, Func<T, CancellationToken, Task> asyncAction)
	{
		ArgumentNullException.ThrowIfNull(asyncAction);
		return new AsyncDefer(token => asyncAction(state, token));
	}

	/// <summary>
	/// Creates a deferred clean-up action that will execute synchronously when the returned <see cref="AsyncDefer"/> is disposed.
	/// </summary>
	/// <param name="action">The action to defer.</param>
	/// <returns>An asynchronous disposable <see cref="AsyncDefer"/> instance.</returns>
	public static AsyncDefer Action(Action action) => new(action);

	/// <summary>
	/// Creates an asynchronous deferred clean-up action that will execute when the returned <see cref="AsyncDefer"/> is disposed.
	/// Alias for <see cref="Action(Func{Task})"/>.
	/// </summary>
	/// <param name="asyncAction">The asynchronous action to defer.</param>
	/// <returns>An asynchronous disposable <see cref="AsyncDefer"/> instance.</returns>
	public static AsyncDefer Run(Func<Task> asyncAction) => new(asyncAction);

	/// <summary>
	/// Creates an asynchronous deferred clean-up action with cancellation support.
	/// Alias for <see cref="Action(Func{CancellationToken, Task})"/>.
	/// </summary>
	/// <param name="asyncAction">The asynchronous action to defer.</param>
	/// <returns>An asynchronous disposable <see cref="AsyncDefer"/> instance.</returns>
	public static AsyncDefer Run(Func<CancellationToken, Task> asyncAction) => new(asyncAction);

	/// <summary>
	/// Creates a deferred clean-up action that will execute synchronously when the returned <see cref="AsyncDefer"/> is disposed.
	/// Alias for <see cref="Action(Action)"/>.
	/// </summary>
	/// <param name="action">The action to defer.</param>
	/// <returns>An asynchronous disposable <see cref="AsyncDefer"/> instance.</returns>
	public static AsyncDefer Run(Action action) => new(action);

	/// <summary>
	/// Creates an asynchronous deferred clean-up action.
	/// Alias for <see cref="Action(Func{Task})"/>.
	/// </summary>
	/// <param name="asyncAction">The asynchronous action to defer.</param>
	/// <returns>An asynchronous disposable <see cref="AsyncDefer"/> instance.</returns>
	public static AsyncDefer Create(Func<Task> asyncAction) => new(asyncAction);

	/// <summary>
	/// Creates a new asynchronous deferral scope that can register multiple actions.
	/// </summary>
	/// <returns>A new <see cref="AsyncDefer"/> scope.</returns>
	public static AsyncDefer Scope() => new();

	/// <summary>
	/// Creates a new asynchronous deferral scope and outputs a delegate to register asynchronous deferred actions.
	/// </summary>
	/// <param name="defer">A delegate to register asynchronous actions in this scope.</param>
	/// <returns>A new <see cref="AsyncDefer"/> scope.</returns>
	/// <example>
	/// <code>
	/// await using var _ = AsyncDefer.Scope(out var defer);
	/// defer(async () => await cleanup1Async());
	/// defer(async () => await cleanup2Async());
	/// </code>
	/// </example>
	public static AsyncDefer Scope(out Action<Func<Task>> defer)
	{
		AsyncDefer instance = new();
		defer = instance.Add;
		return instance;
	}

	#endregion
}
