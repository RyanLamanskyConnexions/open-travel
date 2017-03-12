import * as TravelApi from "../Api";

export interface ICapiSearchResultsResponse extends TravelApi.ICapiBaseResponse {
	sessionId: string;
	currency: string;
	carRentals: ICarRental[];
	vendors: IVendor[];
	rentalLocations: IRentalLocation[];
	vehicles: IVehicle[];
}

export interface IPolicy {
	type: string;
	text: string;
}

export interface ICarRental {
	id: string;
	pickUpLocationId: string;
	dropoffLocationId: string;
	vehicleRefId: string;
	vendorCode: string;
	supplierId: string;
	rateCode: string;
	inventoryType: string;
	fare: IFare;
	policies: IPolicy[];
	cancellationPolicies: ICancellationPolicy[];
	mileage: IMileage;
}

export interface IFare {
	type: string;
	guaranteeRequired?: boolean;
	depositRequired?: boolean;
	displayFare: IDisplayFare;
	optionalCharges: IPayables;
}

export interface IFeeTaxDiscount {
	code: string;
	amount?: number;
	desc: string;
}

export interface ICalculationBasis {
	unitType: string;
	quantity?: number;
}

export interface IChargeCoverageEquipmentBase {
	desc: string;
	amount?: number;
	taxInclusive?: boolean;
	calculationBasis: ICalculationBasis;
}

export interface ICharges extends IChargeCoverageEquipmentBase {
	type: string;
}

export interface ICoverages extends ICharges {
	deductibleAmount?: number;
}

export interface IEquipment extends IChargeCoverageEquipmentBase {
	code: string;
	guaranteeRequired?: boolean;
}

export interface IPayables {
	fees: IFeeTaxDiscount[];
	taxes: IFeeTaxDiscount[];
	charges: ICharges[];
	coverages: ICoverages[];
	equipment: IEquipment[];
}

export interface IDisplayFare {
	totalFare?: number;
	breakup: IBreakup;
}

export interface IBreakup extends IPayables {
	baseFare: IBaseFare;
	discounts: IFeeTaxDiscount[];
}

export interface IBaseFare {
	amount?: number;
	calculationBasis: ICalculationBasis;
}

export interface ICancellationPolicy {
	window: IWindow;
	penaltyAmount?: number;
	text: string;
}

export interface IWindow {
	start: string;
	end: string;
}

export interface IMileage {
	isUnlimited?: boolean;
	allowed: IAllowed[];
}

export interface IAllowed {
	distance: IDistance;
	durationUnit: string;
}

export interface IDistance {
	unit: string;
	value: number;
}

export interface IVendor {
	code: string;
	name: string;
	logo: string;
	policies: IPolicy[];
}

export interface IRentalLocation {
	id: string;
	code: string;
	name: string;
	inTerminal?: boolean;
	atAirport?: boolean;
	shuttle: string;
	hoursOfOperation: IHoursOfOperation[];
	contactInfo: IContactInfo;
}

export interface IHoursOfOperation {
	dayOfWeek: string;
	workingHours: IWorkingHours[];
}

export interface IWorkingHours {
	openTime: string;
	closeTime: string;
}

export interface IContactInfo {
	address: IAddress
	email: string;
	phones: IPhone[];
}

export interface IAddress {
	line1: string;
	line2: string;
	city: ICodeName;
	state: ICodeName;
	countryCode: string;
	postalCode: string;
}

export interface ICodeName {
	code: string;
	name: string;
}

export interface IPhone {
	type: string;
	num: string;
	countryCode: string;
	ext: string;
}

export interface IVehicle {
	sippCode: string;
	refId: string;
	name: string;
	category: string;
	type: string;
	transmission: string;
	desc: string;
	images: string[];
	airConditioned?: boolean;
	fuelType: string;
	baggageCapacity: string;
	passengerCapacity: string;
	doorCount: string;
	driveType: string;
	policies: IPolicy[];
	specialEquipment: ISpecialEquipment[];
}

export interface ISpecialEquipment {
	type: string;
	desc: string;
}