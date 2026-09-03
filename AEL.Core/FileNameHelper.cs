using System.Buffers;

namespace AEL.Core;

public static class FileNameHelper
{
	private static readonly SearchValues<char> s_invalidFileNameChars =
		SearchValues.Create(Path.GetInvalidFileNameChars());

	public static string SanitizeFileName(string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			return fileName;
		}

		int firstInvalid = fileName.AsSpan().IndexOfAny(s_invalidFileNameChars);
		if (firstInvalid < 0)
		{
			return fileName;
		}

		return string.Create(fileName.Length, (fileName, firstInvalid), static (span, state) =>
		{
			state.fileName.AsSpan().CopyTo(span);
			for (int i = state.firstInvalid; i < span.Length; i++)
			{
				if (s_invalidFileNameChars.Contains(span[i]))
				{
					span[i] = '_';
				}
			}
		});
	}
}
