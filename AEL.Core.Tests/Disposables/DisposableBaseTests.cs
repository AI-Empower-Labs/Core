namespace AEL.Core.Tests.Disposables;

using System;
using System.Threading;
using Xunit;

public sealed class DisposableBaseTests
{
    private sealed class DerivedDisposable : DisposableBase
    {
        public CancellationToken Token => CancellationToken; // expose
        public DisposableBag Bag => DisposableBag; // expose
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
}
