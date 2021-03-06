## Connexions Open Travel

This repository contains code that can be used as a reference for implementation of the Connexions travel API (CAPI).

## Building and Running

To fully build and run this code, you need the following tools:

* Node.js 6.9.5 or higher; older versions may work but are not tested.
* Visual Studio 2017 for Windows is the only tested platform.  Older versions of Visual Studio (such as 2015) will not work.  Visual Studio for Mac and Visual Studio Code *may* work but are not tested.
* Windows 7 or higher for the ASP.NET Core "Kestrel" web server.  Windows 8 or higher for IIS or IIS Express.

There are no known dependencies on extra extensions for Visual Studio; it should work with a clean installation.

The free Community Edition of Visual Studio is worth considering if you are on a team of 6 or less, or make your contributions available as open source.
 
## Hosting Requirements

Windows Server 2012 or higher.
Linux and macOS may work as this code is built using .NET Standard and .NET Core, which do not have any intrinsic Windows dependencies.

## License, Contributing

This repository is covered by the [Apache 2.0 license](LICENSE).
You're welcome to copy or fork this code for use on your own site, even if that site is closed source and commercial, as long as you remain in compliance with the aforementioned license.
We can accept pull requests from any source where an appropriate contractual framework exists.

## Technologies Used

The following is a list of the major technologies employed by this application.

* Node.js
* Node Package Manager (NPM)
* Gulp
* Webpack
* React
* TypeScript
* ECMAScript 6th Edition (ES6)
* Web Sockets
* Visual Studio 2017
* C# 7.0
* .NET Standard
* .NET Core

## Notable Issues

There are some design or technical issues that should be resolved before this project can be considered "stable":

* Dependency on `CoreCompat.System.Drawing` -
This library provides emulation of the .NET Classic "System.Drawing" namespace.  It's the only .NET Core compatible image library that's close to production ready
A strong candidate for replacement is [ImageSharp](https://github.com/JimBobSquarePants/ImageSharp), but the API is not yet stable enough for distribution via NuGet.
* Missing Angular 2 WebUI Project -
A React-based project is included but the popular Angular framework has no representation.
Angular 2 could potentially leverage much of the TypeScript code already written for React.
* No WebSocket Fallback -
Although all modern browsers support WebSockets, some old proxy servers may break the feature.
There currently isn't a mechanism in place to use an alternative connectivity approach if a WebSocket connection can't be established.