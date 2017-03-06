using Newtonsoft.Json;

namespace Connexions.Travel
{
	/// <summary>
	/// Contains responses to request <see cref="Message"/>s.
	/// </summary>
	class CommandMessage : Message, ICommandMessage
	{
		/// <summary>
		/// Creates a new <see cref="CommandMessage"/> instance.
		/// </summary>
		public CommandMessage()
		{
		}

		/// <summary>
		/// Creates a new <see cref="CommandMessage"/> instance with the provided source data.
		/// </summary>
		/// <param name="message">The message containing information to carry into this response.</param>
		public CommandMessage(Message message)
		{
			this.Sequence = message.Sequence;
		}

		/// <summary>
		/// Indicates that the process has completed and the client should not expect any additional messages.
		/// </summary>
		/// <remarks>If an exception is detected during a process, a response is sent on its behalf with this and <see cref="ErrorMessage"/> set.</remarks>
		public bool RanToCompletion { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string ErrorMessage { get; set; }
	}
}