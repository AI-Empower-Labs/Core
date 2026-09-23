// ReSharper disable CheckNamespace

namespace System;

public static class Disposables
{
	public static readonly IDisposable Empty = new EmptyDisposable();

	public static readonly IAsyncDisposable EmptyAsync = new EmptyDisposable();

	/// <summary>
	/// Combines the disposables into one that disposes them in reverse order.
	/// </summary>
	public static IDisposable Combine(params IDisposable[] disposables)
	{
		ArgumentNullException.ThrowIfNull(disposables);
		Defer defer = new();
		foreach (IDisposable disposable in disposables)
		{
			defer.Add(disposable);
		}

		return defer;
	}
}
