﻿import * as React from "react";
import * as Session from "../../Session";
import SessionTemplate from "../../Session";
import * as HotelApi from "./Api";
import * as Api from "../Api";
import Result from "./Result";
import PageList from "../../Common/PageList";
import * as Category from "../../Commerce/Category";

interface ISearchResponse extends Session.ICommandMessage {
	SessionId: string;
	Count: number;
	FirstPageAvailable: boolean;
	FirstPage: HotelApi.ICapiSearchResultsResponse;
	FullResultsAvailable: boolean;
}

interface ISearchResultViewResponse extends Session.ICommandMessage {
	hotels: HotelApi.IHotel[];
}

interface IProperties extends Session.ISessionProperty {
	Category: HotelCategory;
}

interface ISearchState {
	SearchInProgress: boolean;
	SearchResponse: ISearchResponse;
	SearchTime: number;
	View: HotelApi.ICapiSearchResultsResponse | ISearchResultViewResponse;
	PageIndex: number;
}

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
	Room: HotelApi.IRoom,
	Rate: HotelApi.IRate,
}

export class HotelCategory extends Category.Category<IRoomRate> {
	constructor(session: SessionTemplate) {
		super(session, "Hotel");
	}

	public PriceCheck(_itemUpdate: (item: Category.Item<IRoomRate>, newPrice: number) => void, done: () => void) {
		this.session.WebSocketCommand({
			"$type": "Connexions.OpenTravel.UserInterface.Commands.Hotel.Price, Connexions.OpenTravel.UserInterface",
			Currency: "USD",
			Rooms: this.Items.map(item => item.Identity)
		}, (message: IPriceResponse) => {
			if (message.RanToCompletion) {
				done();
			}
		});
	}

	public Book(done: () => void) {
		this.session.WebSocketCommand({
			"$type": "Connexions.OpenTravel.UserInterface.Commands.Hotel.Book, Connexions.OpenTravel.UserInterface",
			Currency: "USD",
			Requests: this.Items.map(item => {
				const room = item.Details.Room;
				const rate = item.Details.Rate;

				return {
					sessionId: item.Details.SessionId,
					hotelId: item.Details.HotelId,
					rooms: [
						{
							roomRefId: room.refId,
							rateRefId: rate.refId,
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
							amount: rate.totalFare,
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
		}, message => {
			if (message.RanToCompletion) {
				done();
			}
		});
	}
}

export default class HotelSearch extends React.Component<IProperties, ISearchState> {
	private searchStarted: number;

	constructor() {
		super();

		this.state = {
			SearchInProgress: false,
			SearchResponse: HotelSearch.GetBlankSearchResponse(),
			SearchTime: 0,
			View: {},
			PageIndex: 0,
		};
	}

	private static GetBlankSearchResponse(): ISearchResponse {
		return {
			SessionId: "N/A",
			Count: 0,
			FirstPageAvailable: false,
			FirstPage: {},
			FullResultsAvailable: false,
		} as ISearchResponse;
	}

	runSearch() {
		this.searchStarted = performance.now();
		this.setState({
			SearchResponse: HotelSearch.GetBlankSearchResponse(),
			SearchInProgress: true,
			SearchTime: 0,
			View: {},
			PageIndex: 0,
		});

		this.props.Session.WebSocketCommand({
			"$type": "Connexions.OpenTravel.UserInterface.Commands.Hotel.Search, Connexions.OpenTravel.UserInterface",
			Currency: "USD",
			Occupants: [{ Age: 25 }, { Age: 26 }],
			CheckInDate: Api.CreateInitialDate(30),
			CheckOutDate: Api.CreateInitialDate(32),
			SearchOrigin: { Latitude: 36.08, Longitude: -115.152222 },
			SearchRadiusInKilometers: 48.2803,
			MinimumRating: 1,
		}, message => {
			const response = message as ISearchResponse;
			this.setState({
				SearchResponse: response,
				SearchInProgress: !message.RanToCompletion,
			});

			if (this.state.SearchTime === 0 && response.FirstPageAvailable) {
				this.setState({
					SearchTime: performance.now() - this.searchStarted,
				});
			}

			if (!!response.FirstPage) {
				this.setState({
					View: response.FirstPage,
				});
			}
		});
	}

	render() {
		const itemsPerPage = 10;

		const pageChange = (pageIndex: number): void => {
			if (this.state.SearchInProgress)
				return;

			this.setState({
				SearchInProgress: true,
				PageIndex: pageIndex,
			});

			this.props.Session.WebSocketCommand({
				"$type": "Connexions.OpenTravel.UserInterface.Commands.Hotel.SearchResultView, Connexions.OpenTravel.UserInterface",
				SessionId: this.state.SearchResponse.SessionId,
				ItemsPerPage: itemsPerPage,
				PageIndex: pageIndex,
			}, message => {
				const response = message as ISearchResultViewResponse;
				this.setState({
					View: response,
					SearchInProgress: false,
				});
			});
		};

		return (
			<div>
				<h3>Hotel Search</h3>
				<button disabled={this.state.SearchInProgress} onClick={() => this.runSearch()}>Search</button>
				<div>
					<h4>Status</h4>
					<dl>
						<dt>Session ID</dt>
						<dd>{this.state.SearchResponse.SessionId}</dd>
						<dt>Count</dt>
						<dd>{this.state.SearchResponse.Count.toString()}</dd>
						<dt>Is Complete</dt>
						<dd>{
							this.state.SearchResponse.FirstPageAvailable ?
								!!this.state.View ?
									`Yes, in ${(this.state.SearchTime / 1000).toFixed(3)} seconds`
									: "Almost..."
								: "No"
						}{!!this.state.SearchResponse.FullResultsAvailable ? "; full results available." : ""}</dd>
					</dl>
				</div>
				<div>
					<h4>Results</h4>
					<PageList
						Disabled={this.state.SearchInProgress}
						PageCount={this.state.SearchResponse.Count / itemsPerPage}
						PageIndex={this.state.PageIndex}
						ChangePage={pageChange}
					/>
					{
						!!this.state.View && !!this.state.View.hotels ?
							this.state.View.hotels.map(hotel =>
								<Result
									Session={this.props.Session}
									Category={this.props.Category}
									Hotel={hotel}
									key={hotel.id
									} />) :
							null
					}
					<PageList
						Disabled={this.state.SearchInProgress}
						PageCount={this.state.SearchResponse.Count / itemsPerPage}
						PageIndex={this.state.PageIndex}
						ChangePage={pageChange}
					/>
				</div>
			</div>
		);
	}
}