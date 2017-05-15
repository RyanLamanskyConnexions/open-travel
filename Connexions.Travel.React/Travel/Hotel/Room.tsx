import * as React from "react";
import * as Session from "../../Session";
import * as HotelApi from "./Api";
import { Convert } from "../../ParsedToReact";
import * as Hotel from "./HotelSearch";

interface IRoomProperties extends Session.ISessionProperty {
	RecommendationId: string;
	SessionId: string;
	Hotel: HotelApi.IHotel;
	Room: HotelApi.IRoom;
	Rate: HotelApi.IRate;
	Category: Hotel.HotelCategory;
}

export default class Room extends React.PureComponent<IRoomProperties, void> {
	render() {
		const room = this.props.Room;
		const rate = this.props.Rate;
		const price = rate.totalFare;

		return (
			<li key={room.refId}>
				<h4>{room.name}: ${price.toFixed(2)}</h4>
				<div>{Convert(room.SanitizedDescription)}</div>
				<button onClick={() => this.props.Category.Add({
					Identity: {
						SessionId: this.props.SessionId,
						HotelId: this.props.Hotel.id,
						RecommendationId: this.props.RecommendationId,
					},
					Category: this.props.Category,
					Name: `${room.name} at ${this.props.Hotel.name}`,
					Price: price,
					Details: {
						SessionId: this.props.SessionId,
						HotelId: this.props.Hotel.id,
						Room: room,
						Rate: rate,
					},
				})}>Add to Itinerary</button>
			</li>
		);
	}
}