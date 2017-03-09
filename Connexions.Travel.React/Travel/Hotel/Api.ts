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

/** Contains the list of information about each priced room. You need to use this information while booking the room by using the Book API. */
export interface IRateOccupancyRoom {
	/** rateRefId for the priced room. The rateRefId represents a rate for a valid combination of rooms types and occupancies. */
	rateRefId: string;

	/** Unique reference ID for the occupancy returned. This ID is taken from the occupancies array. */
	occupancyRefId: string;

	/** Unique reference ID corresponding to the priced room. RefId taken from the rooms array. */
	roomRefId: string;
}

/** The response from a price API call. */
export interface IPriceRoomResponse {
	/** Unique session identifier for the current session. You can obtain the sessionId for your current session from the Room List API's response. */
	sessionId: string;

	/** Indicates whether the rate for the requested room is available from the supplier. */
	isRateAvailable: boolean;

	/** Old room rate. This room rate might be outdated. See the pricedTotalFare field for the updated (latest) room rates. */
	quotedTotalFare: number;

	/** Updated (latest) room rate. The pricedTotalFares the latest room rate whereas the quotedTotalFarerate can be an outdated rate. */
	pricedTotalFare: number;

	/** Contains the list of information about each priced room. You need to use this information while booking the room by using the Book API. */
	pricedRooms: IRateOccupancyRoom[];
}

/** The response from a booking API call. */
export interface IBookingInitializationResponse {
	/** Unique ID for the booking. You need to use the bookingId as an identifier for the booking in other API calls at a later point in time. */
	bookingId: string;
}

/** The response from a booking status API call. */
export interface IBookingStatusResponse extends IBookingInitializationResponse
{
	/** Indicates the progress of the Book API call. */
	bookingProgress: string;
	/** Indicates the status of the hotel booking. */
	bookingStatus: string;
	/** Indicates the status of the payment for the booking. */
	paymentStatus: string;
}