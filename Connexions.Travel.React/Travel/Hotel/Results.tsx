﻿import * as React from "react";
import * as Session from "../../Session";
import * as HotelApi from "./Api";
import * as Search from "./Search";
import * as HotelResult from "./HotelResult";
import PageList from "../../Common/PageList";
import LabeledInput from "../../Common/LabeledInput";

interface ISearchResultViewResponse {
	hotels?: HotelApi.IHotel[];
}

const enum Sort {
	PriceAscending,
	PriceDescending,
}

interface IProperties extends Session.ISessionProperty {
	Search: Search.IState;
}

interface IState {
	PageIndex: number;
	PageChangeInProgress: boolean;
	View?: ISearchResultViewResponse;
	Sort: Sort;
}

export default class Results extends React.Component<IProperties, IState>
{
	constructor(props: IProperties) {
		super(props);

		this.state = {
			PageIndex: 0,
			PageChangeInProgress: false,
			Sort: props.Search.Sort === Search.InitialSort.HighestPriceFirst ? Sort.PriceDescending : Sort.PriceAscending,
		};
	}

	public Reset(sort: Search.InitialSort) {
		this.setState({
			PageIndex: 0,
			PageChangeInProgress: false,
			View: undefined,
			Sort: sort === Search.InitialSort.HighestPriceFirst ? Sort.PriceDescending : Sort.PriceAscending,
		});
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

		const view = this.state.View || this.props.Search.Response;
		const hotelResults: JSX.Element[] = [];
		if (view && view.hotels) {
			for (const hotel of view.hotels)
				hotelResults.push(
					<HotelResult.HotelResult
						key={hotel.id}
						Hotel={hotel}
						Search={this.props.Search}
						Session={this.props.Session}
					/>
				);
		}

		const status = this.props.Search.Status;
		const itemsPerPage = 10;

		const updateView = (pageIndex: number, sort: Sort): void => {
			this.props.Session.WebSocketCommand({
				"$type": "Connexions.Travel.Commands.Hotel.SearchResultView, Connexions.Travel",
				SessionId: status.SessionId,
				ItemsPerPage: itemsPerPage,
				PageIndex: pageIndex,
				Sorts: [sort], //Sorting by multiple elements could be useful.
			}, (response: ISearchResultViewResponse) => {
				this.setState({
					View: response,
					PageChangeInProgress: false,
				});
			});
		}

		const pageChange = (pageIndex: number): void => {
			this.setState({
				PageChangeInProgress: true,
				PageIndex: pageIndex,
			});

			updateView(pageIndex, this.state.Sort);
		};

		return (
			<div>
				<LabeledInput Name="Sort">
					<select
						disabled={this.state.PageChangeInProgress}
						value={this.state.Sort}
						onChange={event => {
							const sort = parseInt(event.currentTarget.value) as Sort;
							this.setState({ Sort: parseInt(event.currentTarget.value) });
							updateView(this.state.PageIndex, sort);
						}}
					>
						<option value={Sort.PriceAscending}>Lowest Price First</option>
						<option value={Sort.PriceDescending}>Highest Price First</option>
					</select>
				</LabeledInput>
				<span>{searchProgress}</span>
				<PageList
					Disabled={this.props.Search.SearchStep < Search.Step.ReceivedAllPages}
					PageCount={status.Count / itemsPerPage}
					PageIndex={this.state.PageIndex}
					ChangePage={pageChange}
				/>
				<ul
					style={{ display: hotelResults.length === 0 ? "none" : undefined }}
				>
					{hotelResults}
				</ul>
				<PageList
					Disabled={this.props.Search.SearchStep < Search.Step.ReceivedAllPages}
					PageCount={status.Count / itemsPerPage}
					PageIndex={this.state.PageIndex}
					ChangePage={pageChange}
				/>
			</div>
		);
	}
}