using System.Reflection;

using AEL.Core.Interfaces;
using AEL.Core.Registration;

using Microsoft.Extensions.DependencyInjection;

using Serilog;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting;

public static class HostApplicationBuilderExtensions
{
	public static void AutomaticDependencyInjection<THostApplicationBuilder>(this THostApplicationBuilder builder, params Assembly[] assemblies)
		where THostApplicationBuilder : IHostApplicationBuilder
	{
		builder.AutomaticDependencyInjection(new object(), assemblies);
	}

	public static void AutomaticDependencyInjection<THostApplicationBuilder, T>(this THostApplicationBuilder builder, T options, params Assembly[] assemblies)
		where THostApplicationBuilder : IHostApplicationBuilder
	{
		foreach (Type type in TypeResolverHelper.GetClassTypes(assemblies))
		{
			if (type.IsBasedOn(typeof(ITransientService)))
			{
				builder.Services.RegisterType(type, ServiceLifetime.Transient);
			}

			if (type.IsBasedOn(typeof(IScopedService)))
			{
				builder.Services.RegisterType(type, ServiceLifetime.Scoped);
			}

			if (type.IsBasedOn(typeof(ISingletonService)))
			{
				builder.Services.RegisterType(type, ServiceLifetime.Singleton);
			}

			if (type.IsBasedOn(typeof(IDependencyInjectionRegistration<THostApplicationBuilder>)))
			{
				MethodInfo? methodInfo = type.GetMethod(nameof(IDependencyInjectionRegistration<>.Register));
				int count = builder.Services.Count;
				methodInfo?.Invoke(null, [builder]);
				Log.Logger.Debug("Registered {Count} types with {Type}",
					builder.Services.Count - count,
					type.FullName);
			}

			if (type.IsBasedOn(typeof(IDependencyInjectionRegistration<THostApplicationBuilder, T>)))
			{
				MethodInfo? methodInfo = type.GetMethod(nameof(IDependencyInjectionRegistration<,>.Register));
				int count = builder.Services.Count;
				methodInfo?.Invoke(null, [builder, options]);
				Log.Logger.Debug("Registered {Count} types with {Type}",
					builder.Services.Count - count,
					type.FullName);
			}
		}
	}
}
