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
		params Assembly[] assemblies)
		where THost : IHost
		where THostApplicationBuilder : IHostApplicationBuilder
	{
		Startup startup = new();
		THost host = await HostBuilder.Build(args, create,
			builder =>
			{
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
			build, null, cancellationToken, assemblies);
		await host.StartAsync(cancellationToken);
		TestRunner<THost> bag = new(host);
		bag.DisposableBag.Add(startup);
		bag.DisposableBag.Add(async () =>
		{
			await host.StopAsync(cancellationToken);
			host.Dispose();
		});
		return bag;
	}
}
