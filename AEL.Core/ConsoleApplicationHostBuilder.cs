using System.Reflection;

using Microsoft.Extensions.Hosting;

namespace AEL.Core;

public static class ConsoleApplicationHostBuilder
{
	public static Task<IHost> Build(
		string[] args,
		CancellationToken cancellationToken = default,
		params Assembly[] assemblies)
	{
		return Build(args, null, null, cancellationToken, assemblies);
	}

	public static Task<IHost> Build(
		string[] args,
		Action<HostApplicationBuilder>? configureBuilder,
		Action<IHost>? configureApplication,
		CancellationToken cancellationToken = default,
		params Assembly[] assemblies)
	{
		return HostBuilder
			.Build<IHost, HostApplicationBuilder>(
				args,
				strings =>
				{
					HostApplicationBuilder builder = new(strings);
					configureBuilder?.Invoke(builder);
					return builder;
				},
				webApplicationBuilder =>
				{
					IHost webApplication = webApplicationBuilder.Build();
					configureApplication?.Invoke(webApplication);
					return webApplication;
				},
				cancellationToken, assemblies);
	}
}
