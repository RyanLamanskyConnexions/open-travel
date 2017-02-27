import * as TravelApi from "../Api";

export interface ICapiSearchResultsResponse extends TravelApi.ICapiBaseResponse {
	cars?: ICar[];
}

export interface ICar {
	id: string;
}