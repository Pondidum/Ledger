var gulp = require("gulp");
var shell = require("gulp-shell");
var args = require('yargs').argv;
var fs = require("fs");
var assemblyInfo = require('gulp-dotnet-assembly-info');
var rename = require('gulp-rename');
var msbuild = require('gulp-msbuild');
var xunit =require('gulp-xunit-runner');
var debug = require('gulp-debug');

var project = JSON.parse(fs.readFileSync("./package.json"));

var config = {
  name: project.name,
  version: project.version,
  mode: args.mode || "Debug",
  commit: process.env.APPVEYOR_REPO_COMMIT || "0",
  buildNumber: process.env.APPVEYOR_BUILD_VERSION || "0",
  output: "./build/deploy"
}

gulp.task("default", [ "restore", "version", "compile", "test", "pack" ]);

gulp.task('restore', function() {
  return gulp
    .src(config.name + '.sln', { read: false })
    .pipe(shell('"./tools/nuget/nuget.exe" restore -configFile nuget.config '));
});

gulp.task('version', function() {
  return gulp
    .src(config.name + '/Properties/AssemblyVersion.base')
    .pipe(rename("AssemblyVersion.cs"))
    .pipe(assemblyInfo({
      version: config.version,
      fileVersion: config.version,
      description: "Build: " +  config.buildNumber + ", Sha: " + config.commit
    }))
    .pipe(gulp.dest('./' + config.name + '/Properties'));
});

gulp.task('compile', [ "restore", "version" ], function() {
  return gulp
    .src(config.name + ".sln")
    .pipe(msbuild({
      targets: [ "Clean", "Rebuild" ],
      configuration: config.mode,
      toolsVersion: 14.0,
      errorOnFail: true,
      stdout: true,
      verbosity: "minimal"
    }));
});

gulp.task('test', [ "compile" ], function() {
  return gulp
    .src(['**/bin/*/*.Tests.dll'], { read: false })
    .pipe(xunit({
      executable: './tools/xunit/xunit.console.exe',
      options: {
        quiet: true
      }
    }));
});

gulp.task('pack', [ 'test' ], function () {
  return gulp
    .src('**/*.nuspec', { read: false })
    .pipe(rename({ extname: ".csproj" }))
    .pipe(shell([
      '"tools/nuget/nuget.exe" pack <%= file.path %> -version <%= version %> -prop configuration=<%= mode %> -o <%= output%>'
    ], {
      templateData: config
    }));
});
