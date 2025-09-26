namespace AEL.Core;

public static class FileNameHelper
{
	public static string SanitizeFileName(string fileName)
	{
		// Sanitize file name for the filesystem
		foreach (char c in Path.GetInvalidFileNameChars())
		{
			fileName = fileName.Replace(c, '_');
		}

		return fileName;
	}
}
