var gulp = require('gulp');
var clean = require('gulp-clean');
var bower = require('gulp-bower');
var less = require('gulp-less');
var minifyCSS = require('gulp-minify-css');
var sourcemaps = require('gulp-sourcemaps');
var rename = require('gulp-rename');
var usemin = require('gulp-usemin');
var uglify = require('gulp-uglify');
var minifyHtml = require('gulp-minify-html');
var rev = require('gulp-rev');
var zip = require('gulp-zip');
var replace = require('gulp-replace');
var runSequence = require('run-sequence');

var config = {
    useMinify: true,
    bowerPath: './bower/',
    appLessPath: './less/',
    packagePath: './distr/',
    packageZipName: 'package.zip',
    packageZipPath: '../'
};


gulp.task('clean', function () {
    return gulp.src([config.bowerPath, config.packagePath], { read: false })
      .pipe(clean());
});

gulp.task('watch', function () {
    return gulp.watch(config.appLessPath + '**/*.less', ['app:less']);
});

gulp.task('default', ['watch']);

//#region deploy tasks
gulp.task('deploy', function (cb) {
    config.useMinify = true;
    runSequence('clean', 'bower', 'app', cb);
});

gulp.task('develop:nominify', function (cb) {
    config.useMinify = false;
    runSequence('clean', 'bower', 'app', cb);
});
//#endregion


//#region bower tasks
gulp.task('bower', function (cb) {
    runSequence('bower:install', ['bower:fontawesome-less', 'bower:bootstrap-less'], cb);
});

gulp.task('bower:install', function () {
    return bower()
      .pipe(gulp.dest(config.bowerPath));
});

gulp.task('bower:fontawesome-less', function () {
    return gulp.src(config.bowerPath + 'fontawesome/less/font-awesome.less')
        .pipe(sourcemaps.init())
        .pipe(less())
        .pipe(gulp.dest(config.bowerPath + 'fontawesome/css/'))
        .pipe(rename({ suffix: '.min' }))
        .pipe(minifyCSS())
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest(config.bowerPath + 'fontawesome/css/'));
});

gulp.task('bower:bootstrap-less', function () {
    return gulp.src(config.bowerPath + 'bootstrap-less-only/less/bootstrap.less')
        .pipe(sourcemaps.init())
        .pipe(less())
        .pipe(gulp.dest(config.bowerPath + 'bootstrap-less-only/css/'))
        .pipe(rename({ suffix: '.min' }))
        .pipe(minifyCSS())
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest(config.bowerPath + 'bootstrap-less-only/css/'));
});
//#endregion


//#region app tasks
gulp.task('app', function (cb) {
    runSequence(['app:less', 'app:views', 'app:img', 'app:fonts', 'app:bin', 'app:config'], 'app:index', 'app:config:js:restore', ['app:zip', 'app:config:js:clean'], cb);
});

gulp.task("app:zip", function () {
    return gulp.src(config.packagePath + '**')
        .pipe(zip(config.packageZipName))
        .pipe(gulp.dest(config.packageZipPath));
});
//#endregion

//#region app sub tasks
gulp.task('app:less', function () {
    return gulp.src(config.appLessPath + 'style.less')
        .pipe(sourcemaps.init())
        .pipe(less())
        .pipe(rename('style.css'))
        .pipe(gulp.dest('./css'))
        .pipe(rename({ suffix: '.min' }))
        .pipe(minifyCSS())
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest('./css'));
});

gulp.task('app:index', function () {
    var prms = null;
    if (!config.useMinify) {
        prms = {
            css: ['concat', rev()],
            libjs: [rev()],
            appjs: [rev()],
            html: [minifyHtml({ empty: true, conditionals: true, spare: true, quotes: true })]
        }
    } else {
        prms = {
            css: [minifyCSS(), 'concat', rev()],
            libjs: [uglify(), rev()],
            appjs: [uglify(), rev()],
            html: [minifyHtml({ empty: true, conditionals: true, spare: true, quotes: true })]
        }
    }
    return gulp.src('./index.html')
        .pipe(usemin(prms))
        .pipe(gulp.dest(config.packagePath));
});

gulp.task('app:views', function () {
    return gulp.src('./views/**')
        .pipe(minifyHtml({ empty: true, conditionals: true, spare: true, quotes: true }))
        .pipe(gulp.dest(config.packagePath + 'views/'));
});

gulp.task('app:img', function () {
    return gulp.src('./img/**')
        .pipe(gulp.dest(config.packagePath + 'img/'));
});

gulp.task('app:fonts', function () {
    return gulp.src(config.bowerPath + 'fontawesome/fonts/**')
        .pipe(gulp.dest(config.packagePath + 'fonts/'));
});

gulp.task('app:bin', function () {
    return gulp.src('./bin/**')
        .pipe(gulp.dest(config.packagePath + 'bin/'));
});

gulp.task('app:config', function (cb) {
    runSequence(['app:config:js', 'app:config:web', 'app:config:packages'], cb);
});

gulp.task('app:config:js', function (cb) {
    runSequence('app:config:js:copy', 'app:config:js:replace', cb);
});

gulp.task('app:config:js:replace', function () {
    return gulp.src('./js/config/config.js')
        .pipe(replace(/@REVISION/, Date.now()))
        .pipe(gulp.dest('./js/config/'));
});

gulp.task('app:config:js:copy', function () {
    return gulp.src('./js/config/config.js')
        .pipe(rename('config.original.js'))
        .pipe(gulp.dest('./js/config/'));
});

gulp.task('app:config:js:restore', function () {
    return gulp.src('./js/config/config.original.js')
        .pipe(rename('config.js'))
        .pipe(gulp.dest('./js/config/'));
});

gulp.task('app:config:js:clean', function () {
    return gulp.src('./js/config/config.original.js')
        .pipe(clean());
});

gulp.task('app:config:web', function () {
    return gulp.src('./Web.config')
       .pipe(gulp.dest(config.packagePath));
});

gulp.task('app:config:packages', function () {
    return gulp.src('./packages.config')
       .pipe(gulp.dest(config.packagePath));
});
//#endregion
