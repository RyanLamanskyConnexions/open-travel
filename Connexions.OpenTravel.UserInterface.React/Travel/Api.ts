export interface ICapiBaseResponse {
	code?: string;
	message?: string;
}

export interface IGeocode {
	lat: number;
	long: number;
}

function Pad(value: number, minimumSize: number) {
	return ("000000000" + value).substr(-minimumSize);
}

/**
 * Creates an ISO8601-formatted date value as a string intended for use as the initial value of date inputs.
 * @param daysToAdd The number of days from today to use for the value.
 * @returns The ISO8601-formatted date string.
 */
export function CreateInitialDate(daysToAdd: number): string {
	const date = new Date();
	date.setDate(date.getDate() + daysToAdd);
	return date.getFullYear() + "-" + Pad((date.getMonth() + 1), 2) + "-" + Pad(date.getDate(), 2);
}