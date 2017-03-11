using Newtonsoft.Json;
using System;
using static System.Globalization.CultureInfo;

#pragma warning disable IDE1006 // CAPI naming styles follow a different standard than .NET
#pragma warning disable 649 //Fields are more efficient than properties but the C# compiler doesn't recognize that the JSON serializer writes to them.

namespace Connexions.Travel.Commands.Hotel
{
	/// <summary>
	/// Contains the full results from a search.
	/// </summary>
	class CapiSearchResultsResponse : CapiBaseResponse
	{
		public class Hotel
		{
			public string id;
			public string name;

			public class Contact
			{
				public class Address
				{
					public string line1;
					public string line2;

					public class CodeName
					{
						public string code;
						public string name;
					}

					public CodeName city;
					public CodeName state;

					public string countryCode;
					public string postalCode;
				}

				public Address address;
			}

			public Contact contact;

			public class Image
			{
				public string url;
				public string imageCaption;
				public double? height;
				public double? width;
				public float horizontalResolution;
				public float verticalResolution;

				public override string ToString() => ((FormattableString)$"Image Caption {imageCaption ?? "(null)"} W: {width}, H: {height}").ToString(InvariantCulture);
			}

			public Image[] images;

			public double rating;

			public class Fare
			{
				public string currency;
				public decimal totalFare;
			}

			public Fare fare;

			public class GeoCode
			{
				public double lat;
				public double @long;
			}

			public GeoCode geoCode;
		}

		public Hotel[] hotels;

		public class Paging
		{
			public int totalRecords;
		}

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Paging paging;

		/// <summary>
		/// Removes logging info and organizes/trims data for delivery to a client web browser.
		/// </summary>
		public override void PrepareForClient()
		{
			base.PrepareForClient();

			this.paging = null;

			if (this.hotels != null)
			{
				for (var i = 0; i < this.hotels.Length; i++)
				{
					var hotel = this.hotels[i];
					Array.Sort(hotel.images, ImageComparer.Compare);

					if (hotel.images.Length > 20)
						Array.Resize(ref hotel.images, 20);
				}
			}
			else
			{
				this.hotels = new Hotel[0]; //Allows for simpler client-side code if hotels is never null.
			}
		}
	}
}