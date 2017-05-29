import * as React from "react";
import * as Session from "../../Session";
import * as Search from "./Search";
import * as HotelResult from "./HotelResult";

interface IProperties extends Session.ISessionProperty  {
	Search: Search.IState;
}

interface IState {
}

export default class Results extends React.Component<IProperties, IState>
{
	constructor(props: IProperties) {
		super(props);
	}

	public render(): JSX.Element | null {
		if (this.props.Search.SearchStep <= Search.Step.Initiating)
			return null;

		let searchProgress: string;
		switch (this.props.Search.SearchStep) {
			default: searchProgress = "..."; break;
			case Search.Step.Initiating: searchProgress = "Search initiating..."; break;
			case Search.Step.NoResults: searchProgress = "Waiting for results..."; break;
			case Search.Step.SomeResultsReady: searchProgress = "Some results ready..."; break;
			case Search.Step.AllResultsReady: searchProgress = "All results ready..."; break;
			case Search.Step.ReceivedFirstPage: searchProgress = "First page received..."; break;
			case Search.Step.ReceivedAllPages: searchProgress = "Search complete."; break;
		}


		const hotelResults: JSX.Element[] = [];
		const response = this.props.Search.Response;
		if (response && response.hotels) {
			for (const hotel of response.hotels)
				hotelResults.push(
					<HotelResult.HotelResult
						key={hotel.id}
						Hotel={hotel}
						Search={this.props.Search}
						Session={this.props.Session}
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