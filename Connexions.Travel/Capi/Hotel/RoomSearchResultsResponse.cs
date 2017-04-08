using Newtonsoft.Json;
using System.Linq;

namespace Connexions.Travel.Capi.Hotel
{
	class RoomSearchResultsResponse : BaseResponse
	{
		public class Room
		{
			public string refId;
			public string name;
			public string type;
			public string desc;
			public string code;
			public string roomTypeCode;
			public string smokingIndicator;

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