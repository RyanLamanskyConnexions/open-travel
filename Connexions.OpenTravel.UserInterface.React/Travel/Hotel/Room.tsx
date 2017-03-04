import * as React from "react";
import * as Session from "../../Session";
import * as HotelApi from "./Api";
import * as Objects from "../../Common/Objects";
import { Convert } from "../../ParsedToReact";

interface IRoomProperties extends Session.ISessionProperty {
	Hotel: HotelApi.IHotel;
	Room: HotelApi.IRoom;
	RatesByRefId: Objects.IStringDictionary<HotelApi.IRate>;
}

export default class Room extends React.Component<IRoomProperties, void> {
	render() {
		const room = this.props.Room;
		const price = this.props.RatesByRefId[room.refId].totalFare;

		return (
			<li key={room.refId}>
				<h4>{room.name}: ${price.toFixed(2)}</h4>
				<div>{Convert(room.SanitizedDescription)}</div>
				<button onClick={() => this.props.Session.Cart.Add({
					Identity: room.refId,
					Category: "Hotel",
					Name: `${room.name} at ${this.props.Hotel.name}`,
					Price: price
				})}>Add to Itinerary</button>
			</li>
		);
	}
}