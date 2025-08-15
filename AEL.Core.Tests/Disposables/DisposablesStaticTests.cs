namespace AEL.Core.Tests.Disposables;

using System;
using System.Threading.Tasks;
using Xunit;

public sealed class DisposablesStaticTests
{
    [Fact]
    public void Empty_Disposable_NoThrow_MultipleTimes()
    {
        IDisposable empty = Disposables.Empty;
        empty.Dispose();
        empty.Dispose();
    }

    [Fact]
    public async Task EmptyAsync_Disposable_NoThrow_MultipleTimes()
    {
        IAsyncDisposable empty = Disposables.EmptyAsync;
        await empty.DisposeAsync();
        await empty.DisposeAsync();
    }

    private sealed class CountDisposable : IDisposable
    {
        public int Count;
        public void Dispose() => Count++;
    }

    [Fact]
    public void Combine_Two_DisposesBoth()
    {
        CountDisposable a = new();
        CountDisposable b = new();
        using IDisposable combined = Disposables.Combine(a, b);
        combined.Dispose();
        Assert.Equal(1, a.Count);
        Assert.Equal(1, b.Count);
    }

    [Fact]
    public void Combine_Three_DisposesAll()
    {
        CountDisposable a = new();
        CountDisposable b = new();
        CountDisposable c = new();
        using IDisposable combined = Disposables.Combine(a, b, c);
        combined.Dispose();
        Assert.Equal(1, a.Count);
        Assert.Equal(1, b.Count);
        Assert.Equal(1, c.Count);
    }
}