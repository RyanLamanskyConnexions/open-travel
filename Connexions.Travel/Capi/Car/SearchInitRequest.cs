namespace Connexions.Travel.Capi.Car
{
	class SearchInitRequest
	{
		public string currency;
		public string posId;

		public class Criteria
		{
			public class Pickup
			{
				public string airportCode;
				public string date;
				public string time;
			}

			public Pickup pickup;

			public class DropOff : Pickup
			{
				public bool sameAsPickup;
			}

			public DropOff dropOff;

			public class DriverInfo
			{
				public int age;
				public string nationality;
			}

			public DriverInfo driverInfo;
		}

		public Criteria criteria;
	}
}