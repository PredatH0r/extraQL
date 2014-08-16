@echo off
cd /d %~dp0
set zipper="c:\program files\7-Zip\7z.exe"
set abs=%cd%

rem package binaries
if exist extraQL.zip del extraQL.zip
if errorlevel 1 goto error
mkdir extraQL
mkdir extraQL\scripts
mkdir extraQL\images
xcopy "%abs%\bin\Debug\extraQL.exe" extraQL\ >nul
xcopy "%abs%\scripts" extraQL\scripts >nul
xcopy /s "%abs%\images" extraQL\images >nul
%zipper% a -tzip extraQL.zip "extraQL" -x!extraQL\scripts\!ndex.txt
if errorlevel 1 goto error
rmdir /s /q extraQL

:success
echo.
echo SUCCESS !!!
echo.
pause
goto :eof

:error
echo.
echo FAILED !!!
echo.
pause