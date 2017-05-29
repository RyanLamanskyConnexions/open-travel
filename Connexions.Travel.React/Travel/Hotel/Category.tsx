import * as React from "react";
import * as Session from "../../Session";
import SessionTemplate from "../../Session";
import * as Category from "../../Commerce/Category";
import * as HotelApi from "./Api";

interface IPriceRoom {
	SessionId: string;
	HotelId: string;
	RecommendationId: string;
}

interface IPriceResponse extends Session.ICommandMessage {
	Room: IPriceRoom;
	Price: HotelApi.IPriceRoomResponse;
}

interface IRoomRate {
	SessionId: string;
	HotelId: string;
	Room: HotelApi.IRoom;
	Rate: HotelApi.IRate;
	Price?: HotelApi.IPriceRoomResponse;
}

export interface IBookingResponse extends Session.ICommandMessage {
	Status: HotelApi.IBookingStatusResponse;
}

export default class HotelCategory extends Category.Category<IRoomRate> {
	constructor(session: SessionTemplate) {
		super(session, "Hotel");
	}

	public PriceCheck(_itemUpdate: (item: Category.Item<IRoomRate>, newPrice: number) => void, done: () => void) {
		this.session.WebSocketCommand({
			"$type": "Connexions.Travel.Commands.Hotel.Price, Connexions.Travel",
			Currency: "USD",
			Rooms: this.Items.map(item => item.Identity)
		}, (message: IPriceResponse) => {
			for (var i = 0; i < this.Items.length; i++) {
				const item = this.Items[i];
				const id = item.Identity as IPriceRoom;

				if (id.HotelId !== message.Room.HotelId)
					continue;
				if (id.SessionId !== message.Room.SessionId)
					continue;
				if (id.RecommendationId !== message.Room.RecommendationId)
					continue;

				item.Details.Price = message.Price;
			}

			if (message.RanToCompletion) {
				done();
			}
		});
	}

	public Book(done: (confirmation: JSX.Element) => void) {
		this.session.WebSocketCommand({
			"$type": "Connexions.Travel.Commands.Hotel.Book, Connexions.Travel",
			Currency: "USD",
			Requests: this.Items.map(item => {
				const details = item.Details;
				const price = details.Price;

				if (!price)
					return;

				return {
					sessionId: details.SessionId,
					hotelId: details.HotelId,
					rooms: [
						{
							roomRefId: price.pricedRooms[0].roomRefId,
							rateRefId: price.pricedRooms[0].rateRefId,
							guests: [
								{
									type: "adult",
									name: {
										title: "mr",
										first: "John",
										middle: "Alex",
										last: "Smith",
										suffix: "Jr",
									},
									age: 25,
								},
								{
									type: "adult",
									name: {
										title: "mrs",
										first: "Jane",
										middle: "Anne",
										last: "Smith",
										suffix: "Jr",
									},
									age: 26,
								},
							],
						},
					],
					paymentBreakup: [
						{
							paymentMethodRefId: "1",
							amount: price.pricedTotalFare,
							currency: "USD",
							type: "card",
						},
					],
					paymentMethod: {
						cards: [
							{
								refId: "1",
								num: "4444" + "3333" + "2222" + "1111",
								nameOnCard: "John Doe",
								cvv: "123",
								issuedBy: "AX",
								expiry: {
									month: 12,
									year: 2020,
								},
								contactInfo: {
									phones: [
										{
											type: "unknown",
											num: "555-0173",
											countryCode: "1",
											ext: "123",
											areaCode: "200",
										},
									],
									billingAddress: {
										line1: "3077 ACME Street",
										line2: "Landmark: Beside the ACME Shopping Mall",
										city: {
											code: "SFO",
											name: "San Francisco",
										},
										state: {
											code: "CA",
											name: "California",
										},
										countryCode: "US",
										postalCode: "94133",
									},
									email: "abc@xyz.com",
								},
							},
						],
					},
					customer: {
						name: {
							title: "mr",
							first: "John",
							middle: "Alex",
							last: "Smith",
							suffix: "Jr",
						},
						contactInfo: {
							phones: [
								{
									type: "unknown",
									num: "555-0173",
									countryCode: "1",
									ext: "123",
									areaCode: "200",
								},
							],
							address: {
								line1: "3077 ACME Street",
								line2: "Landmark: Beside the ACME Shopping Mall",
								city: {
									code: "SFO",
									name: "San Francisco",
								},
								state: {
									code: "CA",
									name: "California",
								},
								countryCode: "US",
								postalCode: "94133",
							},
							email: "abc@xyz.com",
						},
						dob: "1989-12-25",
						nationality: "US",
						customerId: "43435",
					},
					primaryGuest: {
						name: {
							title: "mr",
							first: "John",
							middle: "Alex",
							last: "Smith",
							suffix: "Jr",
						},
						contactInfo: {
							phones: [
								{
									type: "unknown",
									num: "555-0173",
									countryCode: "1",
									ext: "123",
									areaCode: "200",
								},
							],
							address: {
								line1: "3077 ACME Street",
								line2: "Landmark: Beside the ACME Shopping Mall",
								city: {
									code: "SFO",
									name: "San Francisco",
								},
								state: {
									code: "CA",
									name: "California",
								},
								countryCode: "US",
								postalCode: "94133",
							},
							email: "abc@xyz.com",
						},
						age: 25,
						hotelLoyalty: {
							chainCode: "HI",
							num: "123",
						},
					},
				};
			}),
		}, (message: IBookingResponse) => {
			if (message.RanToCompletion) {
				done(<p key={message.Status.bookingId}>{JSON.stringify(message.Status)}</p>);
			}
		});
	}
}