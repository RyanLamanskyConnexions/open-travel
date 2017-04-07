using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable IDE1006 // CAPI naming styles follow a different standard than .NET
#pragma warning disable 649 //Fields are more efficient than properties but the C# compiler doesn't recognize that the JSON serializer writes to them.

namespace Connexions.Travel.Commands.Hotel
{
	class Search : Message, ICommand
	{
		public class CapiSearchRequest
		{
			public string currency;
			public string posId;

			public class RoomOccupancy
			{
				public class Occupant
				{
					public string type;
					public int age;
				}

				public Occupant[] occupants;
			}

			public RoomOccupancy[] roomOccupancies;

			public class StayPeriod
			{
				public string start;
				public string end;
			}

			public StayPeriod stayPeriod;
			public string travellerCountryCodeOfResidence;
			public string travellerNationalityCode;

			public class Bounds
			{
				public class Circle
				{
					public class Center
					{
						public double lat;
						public double @long;
					}

					public Center center;
					public double radiusKm;
				}

				public Circle circle;
			}

			public Bounds bounds;

			public class Filters
			{
				public double minHotelRating;
			}

			public Filters filters;
		}

		public CapiSearchRequest Request;

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
			public int Count;
			public bool FirstPageAvailable;
			public CapiSearchResultsResponse FirstPage;
			public bool FullResultsAvailable;
		}

		async Task ICommand.ExecuteAsync(Session session)
		{
			var response = new SearchResponse { Sequence = Sequence };
			const string basePath = "hotel/v1.0/search/";
			var capi = session.GetService<ICapiClient>();
			var service = session.GetService<Configuration.IServiceResolver>();

			this.Request.posId = service.GetServiceForRequest(basePath).PosId;
			var initializationResponse = await capi.PostAsync<CapiSearchInitResponse>(basePath + "init", this.Request, session.CancellationToken);

			if (initializationResponse.sessionId == null)
			{
				response.RanToCompletion = true;
				response.ErrorMessage = initializationResponse.message ?? "Search initialization failed with no message.";
				await session.SendAsync(response);
				return;
			}

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
				if (response.FirstPageAvailable = statusResponse.status == "Complete" || statusResponse.hotelCount != response.Count)
				{
					response.Count = statusResponse.hotelCount;
					await session.SendAsync(response);
				}
			} while (response.FirstPageAvailable == false);

			const int pageSize = 10;

			var page = await capi.PostAsync<CapiSearchResultsResponse>(basePath + "results", new
			{
				sessionId = initializationResponse.sessionId,
				currency = this.Request.currency,
				contentPrefs = new[]
				{
					"basic",
					"images",
					"amenities",
				},
				paging = new
				{
					pageNo = 1,
					pageSize = pageSize,
					orderBy = "price asc",
				},
			}, session.CancellationToken);

			page.PrepareForClient();

			response.FirstPage = page;

			var searchesBySession = session.GetOrAdd(typeof(Search), type => new ConcurrentDictionary<String, CapiSearchResultsResponse>());
			searchesBySession.Clear(); //Only allowing one to be stored for now until some kind of expiration process is in place.

			if (statusResponse.hotelCount <= pageSize)
			{
				//No need for extra work if the full results fit in the first page, finish now.
				response.FullResultsAvailable = true;
				response.RanToCompletion = true;
				await session.SendAsync(response);
				return;
			}

			await session.SendAsync(response);
			response.FirstPage = null; //Don't need to send this giant object again.

			const int fullResultPageSize = 200;

			var fullResultPages = await Task.WhenAll(Enumerable
				.Range(1, statusResponse.hotelCount / fullResultPageSize + (statusResponse.hotelCount % fullResultPageSize != 0 ? 1 : 0))
				.Select(pageNumber => capi.PostAsync<CapiSearchResultsResponse>(basePath + "results", new
				{
					sessionId = initializationResponse.sessionId,
					currency = this.Request.currency,
					contentPrefs = new[]
					{
						"basic",
						"images",
						"amenities",
					},
					paging = new
					{
						pageNo = pageNumber,
						pageSize = fullResultPageSize,
						orderBy = "price asc",
					},
				}, session.CancellationToken).ContinueWith(task =>
				{
					task.Result.PrepareForClient();
					return task.Result;
				})
				));

			var fullResults = new CapiSearchResultsResponse
			{
				hotels = fullResultPages
				.SelectMany(fullResultPage => fullResultPage.hotels)
				.ToArray(),
			};

			searchesBySession[initializationResponse.sessionId] = fullResults;

			response.FullResultsAvailable = true;
			response.RanToCompletion = true;
			await session.SendAsync(response);
		}
	}
}