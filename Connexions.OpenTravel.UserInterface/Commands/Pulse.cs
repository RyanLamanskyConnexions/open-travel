using System.Threading.Tasks;

namespace Connexions.OpenTravel.UserInterface.Commands
{
	class Pulse : ICommand
	{
		public long Sequence { get; set; }

		private sealed class PulseResponse : ICommandMessage
		{
			public long Sequence { get; set; }

			public bool RanToCompletion { get; set; }
		}

		public async Task ExecuteAsync(Session session)
		{
			await session.SendAsync(new PulseResponse { Sequence = Sequence });
			await Task.Delay(3000);
			await session.SendAsync(new PulseResponse { Sequence = Sequence, RanToCompletion = true });
		}
	}
}