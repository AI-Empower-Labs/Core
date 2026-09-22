// ReSharper disable CheckNamespace

namespace System;

/// <summary>
/// Represents a builder for asynchronously disposing resources in a defined order.
/// </summary>
public class AsyncDisposableBag : AsyncDefer
{
	/// <summary>
	/// Initializes a new, empty instance of the <see cref="AsyncDisposableBag"/> class.
	/// </summary>
	public AsyncDisposableBag()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="AsyncDisposableBag"/> class with an asynchronous action to execute upon disposal.
	/// </summary>
	/// <param name="asyncAction">The asynchronous action to defer.</param>
	public AsyncDisposableBag(Func<Task> asyncAction) : base(asyncAction)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="AsyncDisposableBag"/> class with an asynchronous action accepting a cancellation token.
	/// </summary>
	/// <param name="asyncAction">The asynchronous action to defer.</param>
	public AsyncDisposableBag(Func<CancellationToken, Task> asyncAction) : base(asyncAction)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="AsyncDisposableBag"/> class with a synchronous action to execute upon disposal.
	/// </summary>
	/// <param name="action">The action to defer.</param>
	public AsyncDisposableBag(Action action) : base(action)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="AsyncDisposableBag"/> class with an asynchronous disposable resource.
	/// </summary>
	/// <param name="disposable">The asynchronous disposable to defer.</param>
	public AsyncDisposableBag(IAsyncDisposable disposable) : base(disposable)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="AsyncDisposableBag"/> class with a synchronous disposable resource.
	/// </summary>
	/// <param name="disposable">The synchronous disposable to defer.</param>
	public AsyncDisposableBag(IDisposable disposable) : base(disposable)
	{
	}
}
