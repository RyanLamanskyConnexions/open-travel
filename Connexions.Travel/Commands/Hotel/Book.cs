using System.Threading.Tasks;
using static System.Diagnostics.Debug;

#pragma warning disable 649 //Fields are more efficient than properties but the C# compiler doesn't recognize that the JSON serializer writes to them.

namespace Connexions.Travel.Commands.Hotel
{
	class Book : Message, ICommand
	{
		public class Request
		{
			public string sessionId;
			public string hotelId;

			public class Room
			{
				public string roomRefId;
				public string rateRefId;

				public class Guest
				{
					public string type;

					public CapiName name;
					public int age;
				}

				public Guest[] guests;
			}

			public Room[] rooms;

			public class PaymentBreakup
			{
				public string paymentMethodRefId;
				public decimal amount;
				public string currency;
				public string type;
			}

			public PaymentBreakup[] paymentBreakup;

			public class PaymentMethod
			{
				public class Card
				{
					public string refId;
					public string num;
					public string nameOnCard;
					public string cvv;
					public string issuedBy;

					public class Expiry
					{
						public int month;
						public int year;
					}

					public Expiry expiry;

					public CapiContactInfo contactInfo;
				}

				public Card[] cards;
			}

			public PaymentMethod paymentMethod;

			public class Customer
			{
				public CapiName name;
				public CapiContactInfo contactInfo;
				public string dob;
				public string nationality;
				public string customerId;
			}

			public Customer customer;

			public class PrimaryGuest
			{
				public CapiName name;
				public CapiContactInfo contactInfo;
				public int age;

				/// <summary>
				/// Contains the hotel loyalty membership information of the primary guest.
				/// </summary>
				public class HotelLoyalty
				{
					public string chainCode;
					public string num;
				}

				/// <summary>
				/// Contains the hotel loyalty membership information of the primary guest.
				/// </summary>
				public HotelLoyalty hotelLoyalty;
			}

			public PrimaryGuest primaryGuest;
		}

		public Request[] Requests;

		class CapiBookingInitializationResponse
		{
			public string bookingId;
		}

		class CapiBookingStatusResponse : CapiBookingInitializationResponse
		{
			public string bookingProgress;
			public string bookingStatus;
			public string paymentStatus;
		}
#pragma warning restore

		async Task ICommand.ExecuteAsync(Session session)
		{
			var capi = session.GetService<ICapiClient>();
			const string basePath = "hotel/v1.0/hotel/book/";

			foreach (var request in this.Requests)
			{
				var initializationResponse = await capi.PostAsync<CapiBookingInitializationResponse>(
					basePath + "init",
					request,
					session.CancellationToken);

				Assert(initializationResponse != null);

				var statusResponse = await capi.PostAsync<CapiBookingInitializationResponse>(
					basePath + "status",
					initializationResponse,
					session.CancellationToken);

				Assert(statusResponse != null);
			}

			await session.SendAsync(new CommandMessage
			{
				RanToCompletion = true
			});
		}
	}
}