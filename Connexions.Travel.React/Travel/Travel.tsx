import * as React from "react";
import HotelSearch from "./Hotel/HotelSearch";
import CarSearch from "./Car/CarSearch";
import * as Session from "../Session";
import * as Hotel from "./Hotel/HotelSearch";
import DatePicker from "react-datepicker";
import * as moment from "moment";

interface ITravelProperties extends Session.ISessionProperty {
	Show: boolean;
	HotelCategory: Hotel.HotelCategory;
}

interface IState {
	Destination: Session.IAirport;
	CheckInDate: moment.Moment;
	CheckOutDate: moment.Moment;
	HotelSearchInProgress: boolean;
	CarSearchInProgress: boolean;
}

export default class Travel extends React.PureComponent<ITravelProperties, IState> {
	private HotelSearch: HotelSearch;
	private CarSearch: CarSearch;

	constructor(props: ITravelProperties) {
		super(props);

		this.state = {
			Destination: props.Session.state.KnownAirportsByCode["LAS"],
			CheckInDate: moment().add(30, "days"),
			CheckOutDate: moment().add(32, "days"),
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
				<ul>
					<li>
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
					</li>
					<li>
						<label>
							<span>Check-In Date</span>
							<DatePicker
								disabled={searchInProgress}
								selected={this.state.CheckInDate}
								monthsShown={2}
								minDate={moment()}
								maxDate={moment().add(1, "years")}
								onChange={date => { this.setState({ CheckInDate: date || moment().add(30, "days") }) }}
							/>
						</label>
					</li>
					<li>
						<label>
							<span>Check-Out Date</span>
							<DatePicker
								disabled={searchInProgress}
								selected={this.state.CheckOutDate}
								monthsShown={2}
								minDate={moment()}
								maxDate={moment().add(1, "years")}
								onChange={date => { this.setState({ CheckOutDate: date || moment().add(32, "days") }) }}
							/>
						</label>
					</li>
				</ul>
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