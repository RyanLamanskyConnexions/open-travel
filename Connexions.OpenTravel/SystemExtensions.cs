namespace System
{
	/// <summary>
	/// Extensions to types in the <see cref="System"/> namespace.
	/// </summary>
	public static class SystemExtensions
	{
		/// <summary>
		/// Expresses a date in ISO8601 format.
		/// </summary>
		/// <param name="value">The date to format.</param>
		/// <returns>The ISO8601-formatted date.</returns>
		public static string ToIso8601Date(this DateTime value)
			=> value.ToString("yyyy-MM-dd", Globalization.CultureInfo.InvariantCulture);
    }
}