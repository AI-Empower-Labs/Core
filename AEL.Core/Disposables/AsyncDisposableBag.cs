// ReSharper disable CheckNamespace

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace System;

/// <summary>
/// Represents a builder for asynchronously disposing resources in a defined order.
/// </summary>
/// <remarks>
/// This class allows you to add disposables and dispose tasks which will be executed in reverse order
/// when <see cref="DisposeAsync"/> is called. The disposables and tasks can be added using <see cref="Add(Func{Task})"/>,
/// <see cref="Add(Action)"/>, <see cref="Add(IDisposable)"/>, and <see cref="Add(IAsyncDisposable)"/> methods.
/// The <see cref="DisposeAsync"/> method should be called to initiate the disposal process. Once disposed, the
/// <see cref="IsDisposed"/> property will return true to indicate that disposal is completed.
/// </remarks>
public sealed class AsyncDisposableBag : IAsyncDisposable
{
	/// <summary>
	/// Represents a variable used to signal the dispose operation has been signaled or not.
	/// </summary>
	private long _disposeSignaled;
	/// <summary>
	/// A stack to store asynchronous clean-up tasks that will be executed when disposing an object.
	/// </summary>
	private readonly ConcurrentStack<Func<Task>> _disposeTasks = new();

	/// <summary>
	/// Gets a value indicating whether the object has been disposed.
	/// </summary>
	/// <value>
	/// <c>true</c> if the object has been disposed; otherwise, <c>false</c>.
	/// </value>
	/// <remarks>
	/// The <see cref="IsDisposed"/> property returns <c>true</c> if the object has been disposed;
	/// otherwise, it returns <c>false</c>.
	/// </remarks>
	/// <example>
	/// This example demonstrates how to check if the object has been disposed.
	/// <code>
	/// if (obj.IsDisposed)
	/// {
	/// Console.WriteLine("The object has already been disposed.");
	/// }
	/// </code>
	/// </example>
	public bool IsDisposed => Interlocked.Read(ref _disposeSignaled) != 0;

	/// <summary>
	/// Adds a dispose task to the stack of tasks to be executed when disposing.
	/// </summary>
	/// <param name="disposeTask">The dispose task to be added. Must not be null.</param>
	/// <exception cref="ArgumentNullException">
	/// Thrown when the <paramref name="disposeTask"/> is null.
	/// </exception>
	public void Add(Func<Task> disposeTask)
	{
		ArgumentNullException.ThrowIfNull(disposeTask);
		_disposeTasks.Push(disposeTask);
	}

	/// <summary>
	/// Adds an action to the list of actions to be executed.
	/// </summary>
	/// <param name="action">The action to be added.</param>
	public void Add(Action action)
	{
		ArgumentNullException.ThrowIfNull(action);
		Add(() =>
		{
			action();
			return Task.CompletedTask;
		});
	}

	/// <summary>
	/// Adds the specified <paramref name="disposable"/> to the collection of disposables.
	/// </summary>
	/// <param name="disposable">The disposable object to be added. Cannot be null.</param>
	public void Add(IDisposable disposable)
	{
		ArgumentNullException.ThrowIfNull(disposable);
		Add(() =>
		{
			disposable.Dispose();
			return Task.CompletedTask;
		});
	}

	/// <summary>
	/// Adds an IAsyncDisposable object to the collection.
	/// </summary>
	/// <param name="disposable">The IAsyncDisposable object to add.</param>
	public void Add(IAsyncDisposable disposable)
	{
		ArgumentNullException.ThrowIfNull(disposable);
		Add(() => disposable.DisposeAsync().AsTask());
	}

	/// <summary>
	/// Asynchronously releases the resources used by the object.
	/// </summary>
	/// <returns>A task representing the asynchronous operation.</returns>
	public async ValueTask DisposeAsync()
	{
		if (!SignalDispose())
		{
			return;
		}

		await HandleDisposeTasks();
		SuppressFinalize();
	}

	/// <summary>
	/// Signals the dispose operation.
	/// </summary>
	/// <returns>
	/// <c>true</c> if the dispose operation is signaled; otherwise, <c>false</c>.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool SignalDispose()
	{
		return Interlocked.CompareExchange(ref _disposeSignaled, 1, 0) != 1;
	}

	/// <summary>
	/// Handles the disposal tasks by executing the tasks in the stack.
	/// </summary>
	/// <returns>
	/// A Task representing the asynchronous operation.
	/// The Task is completed when all the disposal tasks have been executed.
	/// </returns>
	/// <remarks>
	/// This method pops tasks from the stack of disposal tasks and executes them in a loop.
	/// It awaits each task and handles any exceptions that may occur during the task execution.
	/// The method also ensures that the tasks are executed in a non-blocking manner by using the ConfigureAwait(false) method.
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private async Task HandleDisposeTasks()
	{
		List<Exception>? exceptions = null;
		while (_disposeTasks.TryPop(out Func<Task>? disposable))
		{
			Task shutdownTask = disposable();
			try
			{
				await shutdownTask;
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

	/// Prevents the finalization of an object.
	/// This method is called to suppress the finalization of an object, preventing it from being scheduled for finalization
	/// by the garbage collector. Once this method is called, the object will not be finalized unless the Finalize method
	/// is explicitly called on it.
	/// @remarks
	/// This method is typically called by the Dispose method of an object that implements the IDisposable interface.
	/// By calling SuppressFinalize, the finalization method for the object is not run automatically. This can improve
	/// performance in certain scenarios where the object is no longer needed and the finalization code is not required.
	/// @see <a href="https://docs.microsoft.com/en-us/dotnet/api/system.gc.suppressfinalize">SuppressFinalize Method (GC)</a>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SuppressFinalize()
	{
		// Take yourself off the finalization queue
		// to prevent finalization from executing a second time.
		GC.SuppressFinalize(this);
	}
}
