// ReSharper disable CheckNamespace

namespace System;

/// <summary>
/// Provides an asynchronous Go-like defer pattern for executing clean-up actions when exiting an async scope.
/// </summary>
public class DeferAsync : AsyncDefer
{
	/// <summary>
	/// Initializes a new, empty instance of the <see cref="DeferAsync"/> class acting as an async deferral scope.
	/// </summary>
	public DeferAsync()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DeferAsync"/> class with an asynchronous action to execute upon disposal.
	/// </summary>
	/// <param name="asyncAction">The asynchronous action to defer.</param>
	public DeferAsync(Func<Task> asyncAction) : base(asyncAction)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DeferAsync"/> class with an asynchronous action accepting a cancellation token.
	/// </summary>
	/// <param name="asyncAction">The asynchronous action to defer.</param>
	public DeferAsync(Func<CancellationToken, Task> asyncAction) : base(asyncAction)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DeferAsync"/> class with a synchronous action to execute upon disposal.
	/// </summary>
	/// <param name="action">The action to defer.</param>
	public DeferAsync(Action action) : base(action)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DeferAsync"/> class with an asynchronous disposable resource.
	/// </summary>
	/// <param name="disposable">The asynchronous disposable to defer.</param>
	public DeferAsync(IAsyncDisposable disposable) : base(disposable)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DeferAsync"/> class with a synchronous disposable resource.
	/// </summary>
	/// <param name="disposable">The synchronous disposable to defer.</param>
	public DeferAsync(IDisposable disposable) : base(disposable)
	{
	}
}
