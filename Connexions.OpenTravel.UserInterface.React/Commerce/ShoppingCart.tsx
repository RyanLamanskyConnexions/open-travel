import PopUp from "../Common/PopUp";
import * as React from "react";

export interface IPurchasable {
	Name: string;
	Refundability?: Refundability;
	Status?: Status;
}

export const enum Status {
	None = 0,
	Purchased = 1,
	Cancelled = 2,
}

export const enum Refundability {
	None = 0,
	Cancel = 1,
	Refund = 2,
}

interface IShoppingCartProperties {
}

interface IShoppingCartState {
	Items: IPurchasable[];
	ShowCartDialog: boolean;
}

export default class ShoppingCart extends React.Component<IShoppingCartProperties, IShoppingCartState> {
	constructor() {
		super();

		this.state = {
			Items: [],
			ShowCartDialog: false,
		};
	}

	public Add(item: IPurchasable) {
		this.setState(
			{
				Items: this.state.Items.concat(item),
				ShowCartDialog: true,
			});
	}

	public Remove(item: IPurchasable) {
		const items = this.state.Items;
		const index = items.indexOf(item);
		if (index === -1)
			return;

		items.splice(index, 1);
		if (items.length === 0) {
			this.setState({
				ShowCartDialog: false,
			});
		}
		this.forceUpdate();
	}

	protected PopUpClose() {
		this.setState({ ShowCartDialog: false });
	}

	render(): JSX.Element {
		let viewCart: string;
		if (this.state.Items.length != 0)
			viewCart = `View Cart (${this.state.Items.length})`
		else
			viewCart = "Cart is Empty";

		return (
			<div>
				<button className="Cart" disabled={this.state.Items.length === 0} onClick={() => this.setState({ ShowCartDialog: true })}>{viewCart}</button>
				<PopUp Title="Shopping Cart" Show={this.state.ShowCartDialog} OnClose={() => this.PopUpClose()}>
					<p>Items in Cart:</p>
					<ul>
						{this.state.Items.map(item => {
							return (
								<li>{item.Name} <button onClick={() => this.Remove(item)}>Remove</button></li>
							);
						})}
					</ul>
					<button onClick={() => this.PopUpClose()}>Continue Shopping</button>
					<button disabled>Check Out</button>
				</PopUp>
			</div>
		);
	}
}