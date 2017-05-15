import * as React from "react";
import * as Session from "../Session";

interface ICheckoutProperties extends Session.ISessionProperty {
	Show: boolean;
}

interface IState {
	CheckoutInProgress: boolean;
}

export default class Checkout extends React.PureComponent<ICheckoutProperties, IState> {
	private confirmations: JSX.Element[];

	constructor() {
		super();

		this.state = {
			CheckoutInProgress: false,
		};

		this.confirmations = [];
	}

	private Confirm() {
		this.setState({
			CheckoutInProgress: true,
		});

		for (var category of this.props.Session.Categories) {
			category.PriceCheck(() => { }, () => {
				category.Book(confirmation => {
					this.confirmations.push(confirmation);
					this.props.Session.Cart.Clear();

					this.setState({
						CheckoutInProgress: false,
					});
				});
			});
		}
	}

	render() {
		const showClass = this.props.Show ? "Show" : "";

		return (
			<div className={`Checkout ${showClass}`}>
				<h2>Checkout</h2>
				<button
					disabled={this.state.CheckoutInProgress}
					onClick={() => this.props.Session.setState({ View: Session.View.Main })}
				>Continue Shopping</button>
				<button
					disabled={this.state.CheckoutInProgress}
					onClick={() => this.Confirm()}
				>Confirm</button>
				{this.confirmations}
			</div>
		);
	}
}