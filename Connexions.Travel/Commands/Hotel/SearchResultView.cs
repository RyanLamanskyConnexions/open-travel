using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using static System.Diagnostics.Debug;

#pragma warning disable 649 //Fields are more efficient than properties but the C# compiler doesn't recognize that the JSON serializer writes to them.

namespace Connexions.Travel.Commands.Hotel
{
	using Capi.Hotel;

	/// <summary>
	/// Changes the view of results based on provided input.
	/// </summary>
	class SearchResultView : Message, ICommand, IComparer<SearchResultsResponse.Hotel>
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

		/// <summary>
		/// Options to re-sort the view.
		/// </summary>
		public enum Sort
		{
			/// <summary>
			/// Sort with lowest priced items first.
			/// </summary>
			PriceAscending,
			/// <summary>
			/// Sort with highest priced items first.
			/// </summary>
			PriceDescending,
		}

		/// <summary>
		/// The sequence of desired sorts, in order of preference.  Tie-breakers are used until none are left, resulting in items being considered equal.
		/// </summary>
		public Sort[] Sorts;
#pragma warning restore

		class SearchResultViewResponse : CommandMessage
		{
			public SearchResultViewResponse(SearchResultView view, SearchResultsResponse data)
				: base(view)
			{
				Assert(data != null);
				var hotels = data.hotels;
				Assert(hotels != null);
				this.RanToCompletion = true;

				this.hotels = hotels
					.OrderBy(hotel => hotel, view)
					.Skip(view.ItemsPerPage * view.PageIndex)
					.Take(view.ItemsPerPage)
					.ToArray()
					;
			}

			public SearchResultsResponse.Hotel[] hotels;
		}

		Task ICommand.ExecuteAsync(Session session)
		{
			session.TryGet(typeof(Search), out ConcurrentDictionary<String, SearchResultsResponse> dictionary);
			var data = dictionary[this.SessionId];
			return session.SendAsync(new SearchResultViewResponse(this, data));
		}

		/// <summary>
		/// Compares two hotels using this view's sorting preference.
		/// </summary>
		/// <param name="x">The left side of the comparison.</param>
		/// <param name="y">The right side fo the comparison.</param>
		/// <returns>Less than zero indicating <paramref name="x"/> is less, greater than zero if <paramref name="y"/> is less, otherwise 0.</returns>
		public int Compare(SearchResultsResponse.Hotel x, SearchResultsResponse.Hotel y)
		{
			if (this.Sorts == null)
				return 0;

			foreach (var sort in this.Sorts)
			{
				int result;
				switch (sort)
				{
					default: continue;

					case Sort.PriceAscending:
						result = x.fare.totalFare.CompareTo(y.fare.totalFare);
						break;

					case Sort.PriceDescending:
						result = -x.fare.totalFare.CompareTo(y.fare.totalFare);
						break;
				}

				if (result != 0)
					return result;
			}

			return 0;
		}
	}
}