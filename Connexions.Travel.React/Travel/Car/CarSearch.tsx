import * as React from "react";
import * as Session from "../../Session";
import * as CarApi from "./Api";
import * as Api from "../Api";
import Result from "./Result";
import PageList from "../../Common/PageList";

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

export default class CarSearch extends React.Component<Session.ISessionProperty, ISearchState> {
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

	runSearch() {
		this.searchStarted = performance.now();
		this.setState({
			SearchResponse: CarSearch.GetBlankSearchResponse(),
			SearchInProgress: true,
			SearchTime: 0,
			PageIndex: 0,
		});

		this.props.Session.WebSocketCommand({
			"$type": "Connexions.Travel.Commands.Car.Search, Connexions.Travel",
			Currency: "USD",
			Pickup: Api.CreateInitialDate(30) + "T18:30",
			DropOff: Api.CreateInitialDate(32) + "T20:30",
			PickupAirport: "LAS",
			DropOffAirport: "LAS",
		}, message => {
			const response = message as ISearchResponse;
			this.setState({
				SearchResponse: response,
				SearchInProgress: !message.RanToCompletion,
			});

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
			}, message => {
				const response = message as ISearchResultViewResponse;
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

		return (
			<div className="CarSearch">
				<h3>Car Search</h3>
				<button disabled={this.state.SearchInProgress} onClick={() => this.runSearch()}>Search</button>
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
					<div>
						<h4>Results</h4>
						<PageList
							Disabled={this.state.SearchInProgress}
							PageCount={this.state.SearchResponse.Count / itemsPerPage}
							PageIndex={this.state.PageIndex}
							ChangePage={pageChange}
						/>
						{results}
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