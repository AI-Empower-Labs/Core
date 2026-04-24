using PdfSharp.Pdf;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.IO;

namespace AEL.Core.Docling;

internal static class PdfCleaner
{
	public static async Task<BinaryData> CleanAndProcessPdf(BinaryData binaryData)
	{
		await using Stream s = binaryData.ToStream();
		using PdfDocument pdfDoc = PdfReader.Open(s, PdfDocumentOpenMode.Modify);

		foreach (PdfPage page in pdfDoc.Pages)
		{
			// Iterate through annotations (using index because we might modify the collection)
			for (int i = 0; i < page.Annotations.Count; i++)
			{
				PdfAnnotation annot = page.Annotations[i];

				// 1. Get the Action dictionary (/A)
				PdfDictionary? action = annot.Elements.GetDictionary("/A");
				if (action == null) continue;

				// 2. Check if the Action is a URI action (/S = /URI)
				// In PDFSharp, GetString for a Name usually returns the string with the leading slash
				string subtype = action.Elements.GetString("/S");
				if (subtype != "/URI") continue;

				// 3. Get the actual URI string
				string uri = action.Elements.GetString("/URI");
				if (string.IsNullOrWhiteSpace(uri)) continue;

				string newUri = uri.Trim();
				bool modified = false;

				// Fix: Bare emails (Docling requires 'mailto:')
				if (newUri.Contains('@') && !newUri.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
				{
					newUri = "mailto:" + newUri;
					modified = true;
				}
				// Fix: Bare web links (Docling requires 'http' prefix for 'www' links)
				else if (newUri.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
				{
					newUri = "https://" + newUri;
					modified = true;
				}

				if (modified)
				{
					// Update the URI in the dictionary
					action.Elements.SetString("/URI", newUri);
				}
			}
		}

		using MemoryStream ms = new();
		await pdfDoc.SaveAsync(ms, true);
		return BinaryData.FromBytes(ms.ToArray());
	}
}
