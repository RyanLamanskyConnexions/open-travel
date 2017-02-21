namespace Connexions.OpenTravel.UserInterface
{
    interface IMessage
	{
		/// <summary>
		/// The sequence number of the command as tracked by the client.
		/// </summary>
		long Sequence { get; }
	}
}