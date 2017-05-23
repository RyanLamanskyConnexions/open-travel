import * as React from "react";

interface IProperties {
	Name: string;
}

export default class LabelledInput extends React.Component<IProperties, void>{
	public render(): JSX.Element {
		return (
			<label className="LabeledInput">
				<span>{this.props.Name}</span>
				{this.props.children}
			</label>
		);
	}
}