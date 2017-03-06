using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using static System.Diagnostics.Debug;

#pragma warning disable 649 //Fields are more efficient than properties but the C# compiler doesn't recognize that the JSON serializer writes to them.

namespace Connexions.Travel.Commands.Hotel
{
	/// <summary>
	/// Changes the view of results based on provided input.
	/// </summary>
	class SearchResultView : Message, ICommand
	{
		/// <summary>
		/// The session ID that identifies the full result set to use.
		/// </summary>
		public string SessionId;

		/// <summary>
		/// The number of items to show on each page.
		/// </summary>
		public int ItemsPerPage;

		/// <summary>
		/// The 0-based page index.
		/// </summary>
		public int PageIndex;
#pragma warning restore

		class SearchResultViewResponse : CommandMessage
		{
			public SearchResultViewResponse(SearchResultView view, CapiSearchResultsResponse data)
				: base(view)
			{
				Assert(data != null);
				var hotels = data.hotels;
				Assert(hotels != null);
				this.RanToCompletion = true;

				this.hotels = hotels
					.OrderBy(hotel => hotel.fare.totalFare)
					.Skip(view.ItemsPerPage * view.PageIndex)
					.Take(view.ItemsPerPage)
					.ToArray()
					;
			}

			public CapiSearchResultsResponse.Hotel[] hotels;
		}

		Task ICommand.ExecuteAsync(Session session)
		{
			session.TryGet(typeof(Search), out ConcurrentDictionary<String, CapiSearchResultsResponse> dictionary);
			var data = dictionary[this.SessionId];
			return session.SendAsync(new SearchResultViewResponse(this, data));
		}
	}
}