namespace Estral.Gif.Infra.Extensions;

public static class StringExtensions
{

	/// <summary>
	/// Normalizes a null, empty, or whitespace string into null
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public static string? NormalizeToNull(string? value) =>
		string.IsNullOrWhiteSpace(value) ? null : value;

	public static string StringJoin<T>(this IEnumerable<T> @this, string separator)
	{
		ArgumentNullException.ThrowIfNull(@this);
		ArgumentNullException.ThrowIfNull(separator);

		return string.Join(separator, @this);
	}
}
