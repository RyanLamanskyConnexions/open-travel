import * as React from "react";
import * as Session from "../../Session";
import * as CarApi from "./Api";

interface IResult extends Session.ISessionProperty {
	Car: CarApi.ICarRental;
	Vehicle: CarApi.IVehicle;
}

export default class Result extends React.Component<IResult, void> {
	render() {
		return (
			<div>
				<span>{this.props.Vehicle.name}</span>
			</div>
		);
	}
}