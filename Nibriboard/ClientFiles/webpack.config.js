var output_dir = "../obj/client_dist";

var fs = require("fs"),
	path = require("path");

module.exports = {
	entry: "./index.js",
	devtool: "source-map",
	output: {
		path: path.resolve(__dirname, output_dir),
		filename: "nibriclient.bundle.js"
	}
};
