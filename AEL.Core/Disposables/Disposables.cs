// ReSharper disable CheckNamespace

namespace System;

/// <summary>
/// Represents a class that provides utility methods for working with disposables.
/// </summary>
public sealed class Disposables
{
	/// <summary>
	/// Represents an empty reusable disposable object.
	/// </summary>
	public static readonly IDisposable Empty = new EmptyDisposable();
	/// <summary>
	/// Represents an empty, no-operation reusable asynchronous disposable object.
	/// </summary>
	public static readonly IAsyncDisposable EmptyAsync = new EmptyDisposable();

	/// <summary>
	/// Creates a new instance of DisposableBuilder.
	/// </summary>
	/// <returns>A new instance of DisposableBuilder.</returns>
	public static DisposableBag Create()
	{
		return new DisposableBag();
	}

	/// <summary>
	/// Creates a new instance of the <see cref="AsyncDisposableBag"/> class.
	/// </summary>
	/// <returns>
	/// A new instance of the <see cref="AsyncDisposableBag"/> class.
	/// </returns>
	public static AsyncDisposableBag CreateAsync()
	{
		return new AsyncDisposableBag();
	}

	public static IAsyncDisposable CreateAsync<T>(T state, Func<T, CancellationToken, Task> func)
	{
		ArgumentNullException.ThrowIfNull(func);
		return AsyncDefer.Action(state, func);
	}

	public static IAsyncDisposable CreateAsync(Func<CancellationToken, Task> func)
	{
		ArgumentNullException.ThrowIfNull(func);
		return DeferAsync(func);
	}

	/// <summary>
	/// Combines two instances of IDisposable into a single IDisposable.
	/// </summary>
	/// <param name="disposable1">The first IDisposable instance to combine.</param>
	/// <param name="disposable2">The second IDisposable instance to combine.</param>
	/// <returns>
	/// A single IDisposable instance that combines disposable1 and disposable2.
	/// </returns>
	public static IDisposable Combine(IDisposable disposable1, IDisposable disposable2)
	{
		Defer bag = Defer();
		bag.Add(disposable1);
		bag.Add(disposable2);
		return bag;
	}

	/// <summary>
	/// Combines multiple <see cref="IDisposable"/> objects into a single <see cref="IDisposable"/> object.
	/// </summary>
	/// <param name="disposable1">The first <see cref="IDisposable"/> object to combine.</param>
	/// <param name="disposable2">The second <see cref="IDisposable"/> object to combine.</param>
	/// <param name="disposable3">The third <see cref="IDisposable"/> object to combine.</param>
	/// <returns>A single <see cref="IDisposable"/> object that combines the specified <see cref="IDisposable"/> objects.</returns>
	public static IDisposable Combine(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3)
	{
		Defer bag = Defer();
		bag.Add(disposable1);
		bag.Add(disposable2);
		bag.Add(disposable3);
		return bag;
	}

	/// <summary>
	/// Defers the execution of the specified action until disposal.
	/// </summary>
	/// <param name="action">The action to defer.</param>
	/// <returns>A new <see cref="Defer"/> instance.</returns>
	public static Defer Defer(Action action) => new(action);

	/// <summary>
	/// Defers the execution of the specified parameterized action until disposal.
	/// </summary>
	/// <typeparam name="T">The type of the state object.</typeparam>
	/// <param name="state">The state passed to the action.</param>
	/// <param name="action">The action to defer.</param>
	/// <returns>A new <see cref="Defer"/> instance.</returns>
	public static Defer Defer<T>(T state, Action<T> action) => System.Defer.Action(state, action);

	/// <summary>
	/// Defers the execution of the specified parameterized action with optional state until disposal.
	/// </summary>
	/// <typeparam name="T">The type of the state object.</typeparam>
	/// <param name="action">The action to defer.</param>
	/// <param name="state">The optional state passed to the action.</param>
	/// <returns>A new <see cref="Defer"/> instance.</returns>
	public static Defer Defer<T>(Action<T> action, T state = default!) => System.Defer.Action(state, action);

	/// <summary>
	/// Creates a new <see cref="Defer"/> scope for registering multiple deferred actions.
	/// </summary>
	/// <returns>A new <see cref="Defer"/> scope.</returns>
	public static Defer Defer() => new();

