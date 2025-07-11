// ReSharper disable CheckNamespace

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace System;

/// <summary>
/// Represents a disposable builder.
/// </summary>
public sealed class DisposableBag : IDisposable
{
	/// <summary>
	/// Represents a flag indicating whether the disposal of an object has been signaled.
	/// </summary>
	private long _disposeSignaled;
	/// <summary>
	/// Private variable that represents a stack of actions to be executed when disposing an object.
	/// </summary>
	private readonly ConcurrentStack<Action> _disposeTasks = new();

	/// <summary>
	/// Gets a value indicating whether the object has been disposed.
	/// </summary>
	/// <remarks>
	/// The IsDisposed property returns true if the object has been disposed; otherwise, false.
	/// </remarks>
	/// <returns>
	/// true if the object has been disposed; otherwise, false.
	/// </returns>
	public bool IsDisposed => Interlocked.Read(ref _disposeSignaled) != 0;

	/// <summary>
	/// Adds an action to the collection of dispose tasks.
	/// </summary>
	/// <param name="action">The action to be added.</param>
	/// <exception cref="ArgumentNullException">Thrown if the <paramref name="action"/> is null.</exception>
	public void Add(Action action)
	{
		ArgumentNullException.ThrowIfNull(action);
		_disposeTasks.Push(action);
	}

	/// <summary>
	/// Adds a disposable object to the collection.
	/// </summary>
	/// <param name="disposable">The disposable object to add. Must not be null.</param>
	public void Add(IDisposable disposable)
	{
		ArgumentNullException.ThrowIfNull(disposable);
		Add(disposable.Dispose);
	}

	/// <summary>
	/// Performs application-defined tasks associated with releasing or resetting resources.
	/// </summary>
	public void Dispose()
	{
		if (!SignalDispose())
		{
			return;
		}

		HandleDisposeTasks();
		SuppressFinalize();
	}

	/// <summary>
	/// Signals the disposal of an object.
	/// </summary>
	/// <returns>
	/// true if the dispose signal was successfully set; otherwise, false.
	/// </returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool SignalDispose()
	{
		return Interlocked.CompareExchange(ref _disposeSignaled, 1, 0) != 1;
	}

	/// <summary>
	/// Handles the dispose tasks in a loop until all tasks are completed.
	/// </summary>
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
