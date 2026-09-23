// ReSharper disable CheckNamespace

namespace System;

public sealed class EmptyDisposable : IDisposable, IAsyncDisposable
{
	public void Dispose()
	{
	}

	public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
