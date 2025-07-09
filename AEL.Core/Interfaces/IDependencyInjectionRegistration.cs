// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.Hosting;

public interface IDependencyInjectionRegistration<in TBuilder>
{
	abstract static void Register(TBuilder builder);
}

public interface IDependencyInjectionRegistration<in TBuilder, in T>
{
	abstract static void Register(TBuilder builder, T options);
}
