namespace System;

/// <summary>
/// Collects tasks and disposables; on asynchronous disposal every task is awaited and every
/// disposable disposed, in reverse order. Failures surface as an <see cref="AggregateException"/>.
/// </summary>
public sealed class AsyncCompletionScope : AsyncDisposableBase
{
	public void Add(Task task) => DisposableBag.Add(() => task);

	public void Add(ValueTask valueTask) => DisposableBag.Add(async () => await valueTask);

	public void Add(IDisposable disposable) => DisposableBag.Add(disposable);

	public void Add(IAsyncDisposable disposable) => DisposableBag.Add(disposable);

	public void Add(Stream stream) => DisposableBag.Add((IAsyncDisposable)stream);
}
