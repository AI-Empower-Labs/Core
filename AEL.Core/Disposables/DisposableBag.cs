// ReSharper disable CheckNamespace

namespace System;

/// <summary>
/// Represents a disposable builder.
/// </summary>
public class DisposableBag : Defer
{
	/// <summary>
	/// Initializes a new, empty instance of the <see cref="DisposableBag"/> class.
	/// </summary>
	public DisposableBag()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DisposableBag"/> class with a single action to execute upon disposal.
	/// </summary>
	/// <param name="action">The action to defer until disposal.</param>
	public DisposableBag(Action action) : base(action)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DisposableBag"/> class with a disposable resource to dispose upon disposal.
	/// </summary>
	/// <param name="disposable">The disposable resource to defer.</param>
	public DisposableBag(IDisposable disposable) : base(disposable)
	{
	}
}
