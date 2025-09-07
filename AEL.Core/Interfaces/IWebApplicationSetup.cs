// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

public interface IWebApplicationSetup
{
	abstract static ValueTask Setup(WebApplication app, CancellationToken cancellationToken = default);
}
