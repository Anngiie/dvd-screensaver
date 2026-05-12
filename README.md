# DVD Screensaver

The classic bouncing DVD logo screensaver — built with HTML, CSS & JavaScript.

## Demo

**[Live demo →](https://anngiie.github.io/dvd-screensaver/)**

Or open `index.html` in any browser. Works on all screen sizes.

## How it works

The DVD logo moves diagonally across a dark background, bounces off the edges, and changes color with every wall hit. The animation responds to window resizing so it works on any display resolution.

## Run as a screensaver (.scr)

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```sh
git clone https://github.com/Anngiie/dvd-screensaver.git
cd dvd-screensaver
build.bat
```

This produces `dvd.scr`. Right-click it and choose **Install** to set it as your screensaver, or drop it into `C:\Windows\System32`.

## Run it in the browser

```sh
git clone https://github.com/Anngiie/dvd-screensaver.git
cd dvd-screensaver
open index.html
```
