import * as React from "react";
import * as Session from "../Session";

interface ICheckoutProperties extends Session.ISessionProperty {
	Show: boolean;
}

export default class Checkout extends React.Component<ICheckoutProperties, void> {
	constructor() {
		super();
	}

	private Confirm() {
	}

	render() {
		const showClass = this.props.Show ? "Show" : "";

		return (
			<div className={`Checkout ${showClass}`}>
				<h2>Checkout</h2>
				<button
					onClick={() => this.props.Session.setState({ View: Session.View.Main })}
				>Continue Shopping</button>
				<button
					onClick={() => this.Confirm()}
				>Confirm</button>
			</div>
		);
	}
}