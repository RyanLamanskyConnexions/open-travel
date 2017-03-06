using System;
using System.Threading.Tasks;

#pragma warning disable 649 //Fields are more efficient than properties but the C# compiler doesn't recognize that the JSON serializer writes to them.

namespace Connexions.Travel.Commands.Car
{
	class Search : Message, ICommand
	{
		public string Currency;

		public DateTime Pickup;

		public DateTime DropOff;

		public string PickupAirport;

		public string DropOffAirport;

		class SearchResponse : CommandMessage
		{
			public string SessionId;
			public int Count;
			public bool FirstPageAvailable;
			public CapiSearchResultsResponse FirstPage;
			public bool FullResultsAvailable;
		}

		class CapiSearchInitResponse : CapiBaseResponse
		{
			/// <summary>
			/// Oski "sessionId" representing the hotel search.
			/// </summary>
			public string sessionId;
		}

		class CapiSearchStatusResponse : CapiStatusResponse
		{
			/// <summary>
			/// Total count of car results so far.
			/// </summary>
			public int resultsCount;
		}

		async Task ICommand.ExecuteAsync(Session session)
		{
			var response = new SearchResponse { Sequence = Sequence };
			const string basePath = "car/v1.0/search/";
			var capi = session.GetService<ICapiClient>();
			var initializationResponse = await capi.PostAsync<CapiSearchInitResponse>(basePath + "init", new
			{
				currency = this.Currency,
				posId = Configuration.Capi.PosId,
				criteria = new
				{
					pickup = new
					{
						airportCode = this.PickupAirport,
						date = this.Pickup.ToIso8601Date(),
						time = this.Pickup.ToIso8601Time(),
					},
					dropOff = new
					{
						sameAsPickup = this.PickupAirport == this.DropOffAirport,
						airportCode = this.PickupAirport == this.DropOffAirport ? null : this.DropOffAirport,
						date = this.DropOff.ToIso8601Date(),
						time = this.DropOff.ToIso8601Time(),
					},
					driverInfo = new
					{
						age = 25,
						nationality = session.Nationality,
					},
				}
			}, session.CancellationToken);

			response.SessionId = initializationResponse.sessionId;
			await session.SendAsync(response);

			CapiSearchStatusResponse statusResponse;
			do
			{
				await Task.Delay(250);
				if (session.CancellationToken.IsCancellationRequested)
					return;

				statusResponse = await capi.PostAsync<CapiSearchStatusResponse>(
					basePath + "status",
					new { sessionId = initializationResponse.sessionId },
					session.CancellationToken);

				//Only send an update if there's a change in status.
				if (response.FirstPageAvailable = statusResponse.status == "Complete" || statusResponse.resultsCount != response.Count)
				{
					response.Count = statusResponse.resultsCount;
					await session.SendAsync(response);
				}
			} while (response.FirstPageAvailable == false);
		}
	}
}