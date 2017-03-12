import * as React from "react";
import * as Session from "../../Session";
import * as CarApi from "./Api";

interface IResult extends Session.ISessionProperty {
	Car: CarApi.ICarRental;
	Vehicle: CarApi.IVehicle;
}

export default class Result extends React.Component<IResult, void> {
	render() {
		const vehicle = this.props.Vehicle;

		const size1xw = 195, size1xh = 90;
		const url1x = "/ImageProcessor/Resize?m=Fit&w=" + size1xw + "&h=" + size1xh + "&u=" + encodeURIComponent(vehicle.images[0]);
		const thumb = (
			<figure className="Thumbnail">
				<img src={url1x} />
			</figure>
		);

		return (
			<div>
				{thumb}
				<span>{this.props.Vehicle.name}</span>
			</div>
		);
	}
}