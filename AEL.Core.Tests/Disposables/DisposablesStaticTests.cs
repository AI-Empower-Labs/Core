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

    [Fact]
    public void Defer_Action_And_State_ExecutesOnDispose()
    {
        bool actionRun = false;
        using (Disposables.Defer(() => actionRun = true))
        {
            Assert.False(actionRun);
        }
        Assert.True(actionRun);

        string? stateResult = null;
        using (Disposables.Defer("hello", s => stateResult = s))
        {
            Assert.Null(stateResult);
        }
        Assert.Equal("hello", stateResult);

        string? optionalStateResult = null;
        using (Disposables.Defer((string s) => optionalStateResult = s, "optional"))
        {
            Assert.Null(optionalStateResult);
        }
        Assert.Equal("optional", optionalStateResult);

        string? defaultStateResult = "not-default";
        using (Disposables.Defer((string? s) => defaultStateResult = s))
        {
            Assert.Equal("not-default", defaultStateResult);
        }
        Assert.Null(defaultStateResult);
    }

    [Fact]
    public async Task DeferAsync_Func_And_State_ExecutesOnDisposeAsync()
    {
        bool funcRun = false;
        await using (Disposables.DeferAsync(() => { funcRun = true; return Task.CompletedTask; }))
        {
            Assert.False(funcRun);
        }
        Assert.True(funcRun);

        bool ctRun = false;
        await using (Disposables.DeferAsync(_ => { ctRun = true; return Task.CompletedTask; }))
        {
            Assert.False(ctRun);
        }
        Assert.True(ctRun);

        string? stateResult = null;
        await using (Disposables.DeferAsync("world", s => { stateResult = s; return Task.CompletedTask; }))
        {
            Assert.Null(stateResult);
        }
        Assert.Equal("world", stateResult);

        string? stateCtResult = null;
        await using (Disposables.DeferAsync("world-ct", (s, _) => { stateCtResult = s; return Task.CompletedTask; }))
        {
            Assert.Null(stateCtResult);
        }
        Assert.Equal("world-ct", stateCtResult);

        string? optionalAsyncStateResult = null;
        await using (Disposables.DeferAsync((string s) => { optionalAsyncStateResult = s; return Task.CompletedTask; }, "optional-async"))
        {
            Assert.Null(optionalAsyncStateResult);
        }
        Assert.Equal("optional-async", optionalAsyncStateResult);

        string? optionalAsyncCtStateResult = null;
        await using (Disposables.DeferAsync((string s, CancellationToken _) => { optionalAsyncCtStateResult = s; return Task.CompletedTask; }, "optional-ct"))
        {
            Assert.Null(optionalAsyncCtStateResult);
        }
        Assert.Equal("optional-ct", optionalAsyncCtStateResult);

        string? syncActionStateResult = null;
        await using (Disposables.DeferAsync((string s) => { syncActionStateResult = s; }, "sync-action"))
        {
            Assert.Null(syncActionStateResult);
        }
        Assert.Equal("sync-action", syncActionStateResult);
    }

    [Fact]
    public async Task CreateAsync_ExecutesOnDisposeAsync()
    {
        bool executed = false;
        IAsyncDisposable disposable = Disposables.CreateAsync(_ =>
        {
            executed = true;
            return Task.CompletedTask;
        });
        await disposable.DisposeAsync();
        Assert.True(executed);

        string? stateValue = null;
        IAsyncDisposable stateDisposable = Disposables.CreateAsync("state", (s, _) =>
        {
            stateValue = s;
            return Task.CompletedTask;
        });
        await stateDisposable.DisposeAsync();
        Assert.Equal("state", stateValue);
    }
}