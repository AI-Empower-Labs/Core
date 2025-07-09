// ReSharper disable CheckNamespace

namespace System;

/// <summary>
/// Represents an empty disposable object that implements both IDisposable and IAsyncDisposable interfaces.
/// This object does not perform any actual disposal actions.
/// </summary>
public sealed class EmptyDisposable : IDisposable, IAsyncDisposable
{
	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	/// <remarks>
	/// Implement this method to release any unmanaged resources that are held by the current object before it is destroyed.
	/// This method is automatically called by the .NET runtime when the object is being garbage collected, or explicitly called by the developer to release unmanaged resources.
	/// </remarks>
	public void Dispose()
	{
	}

	/// <summary>
	/// Asynchronously releases the resources used by the object.
	/// </summary>
	/// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
	public ValueTask DisposeAsync()
	{
		return ValueTask.CompletedTask;
	}
}
