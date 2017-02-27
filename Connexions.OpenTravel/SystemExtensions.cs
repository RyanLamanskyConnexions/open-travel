namespace System
{
	/// <summary>
	/// Extensions to types in the <see cref="System"/> namespace.
	/// </summary>
	public static class SystemExtensions
	{
		/// <summary>
		/// Expresses the date portion of a <see cref="DateTime"/> in ISO8601 format.
		/// </summary>
		/// <param name="value">The value to format.</param>
		/// <returns>The ISO8601-formatted date.</returns>
		public static string ToIso8601Date(this DateTime value)
			=> value.ToString("yyyy-MM-dd", Globalization.CultureInfo.InvariantCulture);

		/// <summary>
		/// Expresses the date portion of a <see cref="DateTime"/> in ISO8601 format, using hours and minutes only.
		/// </summary>
		/// <param name="value">The value to format.</param>
		/// <returns>The ISO8601-formatted date.</returns>
		public static string ToIso8601Time(this DateTime value)
			=> value.ToString("HH:mm", Globalization.CultureInfo.InvariantCulture);
	}
}