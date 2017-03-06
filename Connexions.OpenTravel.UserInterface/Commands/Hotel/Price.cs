using System.Linq;
using System.Threading.Tasks;

namespace Connexions.OpenTravel.UserInterface.Commands.Hotel
{
	class Price : Message, ICommand
	{
		public string Currency;

		public class Room
		{
			public string SessionId;
			public string HotelId;
			public string RecommendationId;
		}

		public Room[] Rooms;

		public class CapiPriceResponse : CapiBaseResponse
		{
			/// <summary>
			/// Unique session identifier for the current session. You can obtain the <see cref="sessionId"/> for your current session from the Room List API's response.
			/// </summary>
			public string sessionId;

			/// <summary>
			/// Indicates whether the rate for the requested room is available from the supplier.
			/// </summary>
			public bool isRateAvailable;

			/// <summary>
			/// Old room rate. This room rate might be outdated. See the <see cref="pricedTotalFare"/> field for the updated (latest) room rates.
			/// </summary>
			public decimal quotedTotalFare;

			/// <summary>
			/// Updated (latest) room rate. The <see cref="pricedTotalFare"/> is the latest room rate whereas the <see cref="quotedTotalFare"/> rate can be an outdated rate.
			/// </summary>
			public decimal pricedTotalFare;

			/// <summary>
			/// Contains the list of information about each priced room. You need to use this information while booking the room by using the <see cref="Book"/> API.
			/// </summary>
			public class PricedRooms
			{
				/// <summary>
				/// rateRefId for the priced room. The rateRefId represents a rate for a valid combination of rooms types and occupancies.
				/// </summary>
				public string rateRefId;

				/// <summary>
				/// Unique reference ID for the occupancy returned. This ID is taken from the occupancies array.
				/// </summary>
				public string occupancyRefId;

				/// <summary>
				/// Unique reference ID corresponding to the priced room. RefId taken from the rooms array.
				/// </summary>
				public string roomRefId;
			}

			/// <summary>
			/// Contains the list of information about each priced room. You need to use this information while booking the room by using the <see cref="Book"/> API.
			/// </summary>
			public PricedRooms[] pricedRooms;
		}

		public class Response : CommandMessage
		{
			public Room Room;

			public CapiPriceResponse Price;
		}

		async Task ICommand.ExecuteAsync(Session session)
		{
			var capi = session.GetService<ICapiClient>();

			var tasks = this.Rooms
				.Select((room, index) => new
				{
					Room = room,
					Task = capi.PostAsync<CapiPriceResponse>("hotel/v1.0/rooms/price", new
					{
						sessionId = room.SessionId,
						hotelId = room.HotelId,
						recommendationId = room.RecommendationId,
					}, session.CancellationToken)
				})
				.ToDictionary(kv => kv.Task, kv => kv.Room)
				;

			while (tasks.Count > 0)
			{
				var task = await Task.WhenAny(tasks.Keys);
				var room = tasks[task];
				tasks.Remove(task);

				var result = task.Result;

				await session.SendAsync(new Response
				{
					Sequence = this.Sequence,
					Room = room,
					Price = result,
					RanToCompletion = tasks.Count == 0,
				});
			}
		}
	}
}