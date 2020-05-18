# Nibriboard

> An infinite whiteboard for recording those big ideas.

Nibriboard is a product of an initial idea that I thought I could get done in about 3 weeks in February 2017, but some considerable time later I'm still going strong, but not quite there yet :P

![A screenshot of what the interface looked like in July 2018.](https://raw.githubusercontent.com/sbrl/Nibriboard/master/screenshots/2018-July.png)

## Features
 - Infinite whiteboard that can be panned around (limited only by your hard drive space and floating-point number limits)
 - Real-time multi-user support
 - User input is simplified to reduce disk space / bandwidth

### Todo
 - Authentication + user accounts
 - Multi-whiteboard support (the backend supports it - it's just not exposed correctly yet)
 - Improve client-side multi-user cursor support (disconnected users' cursors don't disappear correctly)
 - Improve colour palette
 - Improve left-hand user interface panel
 - Make debug info hidden by default + toggleable
 - Implement WebSockets gzip support in the [GlidingSquirrel](https://git.starbeamrainbowlabs.com/sbrl/GlidingSquirrel)


## Getting Started

Nibriboard is _not_ ready for general consumption just yet. It's got no authentication yet for one! If you'd like to play around with it, then you'll need the following:

 - git
 - Node.JS + npm
 - mono if you're on Linux / macOS / etc. (See below for Ubuntu 20.04 instructions)
 
Once you've verified that you've got the above installed and in your PATH, simply run `msbuild` (or `dotnet msbuild`, or `xbuild`) in the root of this repository to build Nibriboard.

Windows users may need to use a _Visual Studio Command Prompt_ if the `msbuild` command isn't in your `PATH` environment variable.

Note that if you're intending to use Nibriboard over the internet or an untrusted network, you should proxy it behind nginx to provide TLS, as Nirbiboard doesn't handle HTTPS on it's own.

### Getting Started with Ubuntu 20.04 (and probably others)

This may or may not work with Ubuntu's provided mono packages, though was tested with those provided by mono-project. Note: if for whatever reason you don't have `mono-roslyn` (which provides `msbuild`), use `mono-xbuild` and `xbuild` in their respective places.

* packages: `sudo apt install mono-devel mono-roslyn`
* nuget for dependencies: `wget https://dist.nuget.org/win-x86-commandline/latest/nuget.exe` (note: `msbuild -restore` did not work)
* `mono nuget.exe restore`
* `msbuild`

Finally, run with `mono Nibriboard/bin/Debug/Nibriboard.exe`

> *If you have any trouble with nuget and the JSON dependencies, there's a package you can try installing: `libnewtonsoft-json-cil-dev`*

## Credits
 - Main code - [Starbeamrainbowlabs](https://starbeamrainbowlabs.com/)
 - HTTP + WebSockets implementation: my library [GlidingSquirrel](https://github.com/sbrl/GlidingSquirrel) (bug reports welcome!)
 - Client-Side Libraries:
 	 - [favicon-notification](https://www.npmjs.com/package/favicon-notification)
 	 - [event-emitter-es6](https://www.npmjs.com/package/event-emitter-es6) - For ES6 class event generation and consumption
 	 - [color](https://www.npmjs.com/package/color) - For colour generation and manipulation
 	 - [cuid](https://www.npmjs.com/package/cuid) - For Id generation (Also available for .NET - todo look into using that here too)
 	 - [fps-indicator](https://www.npmjs.com/package/fps-indicator) - The fps graph in the top right
 	 - [pan-zoom](https://www.npmjs.com/package/pan-zoom) - Handles panning & zooming delta calculations
     - [keycode](https://www.npmjs.com/package/keycode) - Key code -> key name translation
 	 - [acorn](https://www.npmjs.com/package/acorn) - Syntax checking etc. during `npm build .`
 	 - [webpack](https://webpack.js.org/) - Building and packing the client-sided js into a single bundle
 - Images:
     - [Transparent Square Tiles](https://www.toptal.com/designers/subtlepatterns/transparent-square-tiles/) from [subtlepatterns.com](https://subtlepatterns.com/)
     - [Graph Paper](https://www.toptal.com/designers/subtlepatterns/graphy/) from [subtlepatterns.com](https://subtlepatterns.com)
     - Icons:
         - [OpenIconic](https://useiconic.com/open) - brush, move -> pan, sun -> point, eraser -> delete
 - Future reference: Libraries I am considering
	 - [Paper.js](http://paperjs.org/) - Client-side rendering
 - [IotWeb](http://sensaura.org/pages/tools/iotweb/) - Underlying HTTP / WebSocket server


## Useful Links
 - MSBuild:
	 - [`Exec` task](https://docs.microsoft.com/en-gb/visualstudio/msbuild/exec-task)
	 - [Dynamic wildcarded embedded resources](https://ayende.com/blog/4446/how-to-setup-dynamic-groups-in-msbuild-without-visual-studio-ruining-them)
