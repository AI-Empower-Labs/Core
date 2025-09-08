// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

public interface IWebApplicationSetupAsync
{
	abstract static ValueTask Setup(WebApplication app, CancellationToken cancellationToken = default);
}
