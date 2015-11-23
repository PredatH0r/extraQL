@echo off
cd /d %~dp0
set steamcmd=d:\games\steamcmd\steamcmd.exe
set src=%cd%\..\source\bin\Debug
set dst=%cd%\content

rmdir /s /q %dst%
if errorlevel 1 goto error

mkdir %dst%
copy %src%\extraQL.exe %dst%
if errorlevel 1 goto error
copy %src%\*.dll %dst%
if errorlevel 1 goto error
copy %src%\steam_appid.txt %dst%
if errorlevel 1 goto error

mkdir %dst%\de
copy %src%\de\* %dst%\de
if errorlevel 1 goto error

mkdir %dst%\ru
copy %src%\ru\* %dst%\ru
if errorlevel 1 goto error

mkdir %dst%\scripts
copy ..\source\scripts\* %dst%\scripts
if errorlevel 1 goto error
del %dst%\scripts\_*.js

%steamcmd% +login hbeham 5Chd1995st +workshop_build_item %cd%\extraQL.vdf +quit
if errorlevel 1 goto error
goto:eof

:error
pause