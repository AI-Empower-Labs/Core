// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

public interface IWebApplicationSetup
{
	abstract static void Setup(WebApplication app);
}
