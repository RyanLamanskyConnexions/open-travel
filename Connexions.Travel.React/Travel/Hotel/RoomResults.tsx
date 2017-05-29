import * as React from "react";
import * as Session from "../../Session";
import * as HotelResult from "./HotelResult";
import RoomResult from "./RoomResult";
import * as HotelApi from "./Api";
import Category from "./Category"

interface IProperties extends Session.ISessionProperty {
	Hotel: HotelApi.IHotel;
	Search: HotelResult.IState;
	Category: Category;
}

interface IState {
}

export default class Results extends React.Component<IProperties, IState> {
	constructor(props: IProperties) {
		super(props);
	}

	public render(): JSX.Element | null {
		let searchProgress: string | null;
		switch (this.props.Search.SearchStep) {
			case HotelResult.Step.Initiating: searchProgress = "Search initiating..."; break;
			case HotelResult.Step.NoResults: searchProgress = "Waiting for results..."; break;
			default:
			case HotelResult.Step.AllResultsReady: searchProgress = null; break;
		}

		const roomResults: JSX.Element[] = [];
		const rooms = this.props.Search.Rooms;
		for (const reccomendation of rooms) {
			roomResults.push(
				<RoomResult
					key={reccomendation.RecommendationId}
					Session={this.props.Session}
					Room={reccomendation.Room}
					Rate={reccomendation.Rate}
					Hotel={this.props.Hotel}
					RecommendationId={reccomendation.RecommendationId}
					SessionId={reccomendation.SessionId}
					Category={this.props.Category}
				/>
			);
		}

		return (
			<div>
				<span>{searchProgress}</span>
				<ul
					style={{ display: roomResults.length === 0 ? "none" : undefined }}
				>
					{roomResults}
				</ul>
			</div>
		);
	}
}