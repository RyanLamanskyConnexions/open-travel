using Newtonsoft.Json;

namespace Connexions.OpenTravel.UserInterface
{
	/// <summary>
	/// Common features of all command response messages.
	/// </summary>
	interface ICommandMessage : IMessage
	{
		/// <summary>
		/// When true, no further messages to this command will be sent and it should no longer be tracked by the client.
		/// </summary>
		/// <remarks>Long-running processes may send multiple updates with this property set to false.</remarks>
		bool RanToCompletion { get; }

		/// <summary>
		/// When not null, contains details of an error.
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		string ErrorMessage { get; }
	}
}