:: extract latest docfx templates and then merge them
:: merged folder is useful for comparing differences at once
:: between our template and original templates

@echo off
setlocal enabledelayedexpansion

set "templatesToExtract=common default modern statictoc"
:: inheritance: common -> default -> modern
set "modernTemplates=common default modern"
:: inheritance: common -> default -> statictoc
set "statictocTemplates=common default statictoc"
set "destFolder=.\"

dotnet tool update --global docfx

:: Loop through and forcefully delete all internal subdirectories quietly
for /D %%d in ("%destFolder%\*") do rmdir /S /Q "%%d"

for %%t in (%templatesToExtract%) do (

    set "template=%%t"

    echo ----------------------------------------------------
    echo Exporting template: !template!

    docfx template export !template! -o %destFolder%

    if !errorlevel! neq 0 (
        echo [ERROR] Command failed for !template! with exit code !errorlevel!. Halting loop execution.
        goto :ExitLoop
    )
)

for %%t in (%modernTemplates%) do (
    set "template=%%t"

    robocopy "%destFolder%\!template!" "%destFolder%\modern-merged" /E /IS /NFL
)

for %%t in (%statictocTemplates%) do (
    set "template=%%t"

    robocopy "%destFolder%\!template!" "%destFolder%\statictoc-merged" /E /IS /NFL
)

:ExitLoop
echo ----------------------------------------------------
echo Script finished or stopped.
pause