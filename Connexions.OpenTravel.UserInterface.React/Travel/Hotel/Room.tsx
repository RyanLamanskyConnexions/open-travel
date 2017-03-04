import * as React from "react";
import * as Session from "../../Session";
import * as HotelApi from "./Api";
import * as Api from "../Api";
import { Convert } from "../../ParsedToReact";

interface IRoomProperties extends Session.ISessionProperty {
	Hotel: HotelApi.IHotel;
	Room: HotelApi.IRoom;
	RatesByRefId: Api.IStringDictionary<HotelApi.IRate>;
}

export default class Room extends React.Component<IRoomProperties, void> {
	render() {
		const room = this.props.Room;
		return (
			<li key={room.refId}>
				<h4>{room.name}: ${this.props.RatesByRefId[room.refId].totalFare.toFixed(2)}</h4>
				<div>{Convert(room.SanitizedDescription)}</div>
				<button onClick={() => this.props.Session.Cart.Add({ Name: room.name + " at " + this.props.Hotel.name })}>Add to Itinerary</button>
			</li>
		);
	}
}