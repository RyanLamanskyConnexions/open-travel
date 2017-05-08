import * as React from "react";
import * as ReactDOM from "react-dom";
import Session from "./Session";
import "babel-polyfill"; //Needed for IE11 compatibility when Babel is used (TypeScript alone doesn't require a polyfill).

ReactDOM.render(<Session />, document.getElementById("root"));