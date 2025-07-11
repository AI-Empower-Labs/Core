// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.Hosting;

public interface IDependencyInjectionRegistration<in TBuilder>
{
	static abstract void Register(TBuilder builder);
}

public interface IDependencyInjectionRegistration<in TBuilder, in T>
{
	static abstract void Register(TBuilder builder, T options);
}
