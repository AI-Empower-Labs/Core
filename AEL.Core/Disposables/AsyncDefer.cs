// ReSharper disable CheckNamespace

namespace System;

/// <summary>
/// Asynchronous Go-like defer scope. Registered actions run in LIFO order on disposal; if any throw,
/// the remaining actions still run and the exceptions are rethrown as an <see cref="AggregateException"/>.
/// </summary>
public class AsyncDefer : DeferBase<Func<CancellationToken, Task>>, IAsyncDisposable
{
	public AsyncDefer()
	{
	}

	public AsyncDefer(Func<Task> asyncAction) => Add(asyncAction);

	public AsyncDefer(Func<CancellationToken, Task> asyncAction) => Add(asyncAction);

	public AsyncDefer(Action action) => Add(action);

	public AsyncDefer(IAsyncDisposable disposable) => Add(disposable);

	public AsyncDefer(IDisposable disposable) => Add(disposable);

	public void Add(Func<Task> asyncAction)
	{
		ArgumentNullException.ThrowIfNull(asyncAction);
		DisposeTasks.Push(_ => asyncAction());
	}

	public void Add(Func<CancellationToken, Task> asyncAction)
	{
		ArgumentNullException.ThrowIfNull(asyncAction);
		DisposeTasks.Push(asyncAction);
	}

	public void Add(Action action)
	{
		ArgumentNullException.ThrowIfNull(action);
		DisposeTasks.Push(_ =>
		{
			action();
			return Task.CompletedTask;
		});
	}

	public void Add(IAsyncDisposable disposable)
	{
		ArgumentNullException.ThrowIfNull(disposable);
		DisposeTasks.Push(_ => disposable.DisposeAsync().AsTask());
	}

	public void Add(IDisposable disposable)
	{
		ArgumentNullException.ThrowIfNull(disposable);
		Add(disposable.Dispose);
	}

	public void Add<T>(T state, Func<T, Task> asyncAction)
	{
		ArgumentNullException.ThrowIfNull(asyncAction);
		DisposeTasks.Push(_ => asyncAction(state));
	}

	public void Add<T>(T state, Func<T, CancellationToken, Task> asyncAction)
	{
		ArgumentNullException.ThrowIfNull(asyncAction);
		DisposeTasks.Push(token => asyncAction(state, token));
	}

	public void Add<T>(T state, Action<T> action)
	{
		ArgumentNullException.ThrowIfNull(action);
		Add(() => action(state));
	}

	public ValueTask DisposeAsync() => DisposeAsync(CancellationToken.None);

	/// <param name="cancellationToken">Passed to every deferred action that accepts a <see cref="CancellationToken"/>.</param>
	public async ValueTask DisposeAsync(CancellationToken cancellationToken)
	{
		if (!SignalDispose())
		{
			return;
		}

		List<Exception>? exceptions = null;
		while (DisposeTasks.TryPop(out Func<CancellationToken, Task>? asyncAction))
		{
			try
			{
				await asyncAction(cancellationToken);
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

	public static AsyncDefer Action(Func<Task> asyncAction) => new(asyncAction);

	public static AsyncDefer Action(Func<CancellationToken, Task> asyncAction) => new(asyncAction);

	public static AsyncDefer Action(Action action) => new(action);

	public static AsyncDefer Action<T>(T state, Func<T, Task> asyncAction)
	{
		AsyncDefer defer = new();
		defer.Add(state, asyncAction);
		return defer;
	}

	public static AsyncDefer Action<T>(T state, Func<T, CancellationToken, Task> asyncAction)
	{
		AsyncDefer defer = new();
		defer.Add(state, asyncAction);
		return defer;
	}

	public static AsyncDefer Action<T>(T state, Action<T> action)
	{
		AsyncDefer defer = new();
		defer.Add(state, action);
		return defer;
	}

	/// <example>
	/// <code>
	/// await using AsyncDefer _ = AsyncDefer.Scope(out Action&lt;Func&lt;Task&gt;&gt; defer);
	/// defer(async () => await Cleanup1Async());
	/// defer(async () => await Cleanup2Async());
	/// </code>
	/// </example>
	public static AsyncDefer Scope(out Action<Func<Task>> defer)
	{
		AsyncDefer instance = new();
		defer = instance.Add;
		return instance;
	}
}
