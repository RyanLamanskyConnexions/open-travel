using Newtonsoft.Json;

namespace Connexions.OpenTravel.UserInterface.Html
{
	abstract class Parsed : ISafe
	{
		protected Parsed()
		{
		}

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public abstract bool IsUnsafe { get; }

		public abstract override string ToString();
	}
}