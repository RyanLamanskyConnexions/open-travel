import * as TravelApi from "../Api";

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