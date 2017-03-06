import * as React from "react";
import * as Session from "../../Session";

interface IResult extends Session.ISessionProperty {
	Car: any;
}

export default class Result extends React.Component<IResult, void> {
	render() {
		return (
			<div>
				<span>Car</span>
			</div>
		);
	}
}