import * as React from "react";
import * as Session from "../../Session";
import * as HotelApi from "./Api";
import * as Common from "../../Common/Objects";
import Room from "./Room";
import * as Hotel from "./HotelSearch"
import Travel from "../Travel";

interface IResult extends Session.ISessionProperty {
	Travel: Travel;
	Hotel: HotelApi.IHotel;
	Category: Hotel.HotelCategory;
}

interface IRecommendedRoom {
	SessionId: string;
	RecommendationId: string;
	Room: HotelApi.IRoom;
	Rate: HotelApi.IRate;
}

interface IResultState {
	RoomSearchInProgress: boolean;
	Rooms: IRecommendedRoom[];
}

interface IRoomSearchResponse extends Session.ICommandMessage {
	SessionId: string;
	Count: number;
	FullResultsAvailable: boolean;
	Results: HotelApi.ICapiRoomSearchResultsResponse;
}

/** Shows the details for a single hotel. */
export default class Result extends React.Component<IResult, IResultState> {
	constructor() {
		super();

		this.state = {
			RoomSearchInProgress: false,
			Rooms: [],
		};
	}

	private GetRooms() {
		this.setState({
			RoomSearchInProgress: true,
		});

		this.props.Session.WebSocketCommand({
			"$type": "Connexions.Travel.Commands.Hotel.Rooms, Connexions.Travel",
			Request: {
				currency: "USD",
				roomOccupancies: [{
					occupants: [{
						type: "adult",
						age: 25,
					}, {
						type: "adult",
						age: 26,
					},
					],
				}],
				stayPeriod: {
					start: this.props.Travel.state.CheckInDate.format("YYYY-MM-DD"),
					end: this.props.Travel.state.CheckOutDate.format("YYYY-MM-DD"),
				},
				travellerCountryCodeOfResidence: "US",
				travellerNationalityCode: "US",
				filters: {
					minHotelRating: 1,
				},
				hotelId: this.props.Hotel.id,
			}
		}, message => {
			const response = message as IRoomSearchResponse;
			if (response.RanToCompletion) {
				const ratesByRefId: Common.IStringDictionary<HotelApi.IRate> = {};
				for (const rate of response.Results.rates) {
					ratesByRefId[rate.refId] = rate;
				}

				const roomsByRefId: Common.IStringDictionary<HotelApi.IRoom> = {};
				for (const room of response.Results.rooms) {
					roomsByRefId[room.refId] = room;
				}

				this.setState({
					RoomSearchInProgress: false,
					Rooms: response.Results.recommendations.map(reccomendation => {
						const rate = ratesByRefId[reccomendation.rateRefIds[0]];
						return {
							SessionId: response.SessionId,
							RecommendationId: reccomendation.id,
							Rate: rate,
							Room: roomsByRefId[rate.rateOccupancies[0].roomRefId],
						}
					}),
				});
			}
		});
	}

	render() {
		const hotel = this.props.Hotel;

		let thumb: JSX.Element;
		if (hotel.images.length > 0) {
			const size1xw = 170, size1xh = 170;
			const url1x = "/ImageProcessor/Resize?m=Fill&w=" + size1xw + "&h=" + size1xh + "&u=" + encodeURIComponent(hotel.images[0].url);
			const srcset =
				"/ImageProcessor/Resize?m=Fill&w=" + (size1xw * 1.5) + "&h=" + (size1xh * 1.5) + "&u=" + encodeURIComponent(hotel.images[0].url) + " 1.5x," +
				"/ImageProcessor/Resize?m=Fill&w=" + (size1xw * 2) + "&h=" + (size1xh * 2) + "&u=" + encodeURIComponent(hotel.images[0].url) + " 2x," +
				"/ImageProcessor/Resize?m=Fill&w=" + (size1xw * 2.5) + "&h=" + (size1xh * 2.5) + "&u=" + encodeURIComponent(hotel.images[0].url) + " 2.5x," +
				"/ImageProcessor/Resize?m=Fill&w=" + (size1xw * 3) + "&h=" + (size1xh * 3) + "&u=" + encodeURIComponent(hotel.images[0].url) + " 3x"
				;
			thumb = (
				<figure className="Thumbnail">
					<img src={url1x} srcSet={srcset} />
				</figure>
			);
		}
		else {
			thumb = <figure className="Thumbnail">No Image</figure>;
		}

		let stars: JSX.Element;
		switch (Math.max(0, Math.min(5, Math.round(hotel.rating)))) {
			default: stars = <span className="Stars"><span className="Clear">☆☆☆☆☆</span></span>; break;
			case 1: stars = <span className="Stars"><span className="Filled">★</span><span className="Clear">☆☆☆☆</span></span>; break;
			case 2: stars = <span className="Stars"><span className="Filled">★★</span><span className="Clear">☆☆☆</span></span>; break;
			case 3: stars = <span className="Stars"><span className="Filled">★★★</span><span className="Clear">☆☆</span></span>; break;
			case 4: stars = <span className="Stars"><span className="Filled">★★★★</span><span className="Clear">☆</span></span>; break;
			case 5: stars = <span className="Stars"><span className="Filled">★★★★★</span></span>; break;
		}

		const geocode = hotel.geoCode.lat.toFixed(4) + "," + hotel.geoCode.long.toFixed(4);
		const mapUrl = "https://www.google.com/maps/place/" + geocode + "/@" + geocode + ",17z/";

		return (
			<div>
				{thumb}
				<span>{stars} {hotel.name}</span>
				<div><a href={mapUrl} target="_blank" rel="noopener noreferrer nofollow">{geocode}</a>; ID# {hotel.id}</div>
				<div>Total: <strong>${hotel.fare.totalFare.toFixed(2)}</strong> {hotel.fare.currency}</div>
				<div>
					<button onClick={() => this.GetRooms()} disabled={this.state.RoomSearchInProgress}>Show Rooms</button>
				</div>
				<ul className="Rooms">
					{
						!!this.state.Rooms ?
							this.state.Rooms.map(reccomendation => <Room
								key={reccomendation.RecommendationId}
								Session={this.props.Session}
								Room={reccomendation.Room}
								Rate={reccomendation.Rate}
								Hotel={hotel}
								RecommendationId={reccomendation.RecommendationId}
								SessionId={reccomendation.SessionId}
								Category={this.props.Category}
								/>) :
							null
					}
				</ul>
			</div>
		);
	}
}