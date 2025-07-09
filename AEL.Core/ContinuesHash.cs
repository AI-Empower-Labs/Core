using System.Security.Cryptography;
using System.Text;

namespace AEL.Core;

public sealed class ContinuesHash : DisposableBase
{
	private readonly MD5 _md5 = MD5.Create();

	public ContinuesHash()
	{
		DisposableBag.Add(_md5);
	}

	public void Add(string textToHash)
	{
		byte[] inputBuffer = Encoding.UTF8.GetBytes(textToHash);
		_md5.TransformBlock(inputBuffer, 0, inputBuffer.Length, inputBuffer, 0);
	}

	public void Add(byte[] bytes)
	{
		_md5.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
	}

	public string ConvertToString()
	{
		_md5.TransformFinalBlock([], 0, 0);
		StringBuilder sb = new();
		foreach (byte b in _md5.Hash ?? [])
		{
			sb.Append(b.ToString("X2"));
		}

		return sb.ToString();
	}
}
