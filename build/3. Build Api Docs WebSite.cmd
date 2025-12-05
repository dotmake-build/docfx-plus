@echo off

set srcFolder=..\docs
set publishFolder=..\docs\_site

dotnet tool uninstall -g docfx-plus
dotnet tool update -g docfx-plus --configfile ../src/nuget.config
docfx-plus %srcFolder%\docfx.json --serve

@echo off
if %ERRORLEVEL% EQU 0 (
  echo:
  echo *************
  echo Generated "Api Docs WebSite" should be found in "%publishFolder%" folder.
  echo *************
  echo:
)
@echo on

@pause