	/// <summary>
	/// Defers the execution of the specified asynchronous action until asynchronous disposal.
	/// </summary>
	/// <param name="func">The asynchronous action to defer.</param>
	/// <returns>A new <see cref="DeferAsync"/> instance.</returns>
	public static DeferAsync DeferAsync(Func<Task> func) => new(func);

	/// <summary>
	/// Defers the execution of the specified asynchronous action with cancellation support until asynchronous disposal.
	/// </summary>
	/// <param name="func">The asynchronous action to defer.</param>
	/// <returns>A new <see cref="DeferAsync"/> instance.</returns>
	public static DeferAsync DeferAsync(Func<CancellationToken, Task> func) => new(func);

	/// <summary>
	/// Defers the execution of the specified parameterized asynchronous action until asynchronous disposal.
	/// </summary>
	/// <typeparam name="T">The type of the state object.</typeparam>
	/// <param name="state">The state passed to the action.</param>
	/// <param name="func">The asynchronous action to defer.</param>
	/// <returns>A new <see cref="DeferAsync"/> instance.</returns>
	public static DeferAsync DeferAsync<T>(T state, Func<T, Task> func) => System.Defer.Async(state, func);

	/// <summary>
	/// Defers the execution of the specified parameterized asynchronous action with optional state until asynchronous disposal.
	/// </summary>
	/// <typeparam name="T">The type of the state object.</typeparam>
	/// <param name="func">The asynchronous action to defer.</param>
	/// <param name="state">The optional state passed to the action.</param>
	/// <returns>A new <see cref="DeferAsync"/> instance.</returns>
	public static DeferAsync DeferAsync<T>(Func<T, Task> func, T state = default!) => System.Defer.Async(state, func);

	/// <summary>
	/// Defers the execution of the specified parameterized asynchronous action with cancellation support until asynchronous disposal.
	/// </summary>
	/// <typeparam name="T">The type of the state object.</typeparam>
	/// <param name="state">The state passed to the action.</param>
	/// <param name="func">The asynchronous action to defer.</param>
	/// <returns>A new <see cref="DeferAsync"/> instance.</returns>
	public static DeferAsync DeferAsync<T>(T state, Func<T, CancellationToken, Task> func) => System.Defer.Async(state, func);

	/// <summary>
	/// Defers the execution of the specified parameterized asynchronous action with cancellation support and optional state until asynchronous disposal.
	/// </summary>
	/// <typeparam name="T">The type of the state object.</typeparam>
	/// <param name="func">The asynchronous action to defer.</param>
	/// <param name="state">The optional state passed to the action.</param>
	/// <returns>A new <see cref="DeferAsync"/> instance.</returns>
	public static DeferAsync DeferAsync<T>(Func<T, CancellationToken, Task> func, T state = default!) => System.Defer.Async(state, func);

	/// <summary>
	/// Defers the execution of the specified synchronous action until asynchronous disposal.
	/// </summary>
	/// <param name="action">The action to defer.</param>
	/// <returns>A new <see cref="DeferAsync"/> instance.</returns>
	public static DeferAsync DeferAsync(Action action) => new(action);

	/// <summary>
	/// Defers the execution of the specified parameterized action with state until asynchronous disposal.
	/// </summary>
	/// <typeparam name="T">The type of the state object.</typeparam>
	/// <param name="state">The state passed to the action.</param>
	/// <param name="action">The action to defer.</param>
	/// <returns>A new <see cref="DeferAsync"/> instance.</returns>
	public static DeferAsync DeferAsync<T>(T state, Action<T> action) => System.Defer.Async(state, action);

	/// <summary>
	/// Defers the execution of the specified parameterized action with optional state until asynchronous disposal.
	/// </summary>
	/// <typeparam name="T">The type of the state object.</typeparam>
	/// <param name="action">The action to defer.</param>
	/// <param name="state">The optional state passed to the action.</param>
	/// <returns>A new <see cref="DeferAsync"/> instance.</returns>
	public static DeferAsync DeferAsync<T>(Action<T> action, T state = default!) => System.Defer.Async(state, action);

	/// <summary>
	/// Creates a new <see cref="DeferAsync"/> scope for registering multiple deferred actions.
	/// </summary>
	/// <returns>A new <see cref="DeferAsync"/> scope.</returns>
	public static DeferAsync DeferAsync() => new();
}
