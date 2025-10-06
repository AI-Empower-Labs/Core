// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting;

public interface IHostSetup<in THost> where THost : IHost
{
	abstract static void Setup(THost host);
}
