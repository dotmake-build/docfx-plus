@echo off

set srcFolder=..\docs
set publishFolder=..\docs\_site
set publishOfflineFolder=..\docs\_site_offline

dotnet tool uninstall -g docfx-plus
dotnet tool update -g docfx-plus --configfile ../src/nuget.config

rmdir /S /Q "%publishFolder%"
docfx-plus %srcFolder%\docfx.json
if %ERRORLEVEL% EQU 0 (
  echo:
  echo *************
  echo Generated "Api Docs WebSite" should be found in "%publishFolder%" folder.
  echo *************
  echo:
)

rmdir /S /Q "%publishOfflineFolder%"
docfx-plus build %srcFolder%\docfx.json -m _enableOfflineMode -o "%publishOfflineFolder%"
if %ERRORLEVEL% EQU 0 (
  echo:
  echo *************
  echo Generated "Api Docs Offline" should be found in "%publishOfflineFolder%" folder.
  echo *************
  echo:
)

docfx-plus serve "%publishFolder%"

@pause