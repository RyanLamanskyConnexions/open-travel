"use strict";

const webpack = require("webpack");
const path = require("path");
const bundleOutputDir = './wwwroot/build';

module.exports = env => {
	const isDevBuild = !(env && env.prod);
	return {
		entry: {
			main: ["./Main"]
		},
		output: {
			path: path.join(__dirname, bundleOutputDir),
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
		plugins: isDevBuild ? [
			// Plugins that apply in development builds only
			new webpack.SourceMapDevToolPlugin({
				filename: "[file].map",
				moduleFilenameTemplate: path.relative(bundleOutputDir, "[resourcePath]")
			})
		] : [
				// Plugins that apply in production builds only
				new webpack.DefinePlugin({
					'process.env': {
						NODE_ENV: JSON.stringify('production')
					}
				}),
				new webpack.optimize.UglifyJsPlugin()
			]
	}
};