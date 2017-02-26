using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable IDE1006 // CAPI naming styles follow a different standard than .NET
#pragma warning disable 649 //Fields are more efficient than properties but the C# compiler doesn't recognize that the JSON serializer writes to them.

namespace Connexions.OpenTravel.UserInterface.Commands.Hotel
{
	class Search : Message, ICommand
	{
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

		public Geocode SearchOrigin;

		public double SearchRadiusInKilometers;

		public int MinimumRating;

		class CapiSearchInitResponse : CapiBaseResponse
		{
			/// <summary>
			/// Oski "sessionId" representing the hotel search.
			/// </summary>
			public string sessionId;
		}

		/// <summary>
		/// Hotel property and hotel room search have common properties.
		/// </summary>
		class CapiStatusResponse : CapiBaseResponse
		{
			/// <summary>
			/// A value of "Complete" indicates completion, otherwise the search is still underway.
			/// </summary>
			public string status;

			public class completedSupplier
			{
				public string id;
				public string family;
				public string name;
			}

			public IEnumerable<completedSupplier> completedSuppliers;
		}

		class CapiSearchStatusResponse : CapiStatusResponse
		{
			/// <summary>
			/// Total count of hotel results so far.
			/// </summary>
			public int hotelCount;
		}
#pragma warning restore

		class SearchResponse : CommandMessage
		{
			public string SessionId;
			public int HotelCount;
			public bool FirstPageAvailable;
			public CapiSearchResultsResponse FirstPage;
			public bool FullResultsAvailable;
		}

		async Task ICommand.ExecuteAsync(Session session)
		{
			var response = new SearchResponse { Sequence = Sequence };
			const string basePath = "hotel/v1.0/search/";
			var capi = session.GetService<ICapiClient>();
			var initializationResponse = await capi.PostAsync<CapiSearchInitResponse>(basePath + "init", new
			{
				currency = this.Currency,
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
				bounds = new
				{
					circle = new
					{
						center = new
						{
							lat = this.SearchOrigin.Latitude,
							@long = this.SearchOrigin.Longitude,
						},
						radiusKm = this.SearchRadiusInKilometers,
					}
				},
				filters = new
				{
					minHotelRating = this.MinimumRating,
				},
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
				if (response.FirstPageAvailable = statusResponse.status == "Complete" || statusResponse.hotelCount != response.HotelCount)
				{
					response.HotelCount = statusResponse.hotelCount;
					await session.SendAsync(response);
				}
			} while (response.FirstPageAvailable == false);

			var page = await capi.PostAsync<CapiSearchResultsResponse>(basePath + "results", new
			{
				sessionId = initializationResponse.sessionId,
				currency = this.Currency,
				contentPrefs = new[]
				{
					"basic",
					"images",
					"amenities",
				},
				paging = new
				{
					pageNo = 1,
					pageSize = 10,
					orderBy = "price asc",
				},
			}, session.CancellationToken);

			page.SanitizeForClient();

			response.FirstPage = page;
			await session.SendAsync(response);
			response.FirstPage = null; //Don't need to send this giant object again.

			var searchesBySession = session.GetOrAdd(typeof(Search), type => new ConcurrentDictionary<String, CapiSearchResultsResponse>());
			searchesBySession.Clear(); //Only allowing one to be stored for now until some kind of expiration process is in place.
			searchesBySession[initializationResponse.sessionId] = await capi.PostAsync<CapiSearchResultsResponse>(basePath + "results/all", new
			{
				sessionId = initializationResponse.sessionId,
				currency = this.Currency,
				contentPrefs = new[]
				{
					"basic",
					"images",
					"amenities",
				},
			}, session.CancellationToken);

			response.FullResultsAvailable = true;
			response.RanToCompletion = true;
			await session.SendAsync(response);
		}
	}
}