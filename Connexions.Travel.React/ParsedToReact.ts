import * as React from "react";

/** A server-side-sanitized HTML component. */
export interface IParsed {
	IsUnsafe?: boolean;
}

export interface ITextElement extends IParsed {
	Content: string;
}

export interface IElement extends IParsed {
	Name: string;
	Children?: IParsed[];
	Attributes?: IAttribute[];
}

export interface IAttribute extends IParsed {
	Name: string;
	Value: string;
}

/** Converts server-side parsed HTML to react elements. */
export function Convert(parsed: IParsed[]): (React.DOMElement<any, any> | string)[] {
	let key = 0;
	return parsed.map(p => {
		const type = (p as any)["$type"] as string;
		switch (type) {
			case "Connexions.Travel.Html.Element, Connexions.Travel":
				const element = p as IElement;
				return React.createElement(element.Name as string, { key: ++key }, element.Children ? Convert(element.Children) : null);
			case "Connexions.Travel.Html.TextElement, Connexions.Travel":
				return (p as ITextElement).Content;
		}

		//Shouldn't get this far unless a new element type were introduced to the parser.
		return React.createElement("span", { key: ++key });
	})
}