using System.Collections;

using AEL.Core.Interfaces;

using FluentValidation;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceRegistrationExtensions
{
	public static void RegisterType<T>(this IServiceCollection services, ServiceLifetime serviceLifetime)
	{
		services.RegisterType(typeof(T), serviceLifetime);
	}

	public static void RegisterType(this IServiceCollection services, Type serviceType, ServiceLifetime serviceLifetime)
	{
		services.Add(new ServiceDescriptor(serviceType, serviceType, serviceLifetime));
		foreach (Type @interface in TypesToRegister(serviceType))
		{
			services.Add(new ServiceDescriptor(@interface, sp => sp.GetRequiredService(serviceType), serviceLifetime));
		}
	}

	public static void RegisterType<T>(this IServiceCollection services,
		Func<IServiceProvider, T> factory,
		ServiceLifetime serviceLifetime) where T : notnull
	{
		services.Add(new ServiceDescriptor(typeof(T), null, (sp, _) => factory(sp), serviceLifetime));
		foreach (Type @interface in TypesToRegister(typeof(T)))
		{
			services.Add(new ServiceDescriptor(@interface, sp => sp.GetRequiredService<T>(), serviceLifetime));
		}
	}

	private static Type[] TypesToRegister(Type type)
	{
		return type
			.GetInterfaces()
			.Where(t => t != typeof(IScopedService)
				&& t != typeof(ITransientService)
				&& t != typeof(ISingletonService)
				&& t != typeof(IValidator)
				&& t != typeof(IDisposable)
				&& t != typeof(IAsyncDisposable)
				&& !t.IsBasedOn(typeof(IEnumerable))
				&& !t.IsBasedOn(typeof(IAsyncEnumerable<>)))
			.ToArray();
	}
}
