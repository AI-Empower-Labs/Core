using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

using Serilog;

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
		Log.Logger.Information("Starting web application runner with {AssemblyCount} assemblies", assemblies.Length);
		return HostRunner
			.Run<WebApplication, WebApplicationBuilder>(args, disableJasper,
				strings =>
				{
					Log.Logger.Debug("Creating web application builder");
					WebApplicationBuilder builder = WebApplication.CreateBuilder(strings);
					Log.Logger.Debug("Configuring Kestrel server header");
					builder.WebHost.UseKestrel(options => options.AddServerHeader = false);
					Log.Logger.Debug("Running web application builder configuration");
					configureBuilder?.Invoke(builder);
					Log.Logger.Debug("Web application builder created");
					return builder;
				},
				webApplicationBuilder =>
				{
					Log.Logger.Debug("Building web application");
					WebApplication webApplication = webApplicationBuilder.Build();
					Log.Logger.Debug("Running web application configuration");
					configureApplication?.Invoke(webApplication);
					Log.Logger.Debug("Web application built");
					return webApplication;
				},
				assemblies);
	}
}
