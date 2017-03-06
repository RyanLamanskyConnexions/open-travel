using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable IDE1006 // CAPI naming styles follow a different standard than .NET
#pragma warning disable 649 //Fields are more efficient than properties but the C# compiler doesn't recognize that the JSON serializer writes to them.

namespace Connexions.Travel.Commands.Hotel
{
	class Rooms : Message, ICommand
	{
		class CapiSearchStatusResponse : CapiStatusResponse
		{
			/// <summary>
			/// Total count of hotel results so far.
			/// </summary>
			public int roomCount;
		}

		class CapiRoomSearchResultsResponse : CapiBaseResponse
		{
			public class Room
			{
				public string refId;
				public string name;
				public string type;
				public string desc;
				public string code;
				public string roomTypeCode;
				public string smokingIndicator;

				[JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
				public Html.Parsed[] SanitizedDescription;
			}

			public class Rate
			{
				public class Policy
				{
					public string type;
					public string text;
				}

				public class BoardBasis
				{
					public string desc;
					public string type;
				}

				public class Taxes
				{
					public string code;
					public string desc;
					public decimal amount;
				}

				public class Fees
				{
					public string desc;
					public decimal amount;
				}

				public class RateOccupancies
				{
					public string roomRefId;
					public string occupancyRefId;
				}

				public string refId;
				public string desc;
				public bool isPrepaid;
				public string type;
				public string supplierId;
				public string code;
				public string refundability;
				public Policy[] policies;
				public BoardBasis boardBasis;
				public decimal baseFare;
				public Taxes[] taxes;
				public Fees[] fees;
				public decimal discount;
				public decimal totalFare;
				public RateOccupancies[] rateOccupancies;
			}

			public class Recommendation
			{
				public class FareBreakup
				{
					public decimal baseFare;
					public string currency;
					public decimal totalFare;
				}

				public FareBreakup fareBreakup;
				public string id;
				public string[] rateRefIds;
			}

			public Room[] rooms;

			public Rate[] rates;

			public Recommendation[] recommendations;

			public override void PrepareForClient()
			{
				base.PrepareForClient();

				foreach (var room in rooms)
				{
					room.SanitizedDescription = Html.Parser.Parse(room.desc).ToArray();
					room.desc = null;
				}
			}
		}
#pragma warning restore

		public string Currency;

		public class Occupant
		{
			/// <summary>
			/// The age of the occupant in years.
			/// </summary>
			public int Age;
		}

		public Occupant[] Occupants;

		public DateTime CheckInDate;

		public DateTime CheckOutDate;

		public string HotelId;

		class SearchResponse : CommandMessage
		{
			public string SessionId;
			public int Count;
			public bool FullResultsAvailable;
			public CapiRoomSearchResultsResponse Results;
		}

		async Task ICommand.ExecuteAsync(Session session)
		{
			var response = new SearchResponse { Sequence = Sequence };
			const string basePath = "hotel/v1.0/";
			var capi = session.GetService<ICapiClient>();

			var initializationResponse = await capi.PostAsync<CapiSearchInitResponse>(basePath + "rooms/search/init/stateless", new
			{
				currency = Currency,
				posId = Configuration.Capi.PosId,
				roomOccupancies = new[]
				{
					new
					{
						occupants = this
						.Occupants
						.Select(o => new { type = o.Age > 18 ? "adult" : "child", age = o.Age })
						.ToArray()
					},
				},
				stayPeriod = new
				{
					start = this.CheckInDate.ToIso8601Date(),
					end = this.CheckOutDate.ToIso8601Date(),
				},
				travellerCountryCodeOfResidence = session.CountryOfResidence,
				travellerNationalityCode = session.Nationality,
				hotelId = this.HotelId,
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
					basePath + "rooms/search/status",
					new
					{
						sessionId = initializationResponse.sessionId,
						hotelId = this.HotelId,
					},
					session.CancellationToken);

				//Only send an update if there's a change in status.
				if (response.FullResultsAvailable = statusResponse.status == "Complete" || statusResponse.roomCount != response.Count)
				{
					response.Count = statusResponse.roomCount;
					await session.SendAsync(response);
				}
			} while (statusResponse.status != "Complete");

			response.Results = await capi.PostAsync<CapiRoomSearchResultsResponse>(basePath + "rooms/search/results", new
			{
				sessionId = initializationResponse.sessionId,
				hotelId = this.HotelId,
			}, session.CancellationToken);

			response.Results.PrepareForClient();

			response.RanToCompletion = true;
			await session.SendAsync(response);
		}
	}
}