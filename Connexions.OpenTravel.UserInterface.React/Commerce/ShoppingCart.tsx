import PopUp from "../Common/PopUp";
import * as React from "react";
import * as Objects from "../Common/Objects";

export interface IPurchasable {
	Identity: string;
	Name: string;
	Price: number;
	Category: string;
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
		const items = this.state.Items;

		if (items.length != 0)
			viewCart = `View Cart (${items.length})`
		else
			viewCart = "Cart is Empty";

		const categories: Objects.IStringDictionary<IPurchasable[] | undefined> = {};
		for (const item of items) {
			let array = categories[item.Category];
			if (array === undefined)
				categories[item.Category] = array = [];

			array.push(item);
		}

		return (
			<div>
				<button className="Cart" disabled={items.length === 0} onClick={() => this.setState({ ShowCartDialog: true })}>{viewCart}</button>
				<PopUp Title="Shopping Cart" Show={this.state.ShowCartDialog} OnClose={() => this.PopUpClose()}>
					{
						Object.keys(categories).map(category => {
							const categoryItems = categories[category];
							if (!categoryItems)
								return null; //This shouldn't happen but enforces that categoryItems is valid.

							return (
								<table className="CartItems" key={category}>
									<caption>{category}</caption>
									<tbody>
										{categoryItems.map(item => {
											return (
												<tr key={`${category} ${item.Identity}`}>
													<td>{item.Name}</td>
													<td>${item.Price.toFixed(2)}</td>
													<td>
														<button onClick={() => this.Remove(item)}>Remove</button>
													</td>
												</tr>
											);
										})}
									</tbody>
								</table>
							);
						})
					}
					<button onClick={() => this.PopUpClose()}>Continue Shopping</button>
					<button disabled>Check Out</button>
				</PopUp>
			</div>
		);
	}
}