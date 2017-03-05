﻿import SessionTemplate from "../Session";

export interface ICategory {
	Name: string;
	PriceCheck(done: (item: IPurchasable, newPrice: number) => void): void;
	Items: IPurchasable[];
	Add(item: IPurchasable): void;
	Remove(item: IPurchasable): void;
}

export interface IPurchasable {
	/** A value that can uniquely identify the purchasable item within its associated system. */
	Identity: any;
	Name: string;
	Price: number;
	Category: ICategory;
	Refundability?: Refundability;
	Status?: Status;
}

export const enum Status {
	None,
	Purchased,
	Cancelled,
}

export const enum Refundability {
	Unknown,
	None,
	Cancel,
	Refund,
}

export interface Item<T> extends IPurchasable {
	/** A value that can uniquely identify the purchasable item within its associated system. */
	Identity: T;
	Category: Category<T>;
}

export abstract class Category<T> implements ICategory
{
	public session: SessionTemplate;
	public Name: string;
	public Items: Item<T>[];

	protected constructor(session: SessionTemplate, name: string) {
		this.session = session;
		this.Name = name;
		this.Items = [];
	}

	public Add(item: Item<T>)
	{
		this.Items.push(item);

		this.session.Cart.AddedItem();
	}

	public Remove(item: Item<T>) {
		const items = this.Items;
		const index = items.indexOf(item);

		if (index === -1)
			return;

		items.splice(index, 1);

		this.session.Cart.RemovedItem();
	}

	public PriceCheck(_done: (item: Item<T>, newPrice: number) => void) {
		this.session.WebSocketCommand({
			"$type": "Connexions.OpenTravel.UserInterface.Commands.Hotel.Price, Connexions.OpenTravel.UserInterface",
			Currency: "USD",
			Rooms: this.Items.map(item => item.Identity)
		}, _message => {
		});
	}
}