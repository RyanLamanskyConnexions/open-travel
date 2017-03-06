using System;
using System.Collections.Generic;
using System.Text;

namespace Connexions.OpenTravel
{
	/// <summary>
	/// Represents a point on the surface of the earth using its latitude and longitude.
	/// </summary>
	public class Geocode
	{
		/// <summary>
		/// Creates a new <see cref="Geocode"/> instance.
		/// </summary>
		public Geocode()
		{
		}

		/// <summary>
		/// Creates a new <see cref="Geocode"/> instance with the provided coordinates.
		/// </summary>
		/// <param name="latitude">The latitude portion of the coordinates.</param>
		/// <param name="longitude">The longitude portion of the coordinates.</param>
		public Geocode(double latitude, double longitude)
		{
			Latitude = latitude;
			Longitude = longitude;
		}

		/// <summary>
		/// The latitude portion of the coordinates.
		/// </summary>
		public double Latitude { get; set; }
		/// <summary>
		/// The longitude portion of the coordinates.
		/// </summary>
		public double Longitude { get; set; }
}
}