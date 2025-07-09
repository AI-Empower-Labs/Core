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

		PreventObjectFinalization();
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

	/// <summary>
	/// Removes the current object from the finalization queue, preventing it from being finalized.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void PreventObjectFinalization()
	{
		// Take yourself off the finalization queue
		// to prevent finalization from executing a second time.
		GC.SuppressFinalize(this);
	}
}
