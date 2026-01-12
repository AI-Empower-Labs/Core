using System.Reflection;

using Microsoft.Extensions.Hosting;

namespace AEL.Core;

public static class HostTestRunner
{
	public static Task<TestRunner<IHost>> Start(
		string[] args,
		Action<IHostApplicationBuilder>? configureBuilder = null,
		Action<IHost>? configureApplication = null,
		bool startHostedServices = false,
		CancellationToken cancellationToken = default,
		params Assembly[] assemblies)
	{
		return TestRunner.Start<IHost, HostApplicationBuilder>(
			args,
			strings =>
			{
				HostApplicationBuilder builder = new(strings);
				configureBuilder?.Invoke(builder);
				return builder;
			},
			hostApplicationBuilder =>
			{
				IHost host = hostApplicationBuilder.Build();
				configureApplication?.Invoke(host);
				return host;
			},
			startHostedServices,
			cancellationToken,
			assemblies);
	}
}
