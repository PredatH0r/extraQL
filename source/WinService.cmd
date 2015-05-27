@echo off
if "%1" == "install" goto install
if "%1" == "delete" goto uninstall
if "%1" == "start" goto start
if "%1" == "stop" goto stop
if "%1" == "restart" goto restart

echo Usage: %~nx0 { install ^| delete ^| start ^| stop ^| restart }
goto eof

:install
sc create extraQL DisplayName= "extraQL" binPath= "d:\sources\quakelive\extraql\bin\debug\extraql.exe -service" start= auto
goto eof

:uninstall
sc delete extraQL
goto eof

:start
net start extraQL
goto eof

:stop
net stop extraQL
goto eof

:restart
net stop extraQL
net start extraQL
goto eof

:eof