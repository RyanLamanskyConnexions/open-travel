namespace Connexions.Travel.Capi.Hotel
{
    class HotelSearchInitRequest : SearchInitRequest
	{
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
	}
}
