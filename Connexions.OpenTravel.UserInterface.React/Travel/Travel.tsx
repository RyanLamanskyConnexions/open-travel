import * as React from "react";
import HotelSearch from "./Hotel/HotelSearch";
import CarSearch from "./Car/CarSearch";
import * as Session from "../Session";

export default class Travel extends React.Component<Session.ISessionProperty, void> {
	render() {
		return (
			<div>
				<h2>Travel</h2>
				<HotelSearch Session={this.props.Session} />
				<CarSearch Session={this.props.Session} />
			</div>
		);
	}
}