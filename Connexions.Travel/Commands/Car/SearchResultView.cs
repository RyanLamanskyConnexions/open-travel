using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using static System.Diagnostics.Debug;

#pragma warning disable 649 //Fields are more efficient than properties but the C# compiler doesn't recognize that the JSON serializer writes to them.

namespace Connexions.Travel.Commands.Car
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
				var cars = data.carRentals;
				Assert(cars != null);
				this.RanToCompletion = true;

				var rentals = this.carRentals = cars
					.OrderBy(car => car.fare.displayFare.totalFare)
					.Skip(view.ItemsPerPage * view.PageIndex)
					.Take(view.ItemsPerPage)
					.ToArray()
					;

				this.rentalLocations = rentals
					.Select(car => car.pickUpLocationId)
					.Concat(rentals.Select(car => car.dropoffLocationId))
					.Distinct()
					.Join(data.rentalLocations, id => id, location => location.id, (id, location) => location)
					.ToArray()
					;

				this.vehicles = rentals
					.Select(car => car.vehicleRefId)
					.Distinct()
					.Join(data.vehicles, refId => refId, vehicle => vehicle.refId, (id, vehicle) => vehicle)
					.ToArray()
					;

				this.vendors = rentals
					.Select(car => car.vendorCode)
					.Distinct()
					.Join(data.vendors, code => code, vendor => vendor.code, (id, vendor) => vendor)
					.ToArray()
					;
			}

			public CapiSearchResultsResponse.CarRental[] carRentals;
			public CapiSearchResultsResponse.RentalLocation[] rentalLocations;
			public CapiSearchResultsResponse.Vehicle[] vehicles;
			public CapiSearchResultsResponse.Vendor[] vendors;
		}

		Task ICommand.ExecuteAsync(Session session)
		{
			session.TryGet(typeof(Search), out ConcurrentDictionary<String, CapiSearchResultsResponse> dictionary);
			var data = dictionary[this.SessionId];
			return session.SendAsync(new SearchResultViewResponse(this, data));
		}
	}
}