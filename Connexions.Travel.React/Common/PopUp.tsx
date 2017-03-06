import * as React from "react";

interface IPopUpProperties {
	Show: boolean;
	Title: string;
	OnClose: () => void;
}

export default class PopUp extends React.Component<IPopUpProperties, void> {
	render(): JSX.Element {
		const showClass = this.props.Show ? "Show" : "";
		return (
			<div className={`PopUp ${showClass}`}>
				<div className="Dialog">
					<h2>{this.props.Title}</h2>
					<button className="Close" onClick={() => this.props.OnClose()}>×</button>
					<div className="Content">
						{this.props.children}
					</div>
				</div>
			</div>
		);
	}
}