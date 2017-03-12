import * as React from "react";
import Travel from "./Travel/Travel";
import ShoppingCart from "./Commerce/ShoppingCart"
import * as Category from "./Commerce/Category"
import Checkout from "./Commerce/Checkout"
import * as HotelSearch from "./Travel/Hotel/HotelSearch";

export const enum View {
	Main,
	Checkout
}

interface ISessionState {
	SocketStatus: string;
	KnownAirports: any[];
	View: View;
}

/** Common features of all command response messages. */
export interface ICommandMessage {
	/** The sequence number of the command as tracked by the client. */
	Sequence: number;
	/** When true, no further messages to this command will be sent and it should no longer be tracked by the client. */
	RanToCompletion: boolean;
	/** When present and not null, contains details of an error. */
	ErrorMessage?: string;
}

interface IAuthorizeResponse extends ICommandMessage {
	/** Returning a predetermined list airports as part of the authorization response is a placeholder until an autocomplete source is available. */
	KnownAirports: IAirport[];
}

interface IAirport {
	IataCode: string;
	Name: string;
	Latitude: number;
	Longitude: number;
}

/** Contains a Session property for the session management services. */
export interface ISessionProperty {
	/** Provides user session services. */
	Session: Session;
}

/** The root React node, holds on to a WebSocket for the duration of the user's visit. */
export default class Session extends React.Component<void, ISessionState> {
	private socket: WebSocket;
	private commandNumber: number;
	private activeCommands: { [key: number]: (message: ICommandMessage) => void };
	public Cart: ShoppingCart;
	public Travel: Travel;
	public Categories: Category.ICategory[];
	private HotelCategory: HotelSearch.HotelCategory;

	constructor() {
		super();

		this.state = {
			SocketStatus: "None",
			KnownAirports: [],
			View: View.Main,
		};
		this.commandNumber = 0;
		this.activeCommands = {};
		this.Categories = [
			this.HotelCategory = new HotelSearch.HotelCategory(this),
		];
	}

	SetSocketStatus(message: string) {
		this.setState({ SocketStatus: message })
	}

	SetSocketStatusActiveCommands() {
		const activeCommands = Object.keys(this.activeCommands).length;
		let message: string;
		switch (activeCommands) {
			case 0: message = "No active commands."; break;
			case 1: message = "1 active command."; break;
			default: message = activeCommands.toString() + " active commands."; break;
		}
		this.SetSocketStatus(message);
	}

	componentDidMount() {
		this.socket = new WebSocket("ws" + (location.protocol === "https:" ? "s" : "") + "://" + location.host + "/Session");
		const component = this;

		this.socket.onopen = () => {
			component.SetSocketStatus("Connected.");
			component.WebSocketCommand({
				"$type": "Connexions.Travel.Commands.Authorize, Connexions.Travel",
			}, message => {
				const response = message as IAuthorizeResponse;
				this.setState({
					KnownAirports: response.KnownAirports,
				});
			});
		};

		this.socket.onclose = () => {
			component.activeCommands = {};
			//TODO: Attempt to reconnect if the component is still mounted.
			component.SetSocketStatus("Disconnected");
		};

		this.socket.onmessage = event => {
			const message = JSON.parse(event.data) as ICommandMessage;
			const processor = component.activeCommands[message.Sequence];
			if (processor) {
				if (message.RanToCompletion) {
					delete component.activeCommands[message.Sequence];
					component.SetSocketStatusActiveCommands();
				}
				processor(message);
			}
		}
	}

	componentWillUnmount() {
		if (!this.socket)
			return;

		this.socket.close();
	}

	/**
	 * Issues a command to the server via the session's WebSocket connection.
	 * @param object The object to send.  A Sequence property is added to uniquely identify this message.
	 * @param processor Any received response messages are sent to the provided response processor.
	 * @returns The command's sequence number.
	 */
	WebSocketCommand(object: any, processor: (message: ICommandMessage) => void): number {
		const commandNumber = ++this.commandNumber;
		this.activeCommands[commandNumber] = processor;
		this.SetSocketStatusActiveCommands();

		object.Sequence = commandNumber;
		this.socket.send(JSON.stringify(object));
		return commandNumber;
	}

	render() {
		return (
			<div>
				<h1>Connexions Open Travel</h1>
				<ShoppingCart
					ref={ref => this.Cart = ref}
					Session={this}
					Categories={this.Categories}
				/>
				<Travel
					ref={ref => this.Travel = ref}
					Session={this}
					Show={this.state.View === View.Main}
					HotelCategory={this.HotelCategory}
				/>
				<Checkout
					Session={this}
					Show={this.state.View === View.Checkout}
				/>
				<p>Session Status: <span>{this.state.SocketStatus}</span></p>
			</div >
		);
	}
}