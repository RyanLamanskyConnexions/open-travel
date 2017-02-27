/// <binding AfterBuild='webpack' Clean='clean-temporary' />
"use strict";
const gulp = require("gulp");
const clean = require("gulp-clean");
const webpack = require("webpack-stream");

//The webpack implementation here is sub-optimal: it's not leveraging gulp's streaming capability, instead basically just running webpack.

// Clears the /build folder which is used to stage TypeScript output for webpack bundling.
gulp.task("clean-temporary", () => gulp
	.src("./build/*", { read: false }) //Don't actually need to read these files sicne we're deleting them.
	.pipe(clean())
);

//
gulp.task("webpack", ["clean-temporary"], () => gulp
	.src("./App", { read: false }) //Read by webpack, only a placeholder here.
	.pipe(webpack(require("./webpack.config.js"), require('webpack')))
	.pipe(gulp.dest("./wwwroot/build/"))
);