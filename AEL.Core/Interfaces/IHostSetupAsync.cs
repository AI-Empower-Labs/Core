// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting;

public interface IHostSetupAsync<in THost> where THost : IHost
{
	abstract static ValueTask Setup(THost host, CancellationToken cancellationToken = default);
}
