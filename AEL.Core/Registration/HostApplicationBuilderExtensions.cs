using System.Reflection;

using AEL.Core.Interfaces;
using AEL.Core.Registration;

using Microsoft.Extensions.DependencyInjection;

using Serilog;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting;

public static class HostApplicationBuilderExtensions
{
	public static async ValueTask AutomaticDependencyInjection<THostApplicationBuilder>(this THostApplicationBuilder builder, CancellationToken cancellationToken, params Assembly[] assemblies)
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
				if (methodInfo is null || methodInfo.ReturnType != typeof(void))
				{
					throw new InvalidOperationException(
						$"Type {type.FullName} implements {nameof(IDependencyInjectionRegistration<>)}<{typeof(THostApplicationBuilder).Name}> " +
						$"but does not declare: public static void {nameof(IDependencyInjectionRegistration<>.Register)}({typeof(THostApplicationBuilder).Name} builder).");
				}

				int beforeCount = builder.Services.Count;
				methodInfo.Invoke(null, [builder]);
				Log.Logger.Debug("Registered {Count} types with {Type}",
					builder.Services.Count - beforeCount,
					type.FullName);
			}

			if (type.IsBasedOn(typeof(IDependencyInjectionRegistrationAsync<THostApplicationBuilder>)))
			{
				MethodInfo? methodInfo = type.GetMethod(nameof(IDependencyInjectionRegistrationAsync<>.Register));
				if (methodInfo is null || methodInfo.ReturnType != typeof(ValueTask))
				{
					throw new InvalidOperationException(
						$"Type {type.FullName} implements {nameof(IDependencyInjectionRegistrationAsync<>)}<{typeof(THostApplicationBuilder).Name}> " +
						$"but does not declare: public static ValueTask {nameof(IDependencyInjectionRegistrationAsync<>.Register)}({typeof(THostApplicationBuilder).Name} builder).");
				}

				int beforeCount = builder.Services.Count;
				object? valueTaskObject = methodInfo.Invoke(null, [builder, cancellationToken]);
				if (valueTaskObject is ValueTask valueTask)
				{
					await valueTask;
				}

				Log.Logger.Debug("Registered {Count} types with {Type}",
					builder.Services.Count - beforeCount,
					type.FullName);
			}
		}
	}
}
