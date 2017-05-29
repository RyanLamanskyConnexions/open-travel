import * as React from "react";
import * as Session from "../../Session";
import * as HotelApi from "./Api";
import * as moment from "moment";
import LabeledInput from "../../Common/LabeledInput";
import Results from "./Results";

interface ISearchResponse extends Session.ICommandMessage {
	SessionId: string;
	Count: number;
	FirstPageAvailable: boolean;
	FirstPage: HotelApi.ICapiSearchResultsResponse;
	FullResultsAvailable: boolean;
}

interface IProperties extends Session.ISessionProperty {
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

export const enum Step {
	None,
	Initiating,
	NoResults,
	SomeResultsReady,
	AllResultsReady,
	ReceivedFirstPage,
	ReceivedAllPages,
}

export interface IState {
	Currency: string;
	Latitude: number;
	Longitude: number;
	Radius: number;
	MeasurementUnit: MeasurementUnit;
	CheckInDate: string;
	CheckOutDate: string;
	MinimumStars: number;
	Sort: InitialSort;
	SearchStep: Step;
	Response?: HotelApi.ICapiSearchResultsResponse;
	Status: ISearchResponse;
	SearchTime: number;
}

export class Search extends React.Component<IProperties, IState> {
	private searchStarted: number;
	private results: Results;

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
			MinimumStars: 3,
			Sort: InitialSort.HighestPriceFirst,
			SearchStep: Step.None,
			SearchTime: 0,
			Status: Search.CreateDefaultStatus(),
		};
	}

	private static CreateDefaultStatus(): ISearchResponse {
		return {
			SessionId: "N/A",
			Count: 0,
			FirstPageAvailable: false,
			FirstPage: {},
			FullResultsAvailable: false,
		} as ISearchResponse
	}

	public RunSearch() {
		this.searchStarted = performance.now();
		this.setState({
			Response: undefined,
			SearchStep: Step.Initiating,
			SearchTime: 0,
			Status: Search.CreateDefaultStatus(),
		});
		this.results.Reset();

		let initialSort: string | null;
		switch (this.state.Sort) {
			default: initialSort = null; break;
			case InitialSort.HighestPriceFirst: initialSort = "price desc"; break;
			case InitialSort.LowestPriceFirst: initialSort = "price asc"; break;
		}

		this.props.Session.WebSocketCommand({
			"$type": "Connexions.Travel.Commands.Hotel.Search, Connexions.Travel",
			InitialSort: initialSort,
			Request: {
				currency: this.state.Currency,
				roomOccupancies: [{
					occupants: [{
						type: "adult",
						age: 25,
					}, {
						type: "adult",
						age: 26,
					},
					],
				}],
				stayPeriod: {
					start: this.state.CheckInDate,
					end: this.state.CheckOutDate,
				},
				travellerCountryCodeOfResidence: "US",
				travellerNationalityCode: "US",
				bounds: {
					circle: {
						center: {
							lat: this.state.Latitude,
							long: this.state.Longitude,
						},
						radiusKm: this.state.Radius * (this.state.MeasurementUnit === MeasurementUnit.Miles ? 1.60934 : 1),
					},
				},
				filters: {
					minHotelRating: this.state.MinimumStars,
				},
			},
		}, (response: ISearchResponse) => {
			this.setState({ Status: response });

			let step = this.state.SearchStep;

			if (response.FullResultsAvailable && step <= Step.ReceivedAllPages)
				step = Step.ReceivedAllPages;
			else if (response.FirstPage && step <= Step.ReceivedFirstPage) {
				step = Step.ReceivedFirstPage;
				this.setState({ Response: response.FirstPage })
			}
			else if (response.FirstPageAvailable && step <= Step.AllResultsReady)
				step = Step.AllResultsReady;
			else if (response.Count > 0 && step <= Step.SomeResultsReady)
				step = Step.SomeResultsReady;
			else if (step <= Step.NoResults)
				step = Step.NoResults;

			if (this.state.SearchStep !== step)
				this.setState({ SearchStep: step });

			if (this.state.SearchTime === 0 && response.FirstPageAvailable) {
				this.setState({
					SearchTime: performance.now() - this.searchStarted,
				});
			}

			if (!!response.FirstPage) {
				this.setState({
				});
			}
		});
	}

	public render(): JSX.Element {
		const disableSearchFields = this.state.SearchStep > Step.None;

		return (
			<div
				style={{ display: this.props.Visible ? undefined : "none" }}
			>
				<div
					className="TabPage"
				>
					<fieldset>
						<legend>Location</legend>
						<LabeledInput Name="Latitude">
							<input
								disabled={disableSearchFields}
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
								disabled={disableSearchFields}
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
								disabled={disableSearchFields}
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
								disabled={disableSearchFields}
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
								disabled={disableSearchFields}
								type="date"
								value={this.state.CheckInDate}
								onChange={event => this.setState({ CheckInDate: event.currentTarget.value })}
							/>
						</LabeledInput>
						<LabeledInput Name="Check-Out Date">
							<input
								disabled={disableSearchFields}
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
								disabled={disableSearchFields}
								type="text"
								maxLength={3}
								value={this.state.Currency}
								onChange={event => this.setState({ Currency: event.currentTarget.value })}
							/>
						</LabeledInput>
						<LabeledInput Name="Star Rating">
							<select
								disabled={disableSearchFields}
								value={this.state.MinimumStars}
								onChange={event => this.setState({ MinimumStars: parseInt(event.currentTarget.value) })}
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
								disabled={disableSearchFields}
								value={this.state.Sort}
								onChange={event => this.setState({ Sort: parseInt(event.currentTarget.value) })}
							>
								<option value={InitialSort.HighestPriceFirst}>Highest Price First</option>
								<option value={InitialSort.LowestPriceFirst}>Lowest Price First</option>
							</select>
						</LabeledInput>
					</fieldset>
					<div>
						<button
							disabled={disableSearchFields}
							onClick={() => this.RunSearch()}
						>Search</button>
						<button
							style={{ display: !disableSearchFields || this.state.SearchStep < Step.ReceivedAllPages ? "none" : undefined }}
							disabled={!disableSearchFields}
							onClick={() => { this.setState({ SearchStep: Step.None, Response: undefined, SearchTime: 0 }) }}
						>Change</button>
					</div>
				</div>
				<Results
					ref={ref => this.results = ref}
					Search={this.state}
					Session={this.props.Session}
				/>
			</div>
		);
	}
}