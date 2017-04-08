namespace Connexions.Travel.Capi.Hotel
{
	class RoomSearchInitRequest : SearchInitRequest
	{
		/// <summary>
		/// Unique Hotel ID that identifies a specific hotel for which you want to retrieve information.
		/// </summary>
		public string hotelId;
	}
}