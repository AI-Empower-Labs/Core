// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

public interface IWebApplicationSetup
{
	static abstract void Setup(WebApplication app);
}
