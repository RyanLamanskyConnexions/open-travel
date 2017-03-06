using System.Threading.Tasks;

namespace Connexions.Travel
{
	/// <summary>
	/// Descibes the common features of all user interface commands.
	/// </summary>
	interface ICommand : IMessage
	{
		/// <summary>
		/// Processses the command using the provided data.
		/// </summary>
		/// <param name="session">Manages the active session.</param>
		Task ExecuteAsync(Session session);
	}
}