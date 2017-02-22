using System.Collections.Generic;
using System.Linq;

namespace Connexions.OpenTravel.UserInterface.StaticData
{
	/// <summary>
	/// Encapsulates basic information about an airport.
	/// </summary>
	sealed class Airport
	{
		/// <summary>
		/// 3-letter code assigned by the International Air Transport Association.
		/// </summary>
		public string IataCode { get; }

		/// <summary>
		/// The English-language name of the airport.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The latitude portion of thie airport's coordinates.
		/// </summary>
		public double Latitude { get; }

		/// <summary>
		/// The longitude portion of thie airport's coordinates.
		/// </summary>
		public double Longitude { get; }

		/// <summary>
		/// Creates a new <see cref="Airport"/> with the provided parameters.
		/// </summary>
		/// <param name="iataCode">3-letter code assigned by the International Air Transport Association.</param>
		/// <param name="name">The English-language name of the airport.</param>
		/// <param name="latitude">The latitude portion of thie airport's coordinates.</param>
		/// <param name="longitude">The longitude portion of thie airport's coordinates.</param>
		public Airport(string iataCode, string name, double latitude, double longitude)
		{
			IataCode = iataCode;
			Name = name;
			Latitude = latitude;
			Longitude = longitude;
		}

		/// <summary>
		/// A pre-defined list of notable airports.
		/// </summary>
		public static readonly Airport[] All = new[]
		{
			new Airport("CDG", "Paris Charles de Gaulle Airport", 49.009722, 2.547778),
			new Airport("PEK", "Beijing Capital International Airport", 40.0725, 116.5975),
			new Airport("ATL", "Hartsfield–Jackson Atlanta International Airport", 33.636667, -84.428056),
			new Airport("LAX", "Los Angeles International Airport", 33.9425, -118.408056),
			new Airport("ORD", "O'Hare International Airport", 41.978611, -87.904722),
			new Airport("DFW", "Dallas/Fort Worth International Airport", 32.896944, -97.038056),
			new Airport("JFK", "John F. Kennedy International Airport", 40.639722, -73.778889),
			new Airport("DEN", "Denver International Airport", 39.861667, -104.673056),
			new Airport("SFO", "San Francisco International Airport", 37.618889,- 122.375),
			new Airport("CLT", "Charlotte Douglas International Airport", 35.213889, -80.943056),
			new Airport("LAS", "McCarran International Airport", 36.08, -115.152222),
			new Airport("PHX", "Phoenix Sky Harbor International Airport", 33.434167, -112.011667),
		};

		/// <summary>
		/// The entries of <see cref="All"/> keyed by the <see cref="IataCode"/>.
		/// </summary>
		public static readonly Dictionary<string, Airport> ByIataCode = All.ToDictionary(airport => airport.IataCode);
	}
}