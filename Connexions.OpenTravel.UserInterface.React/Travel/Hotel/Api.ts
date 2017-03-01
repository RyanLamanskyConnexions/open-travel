import * as TravelApi from "../Api";

export interface IParsed {
	IsUnsafe?: boolean;
}

export interface ITextElement extends IParsed {
	Content?: string;
}

export interface IElement extends IParsed {
	Name?: string;
	Children?: IParsed[];
	Attributes?: IAttribute[];
}

export interface IAttribute {
	IsUnsafe?: boolean;
	Name?: string;
	Value?: string;
}

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

interface IRoom {
	refId: string;
	name: string;
	type: string;
	desc: string;
	code: string;
	roomTypeCode: string;
	smokingIndicator: string;
	SanitizedDescription: IParsed[];
}

interface IPolicy {
	type: string;
	text: string;
}

interface IBoardBasis {
	desc: string;
	type: string;
}

interface ITaxes {
	code: string;
	desc: string;
	amount: number;
}

interface IFees {
	desc: string;
	amount: number;
}

interface IRateOccupancies {
	roomRefId: string;
	occupancyRefId: string;
}

interface IFareBreakup {
	baseFare: number;
	currency: string;
	totalFare: number;
}

interface IRecommendation {
	fareBreakup: IFareBreakup;
	id: string;
	rateRefIds: string[];
}

interface IRate {
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