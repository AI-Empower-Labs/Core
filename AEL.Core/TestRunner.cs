using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AEL.Core;

public sealed class TestRunner<THost> : AsyncDisposableBase
	where THost : IHost
{
	public TestRunner(THost host)
	{
		Host = host;
		DisposableBag.Add(host);
	}

	public THost Host { get; }
}

public static class TestRunner
{
	public static async Task<TestRunner<THost>> Start<THost, THostApplicationBuilder>(
		string[] args,
		Func<string[], THostApplicationBuilder> create,
		Func<THostApplicationBuilder, THost> build,
		bool startHostedServices,
		CancellationToken cancellationToken,
		Action<THostApplicationBuilder>? configureBuilder = null,
		Action<THost>? configureHost = null,
		params Assembly[] assemblies)
		where THost : IHost
		where THostApplicationBuilder : IHostApplicationBuilder
	{
		await using AsyncDefer rollback = new();
		Startup startup = new();
		rollback.Add(startup);

		THost host = await HostBuilder.Build(args, create,
			builder =>
			{
				configureBuilder?.Invoke(builder);
				if (startHostedServices) return;
				// Remove IHostedService service descriptors
				foreach (ServiceDescriptor serviceDescriptor in builder.Services.ToArray())
				{
					if (serviceDescriptor.ServiceType == typeof(IHostedService))
					{
						builder.Services.Remove(serviceDescriptor);
					}
				}
			},
			build, configureHost, cancellationToken, assemblies);

		rollback.Add(host, static async (h, _) =>
		{
			if (h is IAsyncDisposable asyncDisposable)
			{
				try
				{
					await asyncDisposable.DisposeAsync();
				}
				catch
				{
					// Ignore disposal failure
				}
			}
			else
			{
				try
				{
					h.Dispose();
				}
				catch
				{
					// Ignore disposal failure
				}
			}
		});

		await host.StartAsync(cancellationToken);
		TestRunner<THost> testRunner = new(host);
		testRunner.DisposableBag.Add(startup);
		testRunner.DisposableBag.Add(host, static async (h, token) => await h.StopAsync(token));
		rollback.Dismiss();
		return testRunner;
	}
}
