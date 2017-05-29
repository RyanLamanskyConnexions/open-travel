import * as React from "react";
import * as Search from "./Search";
import * as HotelApi from "./Api";
import HotelResult from "./HotelResult";

interface IProperties {
	SearchStep: Search.Step;
	Response?: HotelApi.ICapiSearchResultsResponse;
	SearchTime: number;
}

interface IState {
}

export default class Results extends React.Component<IProperties, IState>
{
	constructor(props: IProperties) {
		super(props);
	}

	public render(): JSX.Element | null {
		if (this.props.SearchStep <= Search.Step.Initiating)
			return null;

		let searchProgress: string;
		switch (this.props.SearchStep) {
			default: searchProgress = "..."; break;
			case Search.Step.Initiating: searchProgress = "Search initiating..."; break;
			case Search.Step.NoResults: searchProgress = "Waiting for results..."; break;
			case Search.Step.SomeResultsReady: searchProgress = "Some results ready..."; break;
			case Search.Step.AllResultsReady: searchProgress = "All results ready..."; break;
			case Search.Step.ReceivedFirstPage: searchProgress = "First page received..."; break;
			case Search.Step.ReceivedAllPages: searchProgress = "Search complete."; break;
		}


		const hotelResults: JSX.Element[] = [];
		const response = this.props.Response;
		if (response && response.hotels) {
			for (const hotel of response.hotels)
				hotelResults.push(
					<HotelResult
						key={hotel.id}
						Hotel={hotel}
					/>
				);
		}

		return (
			<div>
				<span>{searchProgress}</span>
				<ul
					style={{ display: hotelResults.length === 0 ? "none" : undefined }}
				>
					{hotelResults}
				</ul>
			</div>
		);
	}
}