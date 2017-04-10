using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Connexions.Travel.Capi.Hotel
{
	class SearchInitRequest
	{
		/// <summary>
		/// 3-character ISO code that indicates the currency in which you want to retrieve monetary amounts. The API calculates the amount equivalents and returns the amounts in the currency specified.
		/// </summary>
		public string currency;

		/// <summary>
		/// Point of sale (POS) ID. Unique client identifier that allows the booking engine to return the appropriate hotel pricing and inventory.
		/// </summary>
		public string posId;

		public class RoomOccupancy
		{
			public class Occupant
			{
				[JsonConverter(typeof(StringEnumConverter))]
				public enum Type
				{
					adult,
					child
				}

				/// <summary>
				/// Type of guest. Note that there must be at least one adult per room.
				/// </summary>
				public Type type;

				/// <summary>
				/// Age of the guest. This field is required only for a <see cref="Type.child"/> guest and it is optional for an <see cref="Type.adult"/> guest.
				/// </summary>
				public int age;
			}

			/// <summary>
			/// Contains the list of information for each guest in a room. You can make a booking for up to 9 guests in each room.
			/// </summary>
			public Occupant[] occupants;
		}

		/// <summary>
		/// Contains the list of room occupancy objects that contain the <see cref="RoomOccupancy.occupants"/> information for each room. If you perform a multi-room search, the roomOccupancies array will contain multiple objects.
		/// </summary>
		public RoomOccupancy[] roomOccupancies;

		public class StayPeriod
		{
			/// <summary>
			/// Check-in date or the start date of the stay duration. The API supports same-day searches to accommodate different time zones.
			/// </summary>
			public string start;

			/// <summary>
			/// Check-out date or the end date of the stay duration. The end date must be later than the <see cref="start"/> date.
			/// </summary>
			public string end;
		}

		/// <summary>
		/// Contains information about when the guest wants to check in and check out of the hotel. This indicates the complete hotel stay duration.
		/// </summary>
		public StayPeriod stayPeriod;

		/// <summary>
		/// 2-character ISO code that indicates the traveler's country of residence. At times, this information is required by hotels that have a restriction on the guest's country of residence.
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string travellerCountryCodeOfResidence;

		/// <summary>
		/// 2-character ISO code that indicates the traveler's nationality. In recent trends, some suppliers now require the traveler's nationality, which could be for business reasons, such as better pricing based on locals vs tourists.
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string travellerNationalityCode;

		public class Filters
		{
			/// <summary>
			/// Include only hotels whose room rate cost is equal to or more than the rate specified in this field.
			/// </summary>
			[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
			public double minHotelPrice;

			/// <summary>
			/// Include only hotels whose room rate cost is equal to or less than the rate specified in this field.
			/// </summary>
			[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
			public double maxHotelPrice;

			/// <summary>
			/// Include only hotels whose star rating is equal to or more than the star rating specified in this field
			/// </summary>
			[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
			public double minHotelRating;

			/// <summary>
			/// Include only hotels whose star rating is equal to or less than the star rating specified in this field.
			/// </summary>
			[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
			public double maxHotelRating;

			/// <summary>
			/// Indicates that you want the search results to include only those hotels that belong to the list of hotel chains specified.
			/// </summary>
			[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
			public string[] hotelChains;
		}

		/// <summary>
		/// Contains filters to narrow down the hotel search results. The search results will include only those hotels that match all the filter criteria specified.
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Filters filters;

		/// <summary>
		/// If you set this field to true, the search results will include hotels regardless of the availability or room rate information.  False by default.
		/// </summary>
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public bool includeHotelsWithoutRates;
	}
}