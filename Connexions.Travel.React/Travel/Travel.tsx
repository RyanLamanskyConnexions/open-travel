import * as React from "react";
import HotelSearch from "./Hotel/HotelSearch";
import CarSearch from "./Car/CarSearch";
import * as Session from "../Session";
import * as Hotel from "./Hotel/HotelSearch";

interface ITravelProperties extends Session.ISessionProperty {
	Show: boolean;
	HotelCategory: Hotel.HotelCategory;
}

export default class Travel extends React.Component<ITravelProperties, void> {
	render() {
		const showClass = this.props.Show ? "Show" : "";
		return (
			<div className={`Travel ${showClass}`}>
				<h2>Travel</h2>
				<HotelSearch
					Session={this.props.Session}
					Category={this.props.HotelCategory}
				/>
				<CarSearch Session={this.props.Session} />
			</div>
		);
	}
}