import * as React from "react";
import * as Session from "../../Session";
import * as HotelApi from "./Api";

interface IResult extends Session.ISessionProperty {
	Hotel: HotelApi.IHotel;
}

export default class Result extends React.Component<IResult, void> {
	render() {
		const hotel = this.props.Hotel;

		let stars: JSX.Element;
		switch (Math.max(0, Math.min(5, Math.round(hotel.rating)))) {
			default: stars = <span className="Stars"><span className="Clear">☆☆☆☆☆</span></span>; break;
			case 1: stars = <span className="Stars"><span className="Filled">★</span><span className="Clear">☆☆☆☆</span></span>; break;
			case 2: stars = <span className="Stars"><span className="Filled">★★</span><span className="Clear">☆☆☆</span></span>; break;
			case 3: stars = <span className="Stars"><span className="Filled">★★★</span><span className="Clear">☆☆</span></span>; break;
			case 4: stars = <span className="Stars"><span className="Filled">★★★★</span><span className="Clear">☆</span></span>; break;
			case 5: stars = <span className="Stars"><span className="Filled">★★★★★</span></span>; break;
		}

		const geocode = hotel.geoCode.lat.toFixed(4) + "," + hotel.geoCode.long.toFixed(4);
		const mapUrl = "https://www.google.com/maps/place/" + geocode + "/@" + geocode + ",17z/";

		return (
			<div>
				<span>{stars} {hotel.name}</span>
				<div><a href={mapUrl} target="_blank" rel="noopener noreferrer nofollow">{geocode}</a>; ID# {hotel.id}</div>
				<div>Total: <strong>${hotel.fare.totalFare.toFixed(2)}</strong> {hotel.fare.currency}</div>
			</div>
		);
	}
}