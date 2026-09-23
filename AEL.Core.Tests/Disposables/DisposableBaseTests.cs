namespace AEL.Core.Tests.Disposables;

using System;
using System.Threading;
using Xunit;

public sealed class DisposableBaseTests
{
    private sealed class DerivedDisposable : DisposableBase
    {
        public CancellationToken Token => CancellationToken; // expose
        public Defer Bag => DisposableBag; // expose
    }

    [Fact]
    public void Dispose_CancelsToken_IfCreated_And_DisposesBag()
    {
        DerivedDisposable d = new();
        bool innerDisposed = false;
        d.Bag.Add(() => innerDisposed = true);

        // Access token to force creation
        CancellationToken token = d.Token;
        Assert.False(token.IsCancellationRequested);

        d.Dispose();

        Assert.True(d.IsDisposed);
        Assert.True(innerDisposed);
        Assert.True(token.IsCancellationRequested);

        // Idempotent
        d.Dispose();
        Assert.True(d.IsDisposed);
    }

    [Fact]
    public void Dispose_WithoutTokenAccess_DoesNotThrow()
    {
        DerivedDisposable d = new();
        d.Dispose();
        Assert.True(d.IsDisposed);
    }

    [Fact]
    public void Dispose_WhenBagThrows_StillCancelsToken()
    {
        DerivedDisposable d = new();
        d.Bag.Add(() => throw new InvalidOperationException("Cleanup error"));
        CancellationToken token = d.Token;

        Assert.Throws<AggregateException>(() => d.Dispose());
        Assert.True(d.IsDisposed);
        Assert.True(token.IsCancellationRequested);
    }
}
