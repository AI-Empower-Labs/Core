// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.Hosting;

public interface IDependencyInjectionRegistration<in TBuilder>
{
	static abstract void Register(TBuilder builder);
}
