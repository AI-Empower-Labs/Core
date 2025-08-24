namespace System;

/// <summary>
/// A convenience scope for aggregating asynchronous and synchronous disposables,
/// as well as tasks, and ensuring they are completed/disposed when the scope
/// itself is asynchronously disposed.
/// </summary>
/// <remarks>
/// This type builds on <see cref="AsyncDisposableBase"/> and uses its internal
/// disposable bag to track registrations. Any <see cref="Task"/> or
/// <see cref="ValueTask"/> added is awaited during asynchronous disposal; any
/// <see cref="IDisposable"/> is disposed; and any <see cref="IAsyncDisposable"/>
/// is asynchronously disposed.
/// <para>
/// Exceptions thrown by awaited tasks or by dispose operations will surface
/// when disposing this scope.
/// </para>
/// <para>
/// Arguments to the Add methods must be non-null.
/// </para>
/// </remarks>
public sealed class AsyncCompletionScope : AsyncDisposableBase
{
	/// <summary>
	/// Registers a task to be awaited when this scope is asynchronously disposed.
	/// </summary>
	/// <param name="task">The task whose completion should be awaited.</param>
	/// <remarks>
	/// If the task faults, the exception will be observed during scope disposal.
	/// </remarks>
	public void Add(Task task)
	{
		// Register an asynchronous delegate that awaits the provided task on disposal.
		DisposableBag.Add(async () =>
		{
			await task;
		});
	}

	/// <summary>
	/// Registers a value task to be awaited when this scope is asynchronously disposed.
	/// </summary>
	/// <param name="valueTask">The value task whose completion should be awaited.</param>
	/// <remarks>
	/// If the value task faults, the exception will be observed during scope disposal.
	/// </remarks>
	public void Add(ValueTask valueTask)
	{
		// Register an asynchronous delegate that awaits the provided value task on disposal.
		DisposableBag.Add(async () =>
		{
			await valueTask;
		});
	}

	/// <summary>
	/// Registers a synchronous disposable to be disposed when this scope is asynchronously disposed.
	/// </summary>
	/// <param name="disposable">The disposable to register.</param>
	public void Add(IDisposable disposable)
	{
		// Synchronous disposables are disposed during scope disposal.
		DisposableBag.Add(disposable);
	}

	/// <summary>
	/// Registers an asynchronous disposable to be disposed when this scope is asynchronously disposed.
	/// </summary>
	/// <param name="disposable">The asynchronous disposable to register.</param>
	public void Add(IAsyncDisposable disposable)
	{
		// Asynchronous disposables are awaited during scope disposal.
		DisposableBag.Add(disposable);
	}

	/// <summary>
	/// Registers a stream to be asynchronously disposed when this scope is disposed.
	/// </summary>
	/// <param name="stream">The stream to register. The stream is treated as an <see cref="IAsyncDisposable"/>.</param>
	/// <remarks>
	/// Most streams implement <see cref="IAsyncDisposable"/>; this method casts the stream and
	/// registers it for asynchronous disposal.
	/// </remarks>
	public void Add(Stream stream)
	{
		Add((IAsyncDisposable)stream);
	}
}
