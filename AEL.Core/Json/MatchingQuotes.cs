﻿namespace AEL.Core.Json;

/// <summary>
/// From https://github.com/thijse/JsonRepairSharp
/// </summary>
internal sealed class MatchingQuotes
{
	private bool _isSingleQuoteLike;
	private bool _isDoubleQuoteLike;
	private bool _isDoubleQuote;

	/// <summary>
	/// Sets the start quote based on the given character code.
	/// </summary>
	/// <param name="code">The code representing the start quote.</param>
	public void SetStartQuote(int code)
	{
		_isSingleQuoteLike = StringUtils.IsSingleQuoteLike(code);
		_isDoubleQuote = StringUtils.IsDoubleQuote(code);
		_isDoubleQuoteLike = StringUtils.IsDoubleQuoteLike(code);
	}

	/// <summary>
	/// Checks if the given character code represents a matching end quote for the start quote.
	/// </summary>
	/// <param name="code">The code representing the end quote.</param>
	/// <returns>True if the code represents a matching end quote, false otherwise.</returns>
	public bool IsMatchingEndQuote(int code)
	{
		return
			_isSingleQuoteLike ? StringUtils.IsSingleQuoteLike(code) :
			_isDoubleQuote ? StringUtils.IsDoubleQuote(code) :
			_isDoubleQuoteLike && StringUtils.IsDoubleQuoteLike(code);
	}
}
