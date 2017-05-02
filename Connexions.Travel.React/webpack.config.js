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
	plugins: [
		new webpack.DefinePlugin({
			'process.env': {
				NODE_ENV: JSON.stringify('production')
			}
		}),
		new webpack.optimize.UglifyJsPlugin(
			{
				sourceMap: true
			})
	]
};