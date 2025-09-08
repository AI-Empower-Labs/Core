// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.Hosting;

public interface IDependencyInjectionRegistration<in TBuilder>
{
	abstract static void Register(TBuilder builder);
}
