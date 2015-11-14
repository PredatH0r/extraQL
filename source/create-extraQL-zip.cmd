@echo off
setlocal EnableExtensions

cd /d %~dp0
set zipper="c:\program files\7-Zip\7z.exe"
set abs=%cd%

rem package binaries
call :CodeSigning
cd %abs%
if errorlevel 1 goto error
if exist extraQL.zip del extraQL.zip
if errorlevel 1 goto error
rmdir /s /q extraQL
mkdir extraQL
mkdir extraQL\scripts
mkdir extraQL\images
mkdir extraQL\de
echo copying...
xcopy "%abs%\bin\Debug\extraQL.exe" extraQL\
xcopy "%abs%\bin\Debug\de\extraQL.resources.dll" extraQL\de\
xcopy "%abs%\bin\Debug\steam_api.dll" extraQL\
xcopy "%abs%\bin\Debug\steam_appid.txt" extraQL\
xcopy "%abs%\scripts" extraQL\scripts
xcopy /s "%abs%\images" extraQL\images

%zipper% a -tzip extraQL.zip "extraQL" -x!extraQL\scripts\rosterGroup.usr.js
if errorlevel 1 goto error
rmdir /s /q extraQL
goto :success

:CodeSigning
rem -----------------------------
rem If you want to digitally sign the generated .exe and .dll files, 
rem you need to have your code signing certificate installed in the Windows certificate storage
rem -----------------------------
set signtool="C:\Program Files\Microsoft SDKs\Windows\v6.0A\Bin\signtool.exe"
set oldcd=%cd%
cd %abs%\bin\Debug
set files=extraQL.exe de\extraQL.resources.dll
%signtool% sign /a /t "http://timestamp.comodoca.com/authenticode" %files%
goto :eof

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
goto :eof

