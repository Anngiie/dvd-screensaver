@echo off
echo Building DVD Screensaver...

dotnet publish screensaver\DvdScreensaver.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -p:DebugSymbols=false -o publish

if %ERRORLEVEL% NEQ 0 (
    echo Build failed!
    exit /b %ERRORLEVEL%
)

copy /Y publish\dvd.exe dvd.scr
rmdir /s /q publish
echo.
echo Done! dvd.scr is ready.
echo Right-click dvd.scr ^> Install to use it as your screensaver.
