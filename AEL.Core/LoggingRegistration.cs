using System.Text.Json;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using OpenTelemetry.Exporter;

using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.OpenTelemetry;
using Serilog.Sinks.SystemConsole.Themes;

namespace AEL.Core;

public static class LoggingRegistration
{
	public static void Register(IHostApplicationBuilder builder,
		Action<LoggerConfiguration>? configure = null)
	{
		builder.Logging.ClearProviders();
		builder.Services.AddSerilog((sp, configuration) =>
		{
			IConfiguration innerConfiguration;
			string? environmentConfiguration = Environment.GetEnvironmentVariable("SERILOG");
			if (!string.IsNullOrWhiteSpace(environmentConfiguration))
			{
				var logConfigObject = new
				{
					Serilog = new
					{
						MinimumLevel = JsonSerializer.Deserialize<object>(environmentConfiguration)
					}
				};
				byte[] jsonBytes = JsonSerializer.SerializeToUtf8Bytes(logConfigObject);
				using MemoryStream stream = new(jsonBytes);
				innerConfiguration = new ConfigurationBuilder()
					.AddJsonStream(stream)
					.Build();
			}
			else
			{
				innerConfiguration = sp.GetRequiredService<IConfiguration>();
			}

			LoggerConfiguration loggerConfiguration = configuration
				.ReadFrom.Configuration(innerConfiguration)
				.MinimumLevel.Override("Microsoft.AspNetCore.DataProtection", LogEventLevel.Warning)
				.Enrich.FromLogContext()
				.Enrich.WithExceptionDetails()
				.Enrich.WithSpan()
				.WriteTo.Console(
					restrictedToMinimumLevel: Extensions.EnumExtensions.ParseEnumValue(
						innerConfiguration["Serilog:MinimumLevel:Default"] ?? innerConfiguration["Serilog:Default"] ?? "Warning", LogEventLevel.Warning),
					outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
					theme: AnsiConsoleTheme.Code);

			string? environmentVariable = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
			if (Uri.TryCreate(environmentVariable, UriKind.Absolute, out Uri? endpointUri))
			{
				loggerConfiguration.WriteTo.OpenTelemetry(
					endpoint: environmentVariable,
					protocol: OpenTelemetryRegistration.OtelExporterOtlpProtocol switch
					{
						OtlpExportProtocol.Grpc => OtlpProtocol.Grpc,
						_ => OtlpProtocol.HttpProtobuf
					},
					headers: ParseOtlpHeaders(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_HEADERS")));
			}

			configure?.Invoke(configuration);
		});
	}

	private static Dictionary<string, string>? ParseOtlpHeaders(string? headers)
	{
		if (string.IsNullOrWhiteSpace(headers))
		{
			return null;
		}

		return headers
			.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Select(header => header.Split('=', 2, StringSplitOptions.TrimEntries))
			.Where(parts => parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[0]))
			.ToDictionary(parts => parts[0], parts => parts[1]);
	}
}
