const webpack = require("webpack");
const path = require("path");

module.exports = {
	entry: {
		main: ["./Main"]
	},
	output: {
		path: path.resolve(__dirname + "/wwwroot/", "build"),
		publicPath: "/build/",
		filename: "bundle.js"
	},
	resolve: {
		extensions: [".ts", ".tsx", ".js"]
	},
	module: {
		rules: [
			{
				//ASP.NET Core WebpackDevMiddlewareOptions.HotModuleReplacement requires babel.
				//TypeScript alone would otherwise be enough.
				test: /\.ts(x?)$/,
				exclude: /node_modules/,
				use: {
					loader: 'babel-loader',
					options: {
						cacheDirectory: true
					}
				}
			},
			{
				test: /\.tsx?$/,
				exclude: /node_modules/,
				use: 'ts-loader'
			},
		]
	},
	devtool: "source-map",
	plugins: [
		new webpack.DefinePlugin({
			'process.env': {
				NODE_ENV: JSON.stringify('production')
			}
		}),
		new webpack.optimize.UglifyJsPlugin({
			sourceMap: true
		})
	]
};