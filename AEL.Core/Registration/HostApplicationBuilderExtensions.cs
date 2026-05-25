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
		Log.Logger.Debug("Starting automatic dependency injection for {BuilderType} with {AssemblyCount} assemblies", typeof(THostApplicationBuilder).Name, assemblies.Length);
		foreach (Type type in TypeResolverHelper.GetClassTypes(assemblies))
		{
			if (type.IsBasedOn(typeof(ITransientService)))
			{
				Log.Logger.Debug("Registering transient service {Type}", type.FullName);
				builder.Services.RegisterType(type, ServiceLifetime.Transient);
			}
			else if (type.IsBasedOn(typeof(IScopedService)))
			{
				Log.Logger.Debug("Registering scoped service {Type}", type.FullName);
				builder.Services.RegisterType(type, ServiceLifetime.Scoped);
			}
			else if (type.IsBasedOn(typeof(ISingletonService)))
			{
				Log.Logger.Debug("Registering singleton service {Type}", type.FullName);
				builder.Services.RegisterType(type, ServiceLifetime.Singleton);
			}

			if (type.IsBasedOn(typeof(IDependencyInjectionRegistration<THostApplicationBuilder>)))
			{
				Log.Logger.Debug("Running dependency injection registration {Type}", type.FullName);
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
				Log.Logger.Debug("Running async dependency injection registration {Type}", type.FullName);
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
				else if (valueTaskObject is Task task)
				{
					await task;
				}
				else
				{
					throw new InvalidOperationException(
						$"Type {type.FullName} implements {nameof(IDependencyInjectionRegistrationAsync<>)}<{typeof(THostApplicationBuilder).Name}> " +
						$"but does not declare: public static ValueTask {nameof(IDependencyInjectionRegistrationAsync<>.Register)}({typeof(THostApplicationBuilder).Name} builder).");
				}

				Log.Logger.Debug("Registered {Count} types with {Type}",
					builder.Services.Count - beforeCount,
					type.FullName);
			}
		}

		Log.Logger.Debug("Automatic dependency injection completed for {BuilderType}", typeof(THostApplicationBuilder).Name);
	}
}
