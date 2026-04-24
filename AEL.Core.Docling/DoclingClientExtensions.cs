using System.IO.Compression;

using AEL.Core.Docling.Gamma;
using AEL.Core.Docling.Gamma.Models;
using AEL.Core.Docling.Gamma.V1.ConvertNamespace.Source;

using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;

namespace AEL.Core.Docling;

public static class DoclingClientExtensions
{
	extension(DoclingClient doclingClient)
	{
		public async Task<string[]> ExtractMarkdown(
			string fileName,
			BinaryData binaryData,
			TimeSpan timeout,
			ILogger logger,
			CancellationToken cancellationToken)
		{
			// Use a linked token with a bounded timeout so a canceled Wolverine
			// message doesn't abort a mid-flight socket read in an unrecoverable way.
			using CancellationTokenSource doclingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			doclingCts.CancelAfter(timeout);

			try
			{
				return await doclingClient.ExtractMarkdownInternal(fileName, binaryData, logger, doclingCts.Token);
			}
			catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
			{
				logger.LogError("Docling extraction timed out for attachment {AttachmentName}", fileName);
			}
			catch (HttpRequestException ex)
			{
				logger.LogError(ex, "Docling extraction failed for attachment {AttachmentName}", fileName);
			}
			catch (IOException ex)
			{
				logger.LogError(ex, "Docling I/O error for attachment {AttachmentName}", fileName);
			}

			return [];
		}

		private async Task<string[]> ExtractMarkdownInternal(
			string fileName,
			BinaryData binaryData,
			ILogger logger,
			CancellationToken cancellationToken)
		{
			if (IsZipFile(binaryData, fileName))
			{
				logger.LogInformation("Extracting zip attachment {AttachmentName}", fileName);

				await using Stream zipStream = binaryData.ToStream();
				await using ZipArchive archive = new(zipStream, ZipArchiveMode.Read);
				List<string> result = [];
				foreach (ZipArchiveEntry entry in archive.Entries)
				{
					if (string.IsNullOrWhiteSpace(entry.Name))
					{
						continue; // directory entry
					}

					await using MemoryStream entryStream = new();
					await using (Stream sourceStream = await entry.OpenAsync(cancellationToken))
					{
						await sourceStream.CopyToAsync(entryStream, cancellationToken);
					}

					byte[] entryBytes = entryStream.ToArray();
					if (entryBytes.Length == 0)
					{
						continue;
					}

					string entryName = string.IsNullOrWhiteSpace(entry.FullName) ? entry.Name : entry.FullName;

					logger.LogInformation(
						"Processing zip entry {EntryName} from {AttachmentName} ({Size} bytes)",
						entryName,
						fileName,
						entryBytes.Length);

					string[] extractedMarkdown = await doclingClient
						.ExtractMarkdownInternal(
							entryName,
							new BinaryData(entryBytes, GetMimeType(entryName)),
							logger,
							cancellationToken);
					result.AddRange(extractedMarkdown);
				}

				return result.ToArray();
			}

			string mediaType = binaryData.MediaType?.Trim().ToLowerInvariant()
				?? throw new ArgumentNullException(nameof(binaryData.MediaType));
			(InputFormat? inputFormat, string? _, bool _) = GetInputFormat(mediaType);
			if (inputFormat is null)
			{
				return [];
			}

			string base64String = Convert.ToBase64String(binaryData);
			SourceRequestBuilder.SourcePostResponse? response = await doclingClient.V1.Convert.Source
				.PostAsync(new ConvertDocumentsRequest
					{
						Options = new ConvertDocumentsRequestOptions
						{
							AbortOnError = false, // keep partial results if one page fails
							DoOcr = true, // key for scans / mixed PDFs
							FromFormats = [inputFormat],
							ToFormats = [OutputFormat.Md],
							Pipeline = ProcessingPipeline.Standard,
							PdfBackend = PdfBackend.Dlparse_v4,
							DoTableStructure = true,
							TableMode = TableFormerMode.Accurate,
							DoChartExtraction = false,
							IncludeImages = true,
							ImageExportMode = ImageRefMode.Placeholder // use Referenced if you prefer external files
						},
						Sources =
						[
							new()
							{
								FileSourceRequest = new FileSourceRequest
								{
									Filename = fileName,
									Base64String = base64String
								}
							}
						],
						Target = new ConvertDocumentsRequest.ConvertDocumentsRequest_target
						{
							InBodyTarget = new InBodyTarget()
						}
					},
					cancellationToken: cancellationToken);

			if (string.IsNullOrEmpty(response?.ConvertDocumentResponse?.Document?.MdContent?.String))
			{
				return [];
			}

			return [response.ConvertDocumentResponse.Document.MdContent.String];
		}

