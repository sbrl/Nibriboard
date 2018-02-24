"use strict";

var output_dir = "../obj/client_dist";

const fs = require("fs");
const path = require("path");

module.exports = {
	entry: [
		// Polyfills
		"./node_modules/dialog-polyfill/dialog-polyfill.js",
		"./node_modules/dialog-polyfill/dialog-polyfill.css",
		
		// Main entry points
		"./index.js",
		"./Nibri.css",
	],
	devtool: "source-map",
	output: {
		path: path.resolve(__dirname, output_dir),
		filename: "nibriclient.bundle.js"
	},
	module: {
		rules: [
			{
				test: /\.css$/,
				use: [{
					loader: "file-loader",
					options: {
						name: "theme/[name].[ext]"
					}
				}]
			}
		]
	},
};
