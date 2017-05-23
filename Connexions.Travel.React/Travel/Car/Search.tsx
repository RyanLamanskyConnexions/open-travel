import * as React from "react";
import LabeledInput from "../../Common/LabeledInput";

interface IProperties {
	Visible: boolean;
}

interface IState {
	Currency: string;
	PickupAirportCode: string;
	DropoffAirportCode: string;
	DriverAge: number;
	DriverNationality: string;
}

export default class Search extends React.Component<IProperties, IState> {
	constructor() {
		super();

		this.state = {
			Currency: "USD",
			PickupAirportCode: "LAS",
			DropoffAirportCode: "LAS",
			DriverAge: 25,
			DriverNationality: "US",
		};
	}

	public render(): JSX.Element {
		return (
			<div
				style={{ display: this.props.Visible ? undefined : "none" }}
				className="TabPage"
			>
				<fieldset>
					<legend>Location</legend>
					<LabeledInput Name="Pickup Airport Code">
						<input
							type="text"
							maxLength={3}
							value={this.state.PickupAirportCode}
							onChange={event => this.setState({ PickupAirportCode: event.currentTarget.value })}
						/>
					</LabeledInput>
					<LabeledInput Name="Dropoff Airport Code">
						<input
							type="text"
							maxLength={3}
							value={this.state.DropoffAirportCode}
							onChange={event => this.setState({ DropoffAirportCode: event.currentTarget.value })}
						/>
					</LabeledInput>
				</fieldset>
				<fieldset>
					<legend>Driver</legend>
					<LabeledInput Name="Age">
						<input
							type="number"
							maxLength={3}
							value={this.state.DriverAge}
							onChange={event => this.setState({ DriverAge: parseInt(event.currentTarget.value) })}
						/>
					</LabeledInput>
					<LabeledInput Name="Nationality">
						<input
							type="text"
							maxLength={3}
							value={this.state.DriverNationality}
							onChange={event => this.setState({ DriverNationality: event.currentTarget.value })}
						/>
					</LabeledInput>
				</fieldset>
				<fieldset>
					<legend>Preferences</legend>
					<LabeledInput Name="Currency">
						<input
							type="text"
							maxLength={3}
							value={this.state.Currency}
							onChange={event => this.setState({ Currency: event.currentTarget.value })}
						/>
					</LabeledInput>
				</fieldset>
				<div>
					<button>Search</button>
				</div>
			</div>
		);
	}
}