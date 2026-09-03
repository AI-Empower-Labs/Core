using System.Security.Cryptography;
using System.Text;

namespace AEL.Core;

[Obsolete("ContinuousHash is obsolete. Use System.Security.Cryptography.IncrementalHash instead.")]
public sealed class ContinuousHash : DisposableBase
{
	private readonly IncrementalHash _incrementalHash;

	public ContinuousHash() : this(HashAlgorithmName.MD5)
	{
	}

	public ContinuousHash(HashAlgorithmName hashAlgorithm)
	{
		_incrementalHash = IncrementalHash.CreateHash(hashAlgorithm);
		DisposableBag.Add(_incrementalHash);
	}

	public void Add(string textToHash)
	{
		ArgumentNullException.ThrowIfNull(textToHash);
		Add(textToHash.AsSpan());
	}

	public void Add(ReadOnlySpan<char> textToHash)
	{
		int byteCount = Encoding.UTF8.GetByteCount(textToHash);
		Span<byte> buffer = byteCount <= 512 ? stackalloc byte[byteCount] : new byte[byteCount];
		Encoding.UTF8.GetBytes(textToHash, buffer);
		_incrementalHash.AppendData(buffer);
	}

	public void Add(byte[] bytes)
	{
		ArgumentNullException.ThrowIfNull(bytes);
		_incrementalHash.AppendData(bytes);
	}

	public void Add(ReadOnlySpan<byte> bytes)
	{
		_incrementalHash.AppendData(bytes);
	}

	public string ConvertToString()
	{
		byte[] hash = _incrementalHash.GetHashAndReset();
		return Convert.ToHexString(hash);
	}
}
