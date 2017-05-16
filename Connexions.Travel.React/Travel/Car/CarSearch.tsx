import * as React from "react";
import * as Session from "../../Session";
import * as CarApi from "./Api";
import Result from "./Result";
import PageList from "../../Common/PageList";
import Travel from "../Travel";

interface ISearchResponse extends Session.ICommandMessage {
	SessionId: string;
	Count: number;
	FirstPageAvailable: boolean;
	FirstPage: CarApi.ICapiSearchResultsResponse;
	FullResultsAvailable: boolean;
}

interface ISearchResultViewResponse extends Session.ICommandMessage, CarApi.ICapiSearchResultsResponse {
}

interface ISearchState {
	SearchInProgress: boolean;
	SearchResponse: ISearchResponse;
	SearchTime: number;
	View?: CarApi.ICapiSearchResultsResponse | ISearchResultViewResponse;
	PageIndex: number;
}

interface IProperties extends Session.ISessionProperty {
	Travel: Travel;
}

export default class CarSearch extends React.Component<IProperties, ISearchState> {
	private searchStarted: number;

	constructor() {
		super();

		this.state = {
			SearchInProgress: false,
			SearchResponse: CarSearch.GetBlankSearchResponse(),
			SearchTime: 0,
			PageIndex: 0,
		};
	}

	private static GetBlankSearchResponse(): ISearchResponse {
		return {
			SessionId: "N/A",
			Count: 0,
			FirstPageAvailable: false,
			FirstPage: {},
			FullResultsAvailable: false,
		} as ISearchResponse;
	}

	public RunSearch() {
		this.searchStarted = performance.now();
		this.setState({
			SearchResponse: CarSearch.GetBlankSearchResponse(),
			SearchInProgress: true,
			SearchTime: 0,
			PageIndex: 0,
		});

		this.props.Session.WebSocketCommand({
			"$type": "Connexions.Travel.Commands.Car.Search, Connexions.Travel",
			Request: {
				currency: "USD",
				criteria: {
					pickup: {
						airportCode: this.props.Travel.state.Destination.IataCode,
						date: this.props.Travel.state.CheckInDate.format("YYYY-MM-DD"),
						time: "18:30",
					},
					dropOff: {
						sameAsPickup: true,
						//airportCode:,
						date: this.props.Travel.state.CheckOutDate.format("YYYY-MM-DD"),
						time: "20:30",
					},
					driverInfo: {
						age: 25,
						nationality: "US",
					},
				},
			},
		}, (response: ISearchResponse) => {
			this.setState({
				SearchResponse: response,
				SearchInProgress: !response.RanToCompletion,
			});

			if (response.RanToCompletion) {
				this.props.Travel.CarSearchCompleted();
			}

			if (this.state.SearchTime === 0 && response.FirstPageAvailable) {
				this.setState({
					SearchTime: performance.now() - this.searchStarted,
				});
			}

			if (!!response.FirstPage) {
				this.setState({
					View: response.FirstPage,
				});
			}
		});
	}

	render() {
		const itemsPerPage = 10;

		const pageChange = (pageIndex: number): void => {
			if (this.state.SearchInProgress)
				return;

			this.setState({
				SearchInProgress: true,
				PageIndex: pageIndex,
			});

			this.props.Session.WebSocketCommand({
				"$type": "Connexions.Travel.Commands.Car.SearchResultView, Connexions.Travel",
				SessionId: this.state.SearchResponse.SessionId,
				ItemsPerPage: itemsPerPage,
				PageIndex: pageIndex,
			}, (response: ISearchResultViewResponse) => {
				this.setState({
					View: response,
					SearchInProgress: false,
				});
			});
		};

		const results: JSX.Element[] = [];
		if (!!this.state.View) {
			for (const carRental of this.state.View.carRentals) {
				results.push(
					<Result
						key={carRental.id}
						Session={this.props.Session}
						Car={carRental}
						Vehicle={this.state.View.vehicles.filter(vehicle => vehicle.refId === carRental.vehicleRefId)[0]}
					/>
				);
			}
		}

		let searchError: JSX.Element | undefined;
		let status: JSX.Element | undefined;
		if (!this.state.SearchInProgress && this.state.SearchResponse.ErrorMessage) {
			searchError = <p>{this.state.SearchResponse.ErrorMessage}</p>;
		}
		else {
			status = (
				<div>
					<h4>Status</h4>
					<dl>
						<dt>Session ID</dt>
						<dd>{this.state.SearchResponse.SessionId}</dd>
						<dt>Count</dt>
						<dd>{this.state.SearchResponse.Count.toString()}</dd>
						<dt>Is Complete</dt>
						<dd>{
							this.state.SearchResponse.FirstPageAvailable ?
								!!this.state.View ?
									`Yes, in ${(this.state.SearchTime / 1000).toFixed(3)} seconds`
									: "Almost..."
								: "No"
						}{!!this.state.SearchResponse.FullResultsAvailable ? "; full results available." : ""}</dd>
					</dl>
				</div>
			);
		}

		return (
			<div className="CarSearch">
				<h3>Car Search</h3>
				<div>
					{status}
					<div>
						<h4>Results</h4>
						<PageList
							Disabled={this.state.SearchInProgress}
							PageCount={this.state.SearchResponse.Count / itemsPerPage}
							PageIndex={this.state.PageIndex}
							ChangePage={pageChange}
						/>
						{results}
						{searchError}
						<PageList
							Disabled={this.state.SearchInProgress}
							PageCount={this.state.SearchResponse.Count / itemsPerPage}
							PageIndex={this.state.PageIndex}
							ChangePage={pageChange}
						/>
					</div>
				</div>
			</div>
		);
	}
}