# AEL.Core

Shared .NET libraries for [AI Empower Labs](https://github.com/AI-Empower-Labs) applications. The repo ships two NuGet packages:

| Package | Description |
|---------|-------------|
| **AEL.Core** | Host bootstrap, convention-based DI, background services, extensions, and utilities |
| **AEL.Core.Docling** | Docling API client (Kiota-generated) with helpers for document extraction and chunking |

## Features

### AEL.Core

- **Host runners**: One-line startup for web and console apps with Serilog, OpenTelemetry, and JasperFx
- **Convention-based DI**: Register services via marker interfaces (`IScopedService`, `ITransientService`, `ISingletonService`)
- **Background services**: Async hosted services, cron scheduling, and channel-based batch processing
- **Extensions**: Extension methods for common .NET types (strings, tasks, channels, JSON, etc.)
- **Disposables**: Composable disposable patterns (`DisposableBag`, `AsyncDisposableBase`, etc.)
- **Serialization**: JSON converters and enum handling
- **JsonRepair**: Repair malformed JSON from LLM output
- **Utilities**: Nanoid generation, continuous hashing, temp files, progress streams

### AEL.Core.Docling

- **Document extraction**: Convert PDFs, Office docs, HTML, images, and more to Markdown via Docling
- **Chunking**: Hybrid chunker integration for RAG pipelines
- **PDF preprocessing**: Fixes common PDF link annotations before sending to Docling
- **Zip support**: Expands zip attachments and processes each entry

## Installation

### Submodule

```
git submodule add https://github.com/AI-Empower-Labs/Core
```

Add a project reference to the package(s) you need.

### NuGet

Packages are published to GitHub Packages:

```
dotnet add package AEL.Core --source https://nuget.pkg.github.com/AI-Empower-Labs/index.json
dotnet add package AEL.Core.Docling --source https://nuget.pkg.github.com/AI-Empower-Labs/index.json
```

## Quick Start

### Web application

Starts a web application and wires up DI, logging, and host setup automatically:

```csharp
using System.Reflection;
using AEL.Core;

return await WebApplicationRunner.Run(args, Assembly.GetEntryAssembly()!);
```

### Console application

```csharp
using System.Reflection;
using AEL.Core;

return await ConsoleApplicationRunner.Run(args, Assembly.GetEntryAssembly()!);
```

### Docling extraction

```csharp
using AEL.Core.Docling;
using AEL.Core.Docling.Gamma;

DoclingClient client = new(requestAdapter);
BinaryData pdf = BinaryData.FromBytes(bytes, "application/pdf");

var results = await client.ExtractMarkdown(
    "document.pdf",
    pdf,
    TimeSpan.FromMinutes(5),
    logger,
    configure: null,
    cancellationToken);
```

## Project Structure

```
Core/
├── AEL.Core/
│   ├── Extensions/              # Extension methods for common types
│   ├── Interfaces/              # DI markers, host setup contracts
│   ├── Disposables/             # Resource management utilities
│   ├── Registration/            # Automatic DI and host setup
│   ├── Serialization/           # JSON converters
│   ├── Json/                    # JsonRepair and JSON utilities
│   ├── Stream/                  # Progress stream helpers
│   ├── AsyncBackgroundService.cs
│   ├── AsyncBatchProcessor.cs   # Channel-based batch processing
│   ├── CronExecutionAsyncBackgroundService.cs
│   ├── ConsoleApplicationRunner.cs
│   ├── ContinuousHash.cs
│   ├── HostBuilder.cs / HostRunner.cs
│   ├── NanoIdGenerator.cs       # Nanoid class
│   ├── Startup.cs
│   ├── TempFile.cs
│   └── WebApplicationRunner.cs
├── AEL.Core.Docling/
│   ├── DoclingClientExtensions.cs  # ExtractMarkdown, ExtractAndChunk
│   ├── PdfCleaner.cs
│   ├── DoclingOpenApi.json
│   └── Gamma/                      # Kiota-generated API client
└── AEL.Core.Tests/
```

## Components

### Background Services

- `AsyncBackgroundService`: Base class for long-running async hosted services
- `CronExecutionAsyncBackgroundService`: Cron-scheduled periodic execution
- `AsyncBatchProcessor` / `AsyncBatchProcessor<TIn, TOut>`: Channel-based batch processing

### Host & DI

- `HostRunner` / `WebApplicationRunner` / `ConsoleApplicationRunner`: Application entry points
- `HostBuilder`: Builds hosts with automatic DI registration and host setup
- `TestRunner`: Starts a host for integration testing (optionally without hosted services)
- Marker interfaces drive lifetime registration; `IDependencyInjectionRegistration*` types add custom registration

### Utilities

- **Nanoid** (`NanoIdGenerator.cs`): Generate unique, URL-safe identifiers
- **ContinuousHash**: Incremental hashing
- **TempFile**: Temporary file creation and management
- **JsonRepair**: Repair invalid JSON (e.g. LLM output)
- **OpenTelemetryRegistration** / **LoggingRegistration**: Observability setup

## Requirements

- .NET 10 or later

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

If you encounter any issues or have questions, please [open an issue](https://github.com/AI-Empower-Labs/Core/issues) on GitHub.
