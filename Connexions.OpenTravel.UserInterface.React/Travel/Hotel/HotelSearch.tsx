import * as React from "react";
import * as Session from "../../Session";
import * as HotelApi from "./Api";
import Result from "./Result";
import PageList from "../../PageList";

interface ISearchResponse extends Session.ICommandMessage {
	SessionId: string;
	HotelCount: number;
	FirstPageAvailable: boolean;
	FirstPage: HotelApi.ICapiSearchResultsResponse;
	FullResultsAvailable: boolean;

}interface ISearchResultViewResponse extends Session.ICommandMessage {
	hotels: HotelApi.IHotel[];
}

interface IHotelSearchState {
	SearchInProgress: boolean;
	SearchResponse: ISearchResponse;
	SearchTime: number;
	View: HotelApi.ICapiSearchResultsResponse | ISearchResultViewResponse;
	PageIndex: number;
}

export default class HotelSearch extends React.Component<Session.ISessionProperty, IHotelSearchState> {
	private searchStarted: number;

	constructor() {
		super();

		this.state = {
			SearchInProgress: false,
			SearchResponse: HotelSearch.GetBlankSearchResponse(),
			SearchTime: 0,
			View: {},
			PageIndex: 0,
		};
	}

	private static GetBlankSearchResponse(): ISearchResponse {
		return {
			SessionId: "N/A",
			HotelCount: 0,
			FirstPageAvailable: false,
			FirstPage: {},
			FullResultsAvailable: false,
		} as ISearchResponse;
	}

	private static Pad(value: number, minimumSize: number) {
		return ("000000000" + value).substr(-minimumSize);
	}

	private static CreateInitialDate(daysToAdd: number): string {
		const date = new Date();
		date.setDate(date.getDate() + daysToAdd);
		return date.getFullYear() + "-" + HotelSearch.Pad((date.getMonth() + 1), 2) + "-" + HotelSearch.Pad(date.getDate(), 2);
	}

	runSearch() {
		this.searchStarted = performance.now();
		this.setState({
			SearchResponse: HotelSearch.GetBlankSearchResponse(),
			SearchInProgress: true,
			SearchTime: 0,
			View: {},
			PageIndex: 0,
		});

		this.props.Session.WebSocketCommand({
			"$type": "Connexions.OpenTravel.UserInterface.Commands.Hotel.Search, Connexions.OpenTravel.UserInterface",
			Currency: "USD",
			Occupants: [{ Age: 25 }, { Age: 26 }],
			CheckInDate: HotelSearch.CreateInitialDate(30),
			CheckOutDate: HotelSearch.CreateInitialDate(32),
			SearchOrigin: { Latitude: 36.08, Longitude: -115.152222 },
			SearchRadiusInKilometers: 48.2803,
			MinimumRating: 1,
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
				"$type": "Connexions.OpenTravel.UserInterface.Commands.Hotel.SearchResultView, Connexions.OpenTravel.UserInterface",
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

		return (
			<div>
				<h3>Hotel Search</h3>
				<button disabled={this.state.SearchInProgress} onClick={() => this.runSearch()}>Search</button>
				<div>
					<h4>Status</h4>
					<dl>
						<dt>Session ID</dt>
						<dd>{this.state.SearchResponse.SessionId}</dd>
						<dt>Hotel Count</dt>
						<dd>{this.state.SearchResponse.HotelCount.toString()}</dd>
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
				<div>
					<h4>Results</h4>
					<PageList
						Disabled={this.state.SearchInProgress}
						PageCount={this.state.SearchResponse.HotelCount / itemsPerPage}
						PageIndex={this.state.PageIndex}
						ChangePage={pageChange}
					/>
					{
						!!this.state.View && !!this.state.View.hotels ?
							this.state.View.hotels.map(hotel => <Result Session={this.props.Session} Hotel={hotel} key={hotel.id} />) :
							<div></div>
					}
					<PageList
						Disabled={this.state.SearchInProgress}
						PageCount={this.state.SearchResponse.HotelCount / itemsPerPage}
						PageIndex={this.state.PageIndex}
						ChangePage={pageChange}
					/>
				</div>
			</div>
		);
	}
}