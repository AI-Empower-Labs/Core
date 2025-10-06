using System.Reflection;

using Microsoft.Extensions.Hosting;

namespace AEL.Core;

public static class ConsoleApplicationRunner
{
	public static Task<int> Run(string[] args, bool disableJasper = false)
	{
		return Run(args, disableJasper, null, null, [Assembly.GetEntryAssembly()!]);
	}

	public static Task<int> Run(
		string[] args,
		params Assembly[] assemblies)
	{
		return Run(args, false, null, null, assemblies);
	}

	public static Task<int> Run(
		string[] args,
		bool disableJasper = false,
		params Assembly[] assemblies)
	{
		return Run(args, disableJasper, null, null, assemblies);
	}

	public static Task<int> Run(
		string[] args,
		bool disableJasper = false,
		Action<IHostApplicationBuilder>? configureBuilder = null,
		Action<IHost>? configureApplication = null,
		params Assembly[] assemblies)
	{
		return HostRunner
			.Run<IHost, HostApplicationBuilder>(args, disableJasper,
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
				});
	}
}
