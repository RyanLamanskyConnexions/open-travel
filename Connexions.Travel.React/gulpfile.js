/// <binding ProjectOpened='default' />
"use strict";
const gulp = require("gulp");
const sass = require("gulp-sass");
const sourcemaps = require("gulp-sourcemaps");

// Runs the SASS compiler to convert SCSS to standard CSS.
gulp.task("default", () => gulp
	.src(["./**/*.scss", "!./node_modules/**/*", "!./wwwroot/**/*"])
	.pipe(sourcemaps.init())
	.pipe(sass().on("error", sass.logError))
	.pipe(sourcemaps.write("."))
	.pipe(gulp.dest("./wwwroot/build/"))
);