#if NET10_0_OR_GREATER

// ReSharper disable once CheckNamespace
namespace System;

public static class DisposableExtensions
{
	extension(IDisposable disposable)
	{
		public static void DisposeWith(DisposableBag disposableBag) => disposableBag.Add(disposableBag);
	}

	extension(IAsyncDisposable disposable)
	{
		public static void DisposeWith(AsyncDisposableBag disposableBag) => disposableBag.Add(disposableBag);
	}
}
#endif
