@echo off

set srcFolder=..\src
set publishFolder=..\publish

for %%f in (
  DotMake.DocfxPlus.Cli
) do (
  setlocal EnableDelayedExpansion
  set projectName=%%f
  
  dotnet pack %srcFolder%\!projectName!\!projectName!.csproj --configuration Release --output %publishFolder%
  
  if %ERRORLEVEL% EQU 0 (
    echo:
    echo *************
    echo Generated "!projectName!.X.X.X.nupkg" should be found in "%publishFolder%" folder.
    echo *************
    echo:
  )
)

pause