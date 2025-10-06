using Microsoft.AspNetCore.Builder;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Hosting;

public interface IWebApplicationSetup : IHostSetup<WebApplication>;

public interface IWebApplicationSetupAsync : IHostSetupAsync<WebApplication>;
