import * as React from "react";
import * as moment from "moment";
import LabeledInput from "../../Common/LabeledInput";

interface IProperties {
	Visible: boolean;
}

const enum MeasurementUnit {
	Miles,
	Kilometers,
}

const enum InitialSort {
	HighestPriceFirst,
	LowestPriceFirst,
}

interface IState {
	Currency: string;
	Latitude: number;
	Longitude: number;
	Radius: number;
	MeasurementUnit: MeasurementUnit;
	CheckInDate: string;
	CheckOutDate: string;
	Stars: number;
	Sort: InitialSort;
}

export default class Search extends React.Component<IProperties, IState> {
	constructor() {
		super();

		this.state = {
			Currency: "USD",
			Latitude: 36.08,
			Longitude: -115.152222,
			Radius: 30,
			MeasurementUnit: MeasurementUnit.Miles,
			CheckInDate: moment().add(30, "days").format("YYYY-MM-DD"),
			CheckOutDate: moment().add(32, "days").format("YYYY-MM-DD"),
			Stars: 3,
			Sort: InitialSort.HighestPriceFirst,
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
					<LabeledInput Name="Latitude">
						<input
							type="number"
							step="any"
							min="-90"
							max="90"
							value={this.state.Latitude}
							onChange={event => this.setState({ Latitude: parseFloat(event.currentTarget.value) })}
						/>
					</LabeledInput>
					<LabeledInput Name="Longitude">
						<input
							type="number"
							step="any"
							min="-180"
							max="180"
							value={this.state.Longitude}
							onChange={event => this.setState({ Longitude: parseFloat(event.currentTarget.value) })}
						/>
					</LabeledInput>
					<LabeledInput Name="Radius">
						<input
							type="number"
							step="any"
							min="0"
							max="50"
							value={this.state.Radius}
							onChange={event => this.setState({ Radius: parseFloat(event.currentTarget.value) })}
						/>
					</LabeledInput>
					<LabeledInput Name="Radius Measurement">
						<select
							value={this.state.MeasurementUnit}
							onChange={event => this.setState({ MeasurementUnit: parseInt(event.currentTarget.value) })}
						>
							<option value={MeasurementUnit.Miles}>Miles</option>
							<option value={MeasurementUnit.Kilometers}>Kilometers</option>
						</select>
					</LabeledInput>
				</fieldset>
				<fieldset>
					<legend>Details</legend>
					<LabeledInput Name="Check-In Date">
						<input
							type="date"
							value={this.state.CheckInDate}
							onChange={event => this.setState({ CheckInDate: event.currentTarget.value })}
						/>
					</LabeledInput>
					<LabeledInput Name="Check-Out Date">
						<input
							type="date"
							value={this.state.CheckOutDate}
							onChange={event => this.setState({ CheckOutDate: event.currentTarget.value })}
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
					<LabeledInput Name="Star Rating">
						<select
							value={this.state.Stars}
							onChange={event => this.setState({ Stars: parseInt(event.currentTarget.value) })}
						>
							<option value={1}>★</option>
							<option value={2}>★★</option>
							<option value={3}>★★★</option>
							<option value={4}>★★★★</option>
							<option value={5}>★★★★★</option>
						</select>
					</LabeledInput>
					<LabeledInput Name="Initial Sort">
						<select
							value={this.state.Stars}
							onChange={event => this.setState({ Stars: parseInt(event.currentTarget.value) })}
						>
							<option value={InitialSort.HighestPriceFirst}>Highest Price First</option>
							<option value={InitialSort.LowestPriceFirst}>Lowest Price First</option>
						</select>
					</LabeledInput>
				</fieldset>
				<div>
					<button>Search</button>
				</div>
			</div>
		);
	}
}