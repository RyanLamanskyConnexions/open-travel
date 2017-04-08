namespace Connexions.Travel.Capi.Hotel
{
	class SearchInitRequest
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

		public class Filters
		{
			public double minHotelRating;
		}

		public Filters filters;
	}
}