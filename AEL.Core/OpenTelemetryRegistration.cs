using System.Diagnostics;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using OpenTelemetryBuilder = OpenTelemetry.OpenTelemetryBuilder;

namespace AEL.Core;

public static class OpenTelemetryRegistration
{
	public static void Register(IHostApplicationBuilder builder, Action<OpenTelemetryBuilder>? configure = null)
	{
		string? environmentVariable = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
		if (!Uri.TryCreate(environmentVariable, UriKind.Absolute, out Uri? endpointUri))
		{
			Serilog.Log.Warning("OpenTelemetry exporter not configured. Skipping OpenTelemetry registration.");
			return;
		}

		// Allow gRPC over HTTP/2 without TLS when endpoint is http and protocol is gRPC
		if (OtelExporterOtlpProtocol == OtlpExportProtocol.Grpc &&
			string.Equals(endpointUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
		{
			AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
		}

		Activity.DefaultIdFormat = ActivityIdFormat.W3C;
		Activity.ForceDefaultIdFormat = true;

		string? otlpHeaders = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_HEADERS");
		OtlpExportProtocol protocol = OtelExporterOtlpProtocol;
		builder.Services.Configure<OpenTelemetryLoggerOptions>(logging =>
		{
			logging.IncludeFormattedMessage = true;
			logging.IncludeScopes = true;
			logging.ParseStateValues = true;
		});
		OpenTelemetryBuilder openTelemetryBuilder = builder.Services.AddOpenTelemetry();
		openTelemetryBuilder
			.ConfigureResource(resourceBuilder => resourceBuilder
				.AddService(
					serviceName: Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? "AEL",
					serviceVersion: Environment.GetEnvironmentVariable("OTEL_SERVICE_VERSION") ?? "1.0.0",
					serviceInstanceId: Environment.GetEnvironmentVariable("OTEL_SERVICE_INSTANCE_ID") ?? Environment.MachineName)
				.AddEnvironmentVariableDetector())
			.WithLogging(loggerProviderBuilder =>
			{
				loggerProviderBuilder.AddOtlpExporter(options =>
				{
					options.Endpoint = endpointUri;
					options.Protocol = protocol;
					options.Headers = otlpHeaders;
				});
			})
			.WithMetrics(providerBuilder =>
			{
				providerBuilder
					.AddRuntimeInstrumentation()
					.AddHttpClientInstrumentation()
					.AddAspNetCoreInstrumentation()
					.AddOtlpExporter(options =>
					{
						options.Endpoint = endpointUri;
						options.Protocol = protocol;
						options.Headers = otlpHeaders;
					});
			})
			.WithTracing(providerBuilder =>
			{
				providerBuilder
					.AddAspNetCoreInstrumentation(options =>
					{
						options.EnableAspNetCoreSignalRSupport = false;
						options.EnableRazorComponentsSupport = false;
						options.RecordException = true;
					})
					.AddHttpClientInstrumentation()
					.AddOtlpExporter(options =>
					{
						options.Endpoint = endpointUri;
						options.Protocol = protocol;
						options.Headers = otlpHeaders;
					});
			});
		configure?.Invoke(openTelemetryBuilder);
	}

	internal static OtlpExportProtocol OtelExporterOtlpProtocol
	{
		get
		{
			string? otlpProtocolString = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_PROTOCOL");
			if (string.IsNullOrEmpty(otlpProtocolString))
			{
				return OtlpExportProtocol.Grpc;
			}

			if (Extensions.EnumExtensions.TryParseEnumValue(otlpProtocolString, out OtlpExportProtocol otlpProtocol))
			{
				return otlpProtocol;
			}

			return otlpProtocolString.ToLowerInvariant() switch
			{
				"http/json" => OtlpExportProtocol.HttpProtobuf,
				"http/protobuf" => OtlpExportProtocol.HttpProtobuf,
				_ => OtlpExportProtocol.Grpc
			};
		}
	}
}
