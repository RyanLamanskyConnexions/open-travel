﻿import * as React from "react";
import * as Session from "../../Session";
import * as HotelApi from "./Api";
import { Convert } from "../../ParsedToReact";
import * as Cart from "../../Commerce/ShoppingCart"

interface IRoomProperties extends Session.ISessionProperty {
	RecommendationId: string;
	SessionId: string;
	Hotel: HotelApi.IHotel;
	Room: HotelApi.IRoom;
	Rate: HotelApi.IRate;
	Category: Cart.ICategory;
}

export default class Room extends React.Component<IRoomProperties, void> {
	render() {
		const room = this.props.Room;
		const price = this.props.Rate.totalFare;

		return (
			<li key={room.refId}>
				<h4>{room.name}: ${price.toFixed(2)}</h4>
				<div>{Convert(room.SanitizedDescription)}</div>
				<button onClick={() => this.props.Session.Cart.Add({
					Identity: {
						SessionId: this.props.SessionId,
						HotelId: this.props.Hotel.id,
						RecommendationId: this.props.RecommendationId,
					},
					Category: this.props.Category,
					Name: `${room.name} at ${this.props.Hotel.name}`,
					Price: price
				})}>Add to Itinerary</button>
			</li>
		);
	}
}