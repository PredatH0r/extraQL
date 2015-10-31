@echo off
cd /d %~dp0
set zipper="c:\program files\7-Zip\7z.exe"
set abs=%cd%

rem package binaries
call :CodeSigning
cd %abs%
if errorlevel 1 goto error
if exist extraQL.zip del extraQL.zip
if errorlevel 1 goto error
mkdir extraQL
mkdir extraQL\scripts
mkdir extraQL\images
mkdir extraQL\https
xcopy "%abs%\bin\Debug\extraQL.exe" extraQL\ >nul
xcopy "%abs%\bin\Debug\steam_api.dll" extraQL\ >nul
xcopy "%abs%\bin\Debug\steam_appid.txt" extraQL\ >nul
xcopy "%abs%\scripts" extraQL\scripts >nul
xcopy /s "%abs%\images" extraQL\images >nul
xcopy /s "%abs%\https" extraQL\https >nul
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
set files=extraQL.exe
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

