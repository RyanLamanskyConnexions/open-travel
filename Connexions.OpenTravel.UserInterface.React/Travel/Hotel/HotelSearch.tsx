import * as React from "react";
import * as Session from "../../Session";

interface ISearchResponse extends Session.ICommandMessage {
	SessionId: string;
	HotelCount: number;
	IsComplete: boolean;
	CapiSearchResultsResponse: any;
}

interface IHotelSearchState {
	SearchInProgress: boolean;
	SearchResponse: ISearchResponse;
	SearchTime: number;
}

export default class HotelSearch extends React.Component<Session.ISessionProperty, IHotelSearchState> {
	private searchStarted: number;

	constructor() {
		super();

		this.state = {
			SearchInProgress: false,
			SearchResponse: HotelSearch.GetBlankSearchResponse(),
			SearchTime: 0,
		};
	}

	private static GetBlankSearchResponse(): ISearchResponse {
		return {
			SessionId: "N/A",
			HotelCount: 0,
			IsComplete: false,
			CapiSearchResultsResponse: {}
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
			this.setState(
				{
					SearchResponse: message as ISearchResponse,
					SearchInProgress: !message.RanToCompletion,
					SearchTime: message.RanToCompletion ? performance.now() - this.searchStarted : 0,
				});
		});
	}

	render() {
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
							this.state.SearchResponse.IsComplete ?
								this.state.SearchResponse.RanToCompletion ?
									`Yes, in ${(this.state.SearchTime / 1000).toFixed(3)} seconds `
									: "Almost..."
								: "No"
						}</dd>
					</dl>
				</div>
			</div>
		);
	}
}