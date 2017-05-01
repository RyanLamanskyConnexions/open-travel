import * as React from "react";

export interface IPageListProperties {
	Disabled: boolean;
	PageIndex: number;
	PageCount: number;
	ChangePage: (pageIndex: number) => void;
}

/** An ordered list of page buttons that use 0-based indexing internally and presents as 1-based. */
export default class PageList extends React.Component<IPageListProperties, void> {
	render() {
		if (!this.props.PageCount)
			return null;
		if (this.props.PageCount === 0)
			return null;

		const buttons: JSX.Element[] = [];

		for (let i = 0; i < this.props.PageCount; i++) {
			buttons.push(
				<li key={i}>
					<button
						disabled={this.props.Disabled || i === this.props.PageIndex}
						onClick={event => this.props.ChangePage(parseInt((event.currentTarget as HTMLButtonElement).innerText) - 1)}
					>{i + 1}</button>
				</li>);
		}

		return (
			<fieldset>
				<legend>Page</legend>
				<ol className="PageList">
					<li>
						<button
							disabled={this.props.Disabled || this.props.PageIndex <= 0}
							onClick={_event => this.props.ChangePage(this.props.PageIndex - 1)}
						>Previous</button>
					</li>
					{buttons}
					<li>
						<button
							disabled={this.props.Disabled || this.props.PageIndex + 1 >= this.props.PageCount}
							onClick={_event => this.props.ChangePage(this.props.PageIndex + 1)}
						>Next</button>
					</li>
				</ol>
			</fieldset>
		);
	}
}