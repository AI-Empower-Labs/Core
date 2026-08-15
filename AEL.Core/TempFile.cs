namespace AEL.Core;

public sealed class TempFile : DisposableBase
{
	public TempFile() : this(Path.GetTempFileName())
	{
	}

	public TempFile(string fileName)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
		FileName = fileName;
		DisposableBag.Add(() =>
		{
			if (File.Exists(FileName))
			{
				File.Delete(FileName);
			}
		});
	}

	public string FileName { get; }

	public FileStream OpenRead() => File.OpenRead(FileName);
	public FileStream OpenWrite() => File.OpenWrite(FileName);
}