		public async Task<string[]> ExtractAndChunk(
			string fileName,
			BinaryData binaryData,
			TimeSpan timeout,
			ILogger logger,
			CancellationToken cancellationToken)
		{
			// Use a linked token with a bounded timeout so a canceled Wolverine
			// message doesn't abort a mid-flight socket read in an unrecoverable way.
			using CancellationTokenSource doclingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			doclingCts.CancelAfter(timeout);

			try
			{
				return await doclingClient.ExtractAndChunkInternal(fileName, binaryData, logger, doclingCts.Token);
			}
			catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
			{
				logger.LogError("Docling extraction timed out for attachment {AttachmentName}", fileName);
			}
			catch (HttpRequestException ex)
			{
				logger.LogError(ex, "Docling extraction failed for attachment {AttachmentName}", fileName);
			}
			catch (IOException ex)
			{
				logger.LogError(ex, "Docling I/O error for attachment {AttachmentName}", fileName);
			}

			return [];
		}

		internal async Task<string[]> ExtractAndChunkInternal(
			string fileName,
			BinaryData binaryData,
			ILogger logger,
			CancellationToken cancellationToken)
		{
			if (IsZipFile(binaryData, fileName))
			{
				logger.LogInformation("Extracting zip attachment {AttachmentName}", fileName);

				await using Stream zipStream = binaryData.ToStream();
				await using ZipArchive archive = new(zipStream, ZipArchiveMode.Read);
				List<string> result = [];
				foreach (ZipArchiveEntry entry in archive.Entries)
				{
					if (string.IsNullOrWhiteSpace(entry.Name))
					{
						continue; // directory entry
					}

					await using MemoryStream entryStream = new();
					await using (Stream sourceStream = await entry.OpenAsync(cancellationToken))
					{
						await sourceStream.CopyToAsync(entryStream, cancellationToken);
					}

					byte[] entryBytes = entryStream.ToArray();
					if (entryBytes.Length == 0)
					{
						continue;
					}

					string entryName = string.IsNullOrWhiteSpace(entry.FullName) ? entry.Name : entry.FullName;

					logger.LogInformation(
						"Processing zip entry {EntryName} from {AttachmentName} ({Size} bytes)",
						entryName,
						fileName,
						entryBytes.Length);

					string[] extractedMarkdown = await doclingClient
						.ExtractAndChunkInternal(
							entryName,
							new BinaryData(entryBytes, GetMimeType(entryName)),
							logger,
							cancellationToken);
					result.AddRange(extractedMarkdown);
				}

				return result.ToArray();
			}

			string mediaType = binaryData.MediaType?.Trim().ToLowerInvariant()
				?? throw new ArgumentNullException(nameof(binaryData.MediaType));
			(InputFormat? inputFormat, string? extension, bool base64Encode) = GetInputFormat(mediaType);
			if (inputFormat is null)
			{
				return [];
			}

			// No need to chunk small texts
			if (!base64Encode && binaryData.Length <= 800)
			{
				return [binaryData.ToString()];
			}

			if (inputFormat == InputFormat.Pdf)
			{
				binaryData = await PdfCleaner.CleanAndProcessPdf(binaryData);
			}

			string fileContent = Convert.ToBase64String(binaryData);
			ChunkDocumentResponse? response = await doclingClient.V1.Chunk.Hybrid.Source
				.PostAsync(new HybridChunkerOptionsDocumentsRequest
				{
					ConvertOptions = new ConvertDocumentsRequestOptions
					{
						AbortOnError = false, // keep partial results if one page fails
						DoOcr = false, // key for scans / mixed PDFs
						FromFormats = [inputFormat],
						ToFormats = [OutputFormat.Md],
						Pipeline = ProcessingPipeline.Standard,
						PdfBackend = PdfBackend.Docling_parse,
						DoTableStructure = true,
						TableMode = TableFormerMode.Accurate,
						DoChartExtraction = false,
						IncludeImages = false,
						ForceOcr = false,
						DoCodeEnrichment = false,
						DoPictureClassification = false,
						DoFormulaEnrichment = false,
						DoPictureDescription = false,
						ImageExportMode = ImageRefMode.Placeholder // use Referenced if you prefer external files
					},
					Sources =
					[
						new()
						{
							FileSourceRequest = new FileSourceRequest
							{
								Filename = "document." + extension,
								Base64String = fileContent
							}
						}
					],
					Target = new HybridChunkerOptionsDocumentsRequest.HybridChunkerOptionsDocumentsRequest_target
					{
						InBodyTarget = new InBodyTarget()
					}
				}, cancellationToken: cancellationToken);

			return response!.Chunks?
				.Where(item => !string.IsNullOrEmpty(item.Text))
				.Select(item => item.Text!)
				.ToArray() ?? [];
		}
	}

