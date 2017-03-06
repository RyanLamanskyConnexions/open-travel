/** Treats an object as if it were a dictionary keyed by string. */
export interface IStringDictionary<T> {
	[index: string]: T;
}