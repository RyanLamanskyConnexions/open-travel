const webpack = require("webpack");
const path = require("path");

module.exports = {
	entry: ["./Main"],
	output: {
		path: path.resolve(__dirname + "/wwwroot/", "build"),
		filename: "bundle.js"
	},
	resolve: {
		extensions: [".ts", ".tsx", ".js"]
	},
	module: {
		loaders: [
			{
				test: /\.tsx?$/,
				loader: "ts-loader",
				exclude: /node_modules/
			}
		]
	},
	devtool: "source-map",
	externals: {
		//These are loaded via CDN in index.html
		"react": "React",
		"react-dom": "ReactDOM"
	}
};