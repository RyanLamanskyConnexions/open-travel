import * as TravelApi from "../Api";
import { IParsed } from "../../ParsedToReact";

export interface ICapiSearchResultsResponse extends TravelApi.ICapiBaseResponse {
	hotels?: IHotel[];
}

export interface IHotel {
	id: string;
	name: string;
	contact: IHotelContact;
	images: IHotelImage[];
	rating: number;
	fare: IHotelFare;
	geoCode: TravelApi.IGeocode;
}

export interface IHotelContact {
	address: IHotelContactAddress;
}

export interface IHotelContactAddress {
	line1: string;
	line2: string;
	city: IHotelContactAddressCodeName;
	state: IHotelContactAddressCodeName;
	countryCode: string;
	postalCode: string;
}

export interface IHotelContactAddressCodeName {
	code: string;
	name: string;
}

export interface IHotelImage {
	url: string;
	imageCaption: string;
	height: number;
	width: number;
	horizontalResolution: number;
	verticalResolution: number;
};

export interface IHotelFare {
	currency: string;
	totalFare: number;
}

export interface ICapiRoomSearchResultsResponse extends TravelApi.ICapiBaseResponse {
	rooms: IRoom[];
	rates: IRate[];
	recommendations: IRecommendation[];
}

export interface IRoom {
	refId: string;
	name: string;
	type: string;
	desc: string;
	code: string;
	roomTypeCode: string;
	smokingIndicator: string;
	SanitizedDescription: IParsed[];
}

export interface IPolicy {
	type: string;
	text: string;
}

export interface IBoardBasis {
	desc: string;
	type: string;
}

export interface ITaxes {
	code: string;
	desc: string;
	amount: number;
}

export interface IFees {
	desc: string;
	amount: number;
}

export interface IRateOccupancies {
	roomRefId: string;
	occupancyRefId: string;
}

export interface IFareBreakup {
	baseFare: number;
	currency: string;
	totalFare: number;
}

export interface IRecommendation {
	fareBreakup: IFareBreakup;
	id: string;
	rateRefIds: string[];
}

export interface IRate {
	refId: string;
	desc: string;
	isPrepaid: boolean;
	type: string;
	supplierId: string;
	code: string;
	refundability: string;
	policies: IPolicy[];
	boardBasis: IBoardBasis;
	baseFare: number;
	taxes: ITaxes[];
	fees: IFees[];
	discount: number;
	totalFare: number;
	rateOccupancies: IRateOccupancies[];
}