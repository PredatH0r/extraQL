@echo off
cd /d %~dp0
set zipper="c:\program files\7-Zip\7z.exe"
set abs=%cd%

rem package binaries
if exist extraQL.zip del extraQL.zip
if exist source.zip del source.zip
%zipper% a -tzip extraQL.zip "%abs%\bin\Debug\extraQL.exe" images scripts
if errorlevel 1 goto error

rem package source
%zipper% a -tzip source.zip * -x!*.zip -x!*.user -x!bin -x!obj -x!data
if errorlevel 1 goto error

rem add source package to binary package
%zipper% a -tzip extraQL.zip source.zip
if errorlevel 1 goto error
del source.zip

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