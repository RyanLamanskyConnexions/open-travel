/// <binding AfterBuild='_build' Clean='clean-temporary' />
"use strict";
const gulp = require("gulp");
const clean = require("gulp-clean");
const webpack = require("webpack-stream");
const sass = require("gulp-sass");
const sourcemaps = require("gulp-sourcemaps");

// Clears the /build folder which is used to stage TypeScript output for webpack bundling.
gulp.task("clean-temporary", () => gulp
	.src("./build/*", { read: false }) //Don't actually need to read these files since we're deleting them.
	.pipe(clean())
);

//Merges all script files into a single bundle.
gulp.task("webpack", ["clean-temporary"], () => gulp
	.src("./App", { read: false }) //Read by webpack, only a placeholder here.
	.pipe(webpack(require("./webpack.config.js"), require('webpack')))
	.pipe(gulp.dest("./wwwroot/build/"))
);

// Runs the SASS compiler to convert SCSS to standard CSS.
gulp.task("sass", () => gulp
	.src(["./**/*.scss", "!./node_modules/**/*", "!./wwwroot/**/*"])
	.pipe(sourcemaps.init())
	.pipe(sass().on("error", sass.logError))
	.pipe(sourcemaps.write("."))
	.pipe(gulp.dest("./wwwroot/build/"))
);

// Does nothing itself, runs all dependencies in parallel, intended to be run after the .NET code build is successful.
gulp.task("_build", ["webpack", "sass"]);