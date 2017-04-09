using System.Threading.Tasks;

namespace Connexions.Travel.Commands.Hotel
{
	using Capi.Hotel;

	class Rooms : Message, ICommand
	{
		public RoomSearchInitRequest Request;

		class SearchResponse : CommandMessage
		{
			public string SessionId;
			public int Count;
			public bool FullResultsAvailable;
			public RoomSearchResultsResponse Results;
		}

		async Task ICommand.ExecuteAsync(Session session)
		{
			var response = new SearchResponse { Sequence = Sequence };
			const string basePath = "hotel/v1.0/";
			var capi = session.GetService<ICapiClient>();
			var service = session.GetService<Configuration.IServiceResolver>();

			this.Request.posId = service.GetServiceForRequest(basePath).PosId;
			var initializationResponse = await capi.PostAsync<SearchInitResponse>(basePath + "rooms/search/init/stateless", this.Request, session.CancellationToken);

			response.SessionId = initializationResponse.sessionId;
			await session.SendAsync(response);

			RoomSearchStatusResponse statusResponse;
			do
			{
				await Task.Delay(250);

				statusResponse = await capi.PostAsync<RoomSearchStatusResponse>(
					basePath + "rooms/search/status",
					new
					{
						sessionId = initializationResponse.sessionId,
						hotelId = this.Request.hotelId,
					},
					session.CancellationToken);

				//Only send an update if there's a change in status.
				if (response.FullResultsAvailable = statusResponse.status == "Complete" || statusResponse.roomCount != response.Count)
				{
					response.Count = statusResponse.roomCount;
					await session.SendAsync(response);
				}
			} while (statusResponse.status != "Complete");

			response.Results = await capi.PostAsync<RoomSearchResultsResponse>(basePath + "rooms/search/results", new
			{
				sessionId = initializationResponse.sessionId,
				hotelId = this.Request.hotelId,
			}, session.CancellationToken);

			response.Results.PrepareForClient();

			response.RanToCompletion = true;
			await session.SendAsync(response);
		}
	}
}