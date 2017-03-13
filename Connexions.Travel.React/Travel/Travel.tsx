import * as React from "react";
import HotelSearch from "./Hotel/HotelSearch";
import CarSearch from "./Car/CarSearch";
import * as Session from "../Session";
import * as Hotel from "./Hotel/HotelSearch";

interface ITravelProperties extends Session.ISessionProperty {
	Show: boolean;
	HotelCategory: Hotel.HotelCategory;
}

interface IState {
	Destination: Session.IAirport;
	HotelSearchInProgress: boolean;
	CarSearchInProgress: boolean;
}

export default class Travel extends React.Component<ITravelProperties, IState> {
	private HotelSearch: HotelSearch;
	private CarSearch: CarSearch;

	constructor(props: ITravelProperties) {
		super(props);

		this.state = {
			Destination: props.Session.state.KnownAirportsByCode["LAS"],
			HotelSearchInProgress: false,
			CarSearchInProgress: false,
		};
	}

	private runSearches() {
		this.setState({
			HotelSearchInProgress: true,
			CarSearchInProgress: true,
		});

		this.HotelSearch.RunSearch();
		this.CarSearch.RunSearch();
	}

	public HotelSearchCompleted() {
		this.setState({
			HotelSearchInProgress: false,
		});
	}

	public CarSearchCompleted() {
		this.setState({
			CarSearchInProgress: false,
		});
	}

	render() {
		const searchInProgress =
			this.state.HotelSearchInProgress
			|| this.state.CarSearchInProgress
			;

		const showClass = this.props.Show ? "Show" : "";
		const airports: Session.IAirport[] = [];
		{
			const knownAirportsByCode = this.props.Session.state.KnownAirportsByCode;
			for (var key in knownAirportsByCode) {
				airports.push(knownAirportsByCode[key]);
			}
		}

		return (
			<div className={`Travel ${showClass}`}>
				<h2>Travel</h2>
				<label>
					<span>Destination</span>
					<select
						disabled={searchInProgress}
						value={this.state.Destination.IataCode}
						onChange={event => this.setState({ Destination: this.props.Session.state.KnownAirportsByCode[event.target.value] })}
					>
						{
							airports.map(airport => {
								return (
									<option
										value={airport.IataCode}
										key={airport.IataCode}
									>({airport.IataCode}) {airport.Name}</option>
								)
							})
						}
					</select>
				</label>
				<button
					disabled={searchInProgress}
					onClick={() => this.runSearches()}
				>Search</button>
				<HotelSearch
					ref={ref => { this.HotelSearch = ref }}
					Session={this.props.Session}
					Travel={this}
				/>
				<CarSearch
					ref={ref => { this.CarSearch = ref }}
					Session={this.props.Session}
					Travel={this}
				/>
			</div>
		);
	}
}