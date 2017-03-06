using System.Threading.Tasks;

namespace Connexions.Travel.Commands
{
	class Authorize : Message, ICommand
	{
		class Response : CommandMessage
		{
			/// <summary>
			/// Returning a predetermined list airports as part of the authorization response is a placeholder until an autocomplete source is available.
			/// </summary>
			[Newtonsoft.Json.JsonProperty]
			StaticData.Airport[] KnownAirports => StaticData.Airport.All;
		}

		async Task ICommand.ExecuteAsync(Session session)
		{
			session.Nationality = "US";
			session.CountryOfResidence = "US";

			await session.SendAsync(new Response
			{
				Sequence = Sequence,
				RanToCompletion = true,
			});
		}
	}
}