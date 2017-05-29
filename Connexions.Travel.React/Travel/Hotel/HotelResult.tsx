import * as React from "react";
import * as HotelApi from "./Api";

interface IProperties {
	Hotel: HotelApi.IHotel;
}

interface IState {
}

export default class HotelResult extends React.Component<IProperties, IState> {
	public render(): JSX.Element {
		const hotel = this.props.Hotel;

		let thumb: JSX.Element;
		if (hotel.images.length > 0) {
			const size1xw = 170, size1xh = 170;
			const url1x = "/ImageProcessor/Resize?m=Fill&w=" + size1xw + "&h=" + size1xh + "&u=" + encodeURIComponent(hotel.images[0].url);
			const srcset =
				"/ImageProcessor/Resize?m=Fill&w=" + (size1xw * 1.5) + "&h=" + (size1xh * 1.5) + "&u=" + encodeURIComponent(hotel.images[0].url) + " 1.5x," +
				"/ImageProcessor/Resize?m=Fill&w=" + (size1xw * 2) + "&h=" + (size1xh * 2) + "&u=" + encodeURIComponent(hotel.images[0].url) + " 2x," +
				"/ImageProcessor/Resize?m=Fill&w=" + (size1xw * 2.5) + "&h=" + (size1xh * 2.5) + "&u=" + encodeURIComponent(hotel.images[0].url) + " 2.5x," +
				"/ImageProcessor/Resize?m=Fill&w=" + (size1xw * 3) + "&h=" + (size1xh * 3) + "&u=" + encodeURIComponent(hotel.images[0].url) + " 3x"
				;
			thumb = (
				<figure className="Thumbnail">
					<img src={url1x} srcSet={srcset} />
				</figure>
			);
		}
		else {
			thumb = <figure className="Thumbnail">No Image</figure>;
		}

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
			<li>
				{thumb}
				<span>{stars} {hotel.name}</span>
				<div><a href={mapUrl} target="_blank" rel="noopener noreferrer nofollow">{geocode}</a>; ID# {hotel.id}</div>
				<div>Total: <strong>${hotel.fare.totalFare.toFixed(2)}</strong> {hotel.fare.currency}</div>
			</li>
		);
	}
}