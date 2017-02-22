import * as React from "react";
import HotelSearch from "./Hotel/HotelSearch";
import * as Root from "../Root";

export default class Travel extends React.Component<Root.ISessionProperty, void> {
	render() {
		return (
			<div>
				<h2>Travel</h2>
				<HotelSearch Session={this.props.Session} />
			</div>
		);
	}
}