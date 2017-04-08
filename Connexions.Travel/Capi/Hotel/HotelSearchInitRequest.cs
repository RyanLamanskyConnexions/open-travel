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
					/// <summary>
					/// Latitude coordinate of the center point.
					/// </summary>
					public double lat;

					/// <summary>
					/// Longitude coordinate of the center point.
					/// </summary>
					public double @long;
				}

				/// <summary>
				/// Contains the latitude and longitude coordinates of the given point from which the radius is calculated to determine the area to search for hotels.
				/// </summary>
				public Center center;

				/// <summary>
				/// Distance, in kilometers, from the center point that you want to consider in the search area.
				/// </summary>
				public double radiusKm;
			}

			/// <summary>
			/// Contains the geographic coordinates of the center point and the radius within which you want to search for hotels.
			/// </summary>
			public Circle circle;
		}

		/// <summary>
		/// Contains information about the location where the user wants to search for hotels.
		/// </summary>
		public Bounds bounds;
	}
}