import * as React from "react";
import * as Session from "../../Session";
import * as HotelApi from "./Api";
import * as Api from "../Api";
import Room from "./Room";

interface IResult extends Session.ISessionProperty {
	Hotel: HotelApi.IHotel;
}

interface IResultState {
	RoomSearchInProgress: boolean;
	Rooms: HotelApi.ICapiRoomSearchResultsResponse | null;
	RatesByRefId: Api.IStringDictionary<HotelApi.IRate>;
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
			Rooms: null,
			RatesByRefId: {},
		};
	}

	private GetRooms() {
		this.setState({
			RoomSearchInProgress: true,
		});

		this.props.Session.WebSocketCommand({
			"$type": "Connexions.OpenTravel.UserInterface.Commands.Hotel.Rooms, Connexions.OpenTravel.UserInterface",
			Currency: "USD",
			Occupants: [{ Age: 25 }, { Age: 26 }],
			CheckInDate: Api.CreateInitialDate(30),
			CheckOutDate: Api.CreateInitialDate(32),
			HotelId: this.props.Hotel.id,
		}, message => {
			const response = message as IRoomSearchResponse;
			if (response.RanToCompletion) {
				const ratesByRefId: Api.IStringDictionary<HotelApi.IRate> = {};
				for (const rate of response.Results.rates) {
					for (const rateOccupancy of rate.rateOccupancies) {
						ratesByRefId[rateOccupancy.roomRefId] = rate;
					}
				}

				this.setState({
					RoomSearchInProgress: false,
					Rooms: response.Results,
					RatesByRefId: ratesByRefId,
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
				{
					!!this.state.Rooms && !!this.state.Rooms.rooms ?
						this.state.Rooms.rooms.map(room => <Room
							key={room.refId}
							Session={this.props.Session}
							Room={room}
							Hotel={hotel}
							RatesByRefId={this.state.RatesByRefId} />) :
						<div></div>
				}
			</div>
		);
	}
}