	private static (InputFormat? InputFormat, string? Extension, bool Base64Encode) GetInputFormat(string mediaType)
	{
		return mediaType switch
		{
			// PDF
			"application/pdf" => (InputFormat.Pdf, "pdf", true),

			// DOC
			"application/msword" => (InputFormat.Docx, "docx", true),

			// DOCX
			"application/vnd.openxmlformats-officedocument.wordprocessingml.document" => (InputFormat.Docx, "docx", true),

			// PPTX
			"application/vnd.openxmlformats-officedocument.presentationml.presentation" => (InputFormat.Docx, "pptx", true),

			// HTML
			"text/html" => (InputFormat.Html, "html", false),
			"application/xhtml+xml" => (InputFormat.Html, "html", false),

			// Markdown
			"text/markdown" => (InputFormat.Md, "md", false),
			"text/x-markdown" => (InputFormat.Md, "md", false),
			"text/plain" => (InputFormat.Md, "md", false),

			// AsciiDoc
			"text/asciidoc" => (InputFormat.Asciidoc, "ascii", false),
			"text/x-asciidoc" => (InputFormat.Asciidoc, "ascii", false),

			// CSV
			"text/csv" => (InputFormat.Csv, "csv", false),
			"application/csv" => (InputFormat.Csv, "csv", false),

			// XLSX
			"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => (InputFormat.Xlsx, "Xlsx", true),

			// XML variants
			"application/xml" => (InputFormat.Xml_jats, "xml", false),
			"text/xml" => (InputFormat.Xml_jats, "xml", false),
			"application/jats+xml" => (InputFormat.Xml_jats, "xml", false),

			// METS / GBS
			"application/mets+xml" => (InputFormat.Mets_gbs, "xml", false),

			// Docling JSON
			"application/json" => (InputFormat.Json_docling, "json", false),

			// VTT
			"text/vtt" => (InputFormat.Vtt, "vtt", false),

			// LaTeX
			"application/x-latex" => (InputFormat.Latex, "Latex", false),
			"text/x-tex" => (InputFormat.Latex, "Latex", false),

			// Generic image / audio buckets
			var mt when mt.StartsWith("image/") => (InputFormat.Image, "img", true),
			var mt when mt.StartsWith("audio/") => (InputFormat.Audio, "aud", true),

			_ => (null, null, false)
		};
	}

	private static bool IsZipFile(BinaryData binaryData, string fileName)
	{
		return fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(binaryData.MediaType, "application/zip", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(binaryData.MediaType, "application/x-zip-compressed", StringComparison.OrdinalIgnoreCase);
	}

	private static readonly FileExtensionContentTypeProvider s_provider = new();
	private static string GetMimeType(string fileNameOrPath)
	{
		if (!s_provider.TryGetContentType(fileNameOrPath, out string? contentType))
		{
			contentType = "application/octet-stream"; // safe default
		}

		return contentType;
	}
}
