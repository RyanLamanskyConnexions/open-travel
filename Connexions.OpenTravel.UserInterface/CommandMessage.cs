using Newtonsoft.Json;

namespace Connexions.OpenTravel.UserInterface
{
	class CommandMessage : Message, ICommandMessage
	{
		public bool RanToCompletion { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string ErrorMessage { get; set; }
	}
}