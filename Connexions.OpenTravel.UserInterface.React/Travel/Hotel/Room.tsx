import * as React from "react";
import * as Session from "../../Session";
import * as HotelApi from "./Api";
import { Convert } from "../../ParsedToReact";

interface IRoomProperties extends Session.ISessionProperty {
	Room: HotelApi.IRoom;
}

export default class Room extends React.Component<IRoomProperties, void> {
	render() {
		const room = this.props.Room;
		return (
			<li key={room.refId}>
				<h4>{room.name}</h4>
				<div>{Convert(room.SanitizedDescription)}</div>
			</li>
		);
	}
}