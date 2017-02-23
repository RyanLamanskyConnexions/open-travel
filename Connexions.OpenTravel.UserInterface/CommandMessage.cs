using Newtonsoft.Json;

namespace Connexions.OpenTravel.UserInterface
{
	class CommandMessage : Message, ICommandMessage
	{
		/// <summary>
		/// Indicates that the process has completed and the client should not expect any additional messages.
		/// </summary>
		/// <remarks>If an exception is detected during a process, a response is sent on its behalf with this and <see cref="ErrorMessage"/> set.</remarks>
		public bool RanToCompletion { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string ErrorMessage { get; set; }
	}
}