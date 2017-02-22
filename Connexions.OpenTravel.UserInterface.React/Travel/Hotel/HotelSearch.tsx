import * as React from "react";
import * as Session from "../../Session";

export default class HotelSearch extends React.Component<Session.ISessionProperty, void> {
	render() {
		return (
			<div>
				<h3>Hotel Search</h3>
				<button onClick={() => this.props.Session.WebSocketCommand({
					"$type": "Connexions.OpenTravel.UserInterface.Commands.Hotel.Search, Connexions.OpenTravel.UserInterface",
					Currency: "USD",
					Occupants: [{ Age: 25 }, { Age: 26 }],
					CheckInDate: "2017-02-28",
					CheckOutDate: "2017-03-01",
					SearchOrigin: { Latitude: 36.08, Longitude: -115.152222 },
					SearchRadiusInKilometers: 48.2803,
					MinimumRating: 1,
				}, _message => { })}>Search</button>
			</div>
		);
	}
}