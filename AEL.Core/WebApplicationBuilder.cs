using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace AEL.Core;

public static class WebAppBuilder
{
	public static WebApplication Build(
		string[] args,
		params Assembly[] assemblies)
	{
		return Build(args, null, null, assemblies);
	}

	public static WebApplication Build(
		string[] args,
		Action<WebApplicationBuilder>? configureBuilder,
		Action<WebApplication>? configureApplication,
		params Assembly[] assemblies)
	{
		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
		builder.AutomaticDependencyInjection(assemblies);
		configureBuilder?.Invoke(builder);

		WebApplication application = builder.Build();
		application.AutomaticWebApplicationSetup(assemblies);
		configureApplication?.Invoke(application);

		return application;
	}
}
