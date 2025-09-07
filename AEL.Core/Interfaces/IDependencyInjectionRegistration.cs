// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.Hosting;

public interface IDependencyInjectionRegistration<in TBuilder>
{
	abstract static ValueTask Register(TBuilder builder, CancellationToken cancellationToken = default);
}
