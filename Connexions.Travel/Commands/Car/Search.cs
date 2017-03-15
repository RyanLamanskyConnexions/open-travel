using System;
using System.Collections.Concurrent;
using System.Linq;
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
			var service = session.GetService<Configuration.IServiceResolver>();

			var initializationResponse = await capi.PostAsync<CapiSearchInitResponse>(basePath + "init", new
			{
				currency = this.Currency,
				posId = service.GetServiceForRequest(basePath).PosId,
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
				if (response.FirstPageAvailable = statusResponse.status == "Complete" || statusResponse.resultsCount != response.Count)
				{
					response.Count = statusResponse.resultsCount;
					await session.SendAsync(response);
				}
			} while (response.FirstPageAvailable == false);

			const int pageSize = 10;

			var page = await capi.PostAsync<CapiSearchResultsResponse>(basePath + "results", new
			{
				sessionId = initializationResponse.sessionId,
				currency = this.Currency,
				contentPrefs = new[]
				{
					"all",
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

			if (statusResponse.resultsCount <= pageSize)
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
				.Range(1, statusResponse.resultsCount / fullResultPageSize + (statusResponse.resultsCount % fullResultPageSize != 0 ? 1 : 0))
				.Select(pageNumber => capi.PostAsync<CapiSearchResultsResponse>(basePath + "results", new
				{
					sessionId = initializationResponse.sessionId,
					currency = this.Currency,
					contentPrefs = new[]
					{
						"all",
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
				carRentals = fullResultPages
				.SelectMany(fullResultPage => fullResultPage.carRentals)
				.ToArray(),

				rentalLocations = fullResultPages
				.SelectMany(fullResultPage => fullResultPage.rentalLocations)
				.GroupBy(rentalLocation => rentalLocation.id)
				.Select(group => group.First())
				.ToArray(),

				vehicles = fullResultPages
				.SelectMany(fullResultPage => fullResultPage.vehicles)
				.GroupBy(vehicle => vehicle.refId)
				.Select(group => group.First())
				.ToArray(),

				vendors = fullResultPages
				.SelectMany(fullResultPage => fullResultPage.vendors)
				.GroupBy(vendor => vendor.code)
				.Select(group => group.First())
				.ToArray(),
			};

			searchesBySession[initializationResponse.sessionId] = fullResults;

			response.FullResultsAvailable = true;
			response.RanToCompletion = true;
			await session.SendAsync(response);
		}
	}
}