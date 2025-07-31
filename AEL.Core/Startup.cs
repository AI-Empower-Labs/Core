using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

using FluentValidation;

using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.SystemConsole.Themes;

namespace AEL.Core;

public sealed class Startup : DisposableBase
{
	public Startup()
	{
		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Information()
			.Enrich.FromLogContext()
			.Enrich.WithExceptionDetails()
			.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
			.WriteTo.OpenTelemetry(_ => {})
			.CreateBootstrapLogger();

		AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromSeconds(1.0));
		Console.OutputEncoding = Encoding.UTF8;
		Log.Logger.Information("Server Starting...");
		DisposableBag.Add(() =>
		{
			Log.Logger.Information("Server Shutting down...");
			Log.CloseAndFlush();
		});

		// Configure FluentValidation to use JSON property names in validation error messages
		// This ensures that validation error messages display the JSON property name (if available)
		// rather than the C# property name, which is useful for API responses
		ValidatorOptions.Global.DisplayNameResolver = (type, member, _) =>
		{
			// If no member is provided (validating the type itself), return the type name
			if (member is null)
			{
				return type.Name;
			}

			// Check if the member has a JsonPropertyNameAttribute (from System.Text.Json)
			JsonPropertyNameAttribute? propertyNameAttribute = member.GetCustomAttribute<JsonPropertyNameAttribute>();
			string? jsonPropertyName = propertyNameAttribute?.Name;

			// Return the JSON property name if available, otherwise fall back to the member name
			return string.IsNullOrEmpty(jsonPropertyName) ? member.Name : jsonPropertyName;
		};
	}
}
