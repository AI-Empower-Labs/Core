// ReSharper disable CheckNamespace

namespace System;

/// <summary>
/// Go-like defer scope. Registered actions run in LIFO order on disposal; if any throw,
/// the remaining actions still run and the exceptions are rethrown as an <see cref="AggregateException"/>.
/// </summary>
public class Defer : DeferBase<Action>, IDisposable, IAsyncDisposable
{
	public Defer()
	{
	}

	public Defer(Action action) => Add(action);

	public Defer(IDisposable disposable) => Add(disposable);

	public void Add(Action action)
	{
		ArgumentNullException.ThrowIfNull(action);
		DisposeTasks.Push(action);
	}

	public void Add<T>(T state, Action<T> action)
	{
		ArgumentNullException.ThrowIfNull(action);
		DisposeTasks.Push(() => action(state));
	}

	public void Add(IDisposable disposable)
	{
		ArgumentNullException.ThrowIfNull(disposable);
		DisposeTasks.Push(disposable.Dispose);
	}

	public void Dispose()
	{
		if (!SignalDispose())
		{
			return;
		}

		List<Exception>? exceptions = null;
		while (DisposeTasks.TryPop(out Action? action))
		{
			try
			{
				action();
			}
			catch (Exception e)
			{
				exceptions ??= [];
				exceptions.Add(e);
			}
		}

		GC.SuppressFinalize(this);

		if (exceptions is not null)
		{
			throw new AggregateException(exceptions);
		}
	}

	public ValueTask DisposeAsync()
	{
		Dispose();
		return ValueTask.CompletedTask;
	}

	public static Defer Action(Action action) => new(action);

	public static Defer Action<T>(T state, Action<T> action)
	{
		Defer defer = new();
		defer.Add(state, action);
		return defer;
	}

	/// <example>
	/// <code>
	/// using Defer _ = Defer.Scope(out Action&lt;Action&gt; defer);
	/// defer(() => Cleanup1());
	/// defer(() => Cleanup2());
	/// </code>
	/// </example>
	public static Defer Scope(out Action<Action> defer)
	{
		Defer instance = new();
		defer = instance.Add;
		return instance;
	}
}
