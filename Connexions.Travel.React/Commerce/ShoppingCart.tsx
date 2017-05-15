import PopUp from "../Common/PopUp";
import * as React from "react";
import * as Session from "../Session";
import * as Category from "./Category";

interface IShoppingCartProperties extends Session.ISessionProperty {
	Categories: Category.ICategory[];
}

interface IShoppingCartState {
	ShowCartDialog: boolean;
	ItemCount: number;
}

export default class ShoppingCart extends React.PureComponent<IShoppingCartProperties, IShoppingCartState> {
	constructor() {
		super();

		this.state = {
			ShowCartDialog: false,
			ItemCount: 0,
		};
	}

	protected PopUpClose() {
		this.setState({ ShowCartDialog: false });
	}

	private NavigateToCheckout() {
		this.props.Session.setState({ View: Session.View.Checkout });
		this.setState({ ShowCartDialog: false });
	}

	/**
	 * Notifies the cart that an item has just been added.
	 */
	public AddedItem() {
		this.setState({
			ShowCartDialog: true,
			ItemCount: this.state.ItemCount + 1,
		});
	}

	/**
	 * Notifies the cart that an item has just been removed.W
	 */
	public RemovedItem() {
		this.setState({
			ItemCount: this.state.ItemCount - 1,
		});
	}

	public Clear() {
		this.setState({
			ItemCount: 0,
		});
	}

	render(): JSX.Element {
		let viewCart: string;
		const cartIsEmpty = this.state.ItemCount === 0;

		if (cartIsEmpty === false)
			viewCart = `View Cart (${this.state.ItemCount})`
		else
			viewCart = "Cart is Empty";

		return (
			<div>
				<button className="Cart" disabled={cartIsEmpty} onClick={() => this.setState({ ShowCartDialog: true })}>{viewCart}</button>
				<PopUp Title="Shopping Cart" Show={this.state.ShowCartDialog} OnClose={() => this.PopUpClose()}>
					{
						this.props.Categories.map(category => {
							const categoryItems = category.Items;
							if (!categoryItems)
								return null; //This shouldn't happen but enforces that categoryItems is valid.

							return (
								<table className="CartItems" key={category.Name}>
									<caption>{category.Name}</caption>
									<tbody>
										{categoryItems.map(item => {
											return (
												<tr key={`${category.Name} ${JSON.stringify(item.Identity)}`}>
													<td>{item.Name}</td>
													<td>${item.Price.toFixed(2)}</td>
													<td>
														<button onClick={() => category.Remove(item)}>Remove</button>
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
					<button
						disabled={cartIsEmpty}
						onClick={() => this.NavigateToCheckout()}
					>Check Out</button>
				</PopUp>
			</div>
		);
	}
}