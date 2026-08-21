:: extract latest docfx templates and then merge them
:: merged folder is useful for comparing differences at once
:: between our template and original templates

@echo off
setlocal enabledelayedexpansion

:: inheritance: common -> default -> modern
set "templates=common default modern"
set "destFolder=.\"

dotnet tool update --global docfx

:: Loop through and forcefully delete all internal subdirectories quietly
for /D %%d in ("%destFolder%\*") do rmdir /S /Q "%%d"

:: Loop through all .dll files in the folder
for %%t in (%templates%) do (

    set "template=%%t"

    echo ----------------------------------------------------
    echo Exporting template: !template!

    docfx template export !template! -o %destFolder%

    if !errorlevel! neq 0 (
        echo [ERROR] Command failed for !template! with exit code !errorlevel!. Halting loop execution.
        goto :ExitLoop
    )
    
    robocopy "%destFolder%\!template!" "%destFolder%\merged" /E /IS
)

:ExitLoop
echo ----------------------------------------------------
echo Script finished or stopped.
pause