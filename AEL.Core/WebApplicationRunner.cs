using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace AEL.Core;

public static class WebApplicationRunner
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
		Action<WebApplicationBuilder>? configureBuilder = null,
		Action<WebApplication>? configureApplication = null,
		params Assembly[] assemblies)
	{
		return HostRunner
			.Run<WebApplication, WebApplicationBuilder>(args, disableJasper,
				strings =>
				{
					WebApplicationBuilder builder = WebApplication.CreateBuilder(strings);
					builder.WebHost.UseKestrel(options => options.AddServerHeader = false);
					configureBuilder?.Invoke(builder);
					return builder;
				},
				webApplicationBuilder =>
				{
					WebApplication webApplication = webApplicationBuilder.Build();
					configureApplication?.Invoke(webApplication);
					return webApplication;
				});
	}
}
