@echo off

set projectName=DotMake.SvgSprite
set srcFolder=..\src
set publishFolder=..\docs\api

dotnet build %srcFolder%\HelpBuilder\%projectName%.shfbproj --configuration Release --output %publishFolder%

if %ERRORLEVEL% EQU 0 (
  echo:
  echo *************
  echo Generated "Api Docs WebSite" should be found in "%publishFolder%" folder.
  echo *************
  echo:
)

pause