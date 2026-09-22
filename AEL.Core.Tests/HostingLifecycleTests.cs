namespace AEL.Core.Tests;

using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Xunit;

public sealed class HostingLifecycleTests
{
    private sealed class TrackingHost(IHost inner) : IHost
    {
        public bool IsDisposed { get; private set; }
        public IServiceProvider Services => inner.Services;

        public void Dispose()
        {
            IsDisposed = true;
            inner.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken = default) => inner.StartAsync(cancellationToken);
        public Task StopAsync(CancellationToken cancellationToken = default) => inner.StopAsync(cancellationToken);
    }

    [Fact]
    public async Task HostBuilder_Build_WhenConfigureHostFails_DisposesBuiltHost()
    {
        TrackingHost? trackingHost = null;

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await AEL.Core.HostBuilder.Build<TrackingHost, HostApplicationBuilder>(
                args: [],
                create: _ => new HostApplicationBuilder(),
                configureBuilder: null,
                build: b =>
                {
                    trackingHost = new TrackingHost(b.Build());
                    return trackingHost;
                },
                configureHost: _ => throw new InvalidOperationException("ConfigureHost failed"),
                cancellationToken: CancellationToken.None,
                assemblies: Assembly.GetExecutingAssembly());
        });

        Assert.NotNull(trackingHost);
        Assert.True(trackingHost.IsDisposed);
    }

    [Fact]
    public async Task TestRunner_Start_WhenBuildOrStartFails_DisposesStartupAndHost()
    {
        CultureInfo? previousCulture = CultureInfo.DefaultThreadCurrentCulture;
        using Defer _ = System.Disposables.Defer(() => CultureInfo.DefaultThreadCurrentCulture = previousCulture);
        CultureInfo danish = CultureInfo.GetCultureInfo("da-DK");
        TrackingHost? trackingHost = null;

        CultureInfo.DefaultThreadCurrentCulture = danish;

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await TestRunner.Start<TrackingHost, HostApplicationBuilder>(
                args: [],
                create: _ => new HostApplicationBuilder(),
                build: b =>
                {
                    trackingHost = new TrackingHost(b.Build());
                    return trackingHost;
                },
                startHostedServices: false,
                cancellationToken: CancellationToken.None,
                configureBuilder: null,
                configureHost: _ => throw new InvalidOperationException("Startup failure"),
                assemblies: Assembly.GetExecutingAssembly());
        });

        // Culture should be restored to danish because Startup was disposed in catch block
        Assert.Equal(danish, CultureInfo.DefaultThreadCurrentCulture);
        Assert.NotNull(trackingHost);
        Assert.True(trackingHost.IsDisposed);
    }
}
