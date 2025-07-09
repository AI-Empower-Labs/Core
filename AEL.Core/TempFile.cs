namespace AEL.Core;

public sealed class TempFile : DisposableBase
{
	public TempFile()
	{
		DisposableBag.Add(() =>
		{
			if (File.Exists(FileName))
			{
				File.Delete(FileName);
			}
		});
	}

	public TempFile(string fileName) : this()
	{
		FileName = fileName;
	}

	public string FileName { get; } = Path.GetTempFileName();

	public FileStream OpenRead() => File.OpenRead(FileName);
	public FileStream OpenWrite() => File.OpenWrite(FileName);
}
