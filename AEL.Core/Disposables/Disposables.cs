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
		DisposableBag bag = new();
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
		DisposableBag bag = new();
		bag.Add(disposable1);
		bag.Add(disposable2);
		bag.Add(disposable3);
		return bag;
	}
}
