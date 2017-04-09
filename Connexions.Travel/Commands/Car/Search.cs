using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Connexions.Travel.Commands.Car
{
	using Capi.Car;

	class Search : Message, ICommand
	{
		public SearchInitRequest Request;

		class SearchResponse : CommandMessage
		{
			public string SessionId;
			public int Count;
			public bool FirstPageAvailable;
			public SearchResultsResponse FirstPage;
			public bool FullResultsAvailable;
		}

		async Task ICommand.ExecuteAsync(Session session)
		{
			var response = new SearchResponse { Sequence = Sequence };
			const string basePath = "car/v1.0/search/";
			var capi = session.GetService<ICapiClient>();
			var service = session.GetService<Configuration.IServiceResolver>();

			this.Request.posId = service.GetServiceForRequest(basePath).PosId;
			var initializationResponse = await capi.PostAsync<SearchInitResponse>(basePath + "init", this.Request, session.CancellationToken);

			if (initializationResponse.sessionId == null)
			{
				response.RanToCompletion = true;
				response.ErrorMessage = initializationResponse.message ?? "Search initialization failed with no message.";
				await session.SendAsync(response);
				return;
			}

			response.SessionId = initializationResponse.sessionId;
			await session.SendAsync(response);

			SearchStatusResponse statusResponse;
			do
			{
				await Task.Delay(250);

				statusResponse = await capi.PostAsync<SearchStatusResponse>(
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

			var page = await capi.PostAsync<SearchResultsResponse>(basePath + "results", new
			{
				sessionId = initializationResponse.sessionId,
				currency = this.Request.currency,
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

			var searchesBySession = session.GetOrAdd(typeof(Search), type => new ConcurrentDictionary<String, SearchResultsResponse>());
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
				.Select(pageNumber => capi.PostAsync<SearchResultsResponse>(basePath + "results", new
				{
					sessionId = initializationResponse.sessionId,
					currency = this.Request.currency,
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

			var fullResults = new SearchResultsResponse
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