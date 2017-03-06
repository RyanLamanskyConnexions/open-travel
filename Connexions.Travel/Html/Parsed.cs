using Newtonsoft.Json;

namespace Connexions.Travel.Html
{
	/// <summary>
	/// A server-side-sanitized HTML component.
	/// </summary>
	abstract class Parsed : ISafe
	{
		/// <summary>
		/// Creates a new <see cref="Parsed"/> instance.
		/// </summary>
		protected Parsed()
		{
		}

		/// <summary>
		/// When true, the item may cause undesirable side effects if sent to a web browser.
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public abstract bool IsUnsafe { get; }

		/// <summary>
		/// Provides a string representation of the item.
		/// </summary>
		/// <returns>The string representation.</returns>
		public abstract override string ToString();
	}
}