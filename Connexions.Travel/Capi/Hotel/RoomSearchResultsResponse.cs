using Newtonsoft.Json;
using System.Linq;

namespace Connexions.Travel.Capi.Hotel
{
	class RoomSearchResultsResponse : BaseResponse
	{
		public class Room
		{
			/// <summary>
			/// Unique ID for this specific room information. This refId is used as the reference in the <see cref="Rate.rateOccupancies"/> array to identify this specific room.
			/// </summary>
			public string refId;

			/// <summary>
			/// Name of this room.
			/// </summary>
			public string name;

			/// <summary>
			/// Indicates the type of this room.
			/// </summary>
			public string type;

			/// <summary>
			/// Description of this room.
			/// </summary>
			public string desc;

			/// <summary>
			/// Code for this room.
			/// </summary>
			public string code;

			/// <summary>
			/// Code for this room type.
			/// </summary>
			public string roomTypeCode;

			/// <summary>
			/// Indicates the number of rooms available for this room type.
			/// </summary>
			public double availableRoomCount;

			/// <summary>
			/// Indicates the maximum number of guests that can stay in the room.
			/// </summary>
			public double maxOccupancy;

			public class BedDetails
			{
				/// <summary>
				/// Indicates the bed type, such as a king, queen, double, or bunk bed.
				/// </summary>
				public string type;

				/// <summary>
				/// Description of this bed type.
				/// </summary>
				public string desc;

				/// <summary>
				/// Indicates the number of beds available for this bed type.
				/// </summary>
				public double count;
			}

			/// <summary>
			/// Contains the list of information about the beds available in the room.
			/// </summary>
			public BedDetails[] bedDetails;

			/// <summary>
			/// Indicates whether the room is a smoking room or a nonsmoking room.
			/// </summary>
			public string smokingIndicator;

			/// <summary>
			/// The output of safe HTML parsing from the raw <see cref="desc"/> value.
			/// </summary>
			[JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
			public Html.Parsed[] SanitizedDescription;
		}

		public class Rate
		{
			public class Policy
			{
				public string type;
				public string text;
			}

			public class BoardBasis
			{
				public string desc;
				public string type;
			}

			public class Taxes
			{
				public string code;
				public string desc;
				public decimal amount;
			}

			public class Fees
			{
				public string desc;
				public decimal amount;
			}

			public class RateOccupancies
			{
				public string roomRefId;
				public string occupancyRefId;
			}

			public string refId;
			public string desc;
			public bool isPrepaid;
			public string type;
			public string supplierId;
			public string code;
			public string refundability;
			public Policy[] policies;
			public BoardBasis boardBasis;
			public decimal baseFare;
			public Taxes[] taxes;
			public Fees[] fees;
			public decimal discount;
			public decimal totalFare;
			public RateOccupancies[] rateOccupancies;
		}

		public class Recommendation
		{
			public class FareBreakup
			{
				public decimal baseFare;
				public string currency;
				public decimal totalFare;
			}

			public FareBreakup fareBreakup;
			public string id;
			public string[] rateRefIds;
		}

		public Room[] rooms;

		public Rate[] rates;

		public Recommendation[] recommendations;

		public override void PrepareForClient()
		{
			base.PrepareForClient();

			foreach (var room in rooms)
			{
				room.SanitizedDescription = Html.Parser.Parse(room.desc).ToArray();
				room.desc = null;
			}
		}
	}
}