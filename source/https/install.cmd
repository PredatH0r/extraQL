@echo off

rem If you have Microsoft Windows SDK installed, you can create your own self-signed server authentication certificate by using the "-create" parameter
rem In the password dialog click the "No Password" button. 

if "%1" == "-gui" goto gui
if not "%1" == "-create" goto install
certmgr.exe >nul 2>nul
if errorlevel 9009 goto gui

:create
makecert.exe -r -a sha1 -n CN=localhost -sky exchange -pe -b 01/01/2000 -e 01/01/2100 -l http://sourceforge.net/projects/extraql/ -eku 1.3.6.1.5.5.7.3.1 -sv localhost.pvk localhost.cer
if errorlevel 1 goto fail
rem On Win XP/2003 the .NET crypto API fails when trying to load a pfx file without a password, so set it to "extraQL" here
pvk2pfx.exe -f -pvk localhost.pvk -spc localhost.cer -pfx localhost.pfx -po "extraQL"
if errorlevel 1 goto fail
del localhost.pvk

:install
rem Install the certificate in the trusted root storage (required, because it has no root. The certificate itself cannot be used to sign other certificates, so this is no security risk):
certmgr.exe -add localhost.cer -s -r localmachine root
if not errorlevel 1 goto eof
:gui
echo In the next step, a dialog with certificate details will be opened:
echo 1) Click the "Install Certificate..." button
echo 2) If asked for a storage location, select "Local Machine", then "Next"
echo 3) Select "Place all certificates in the following store", then Browse
echo 4) Select "Trusted Root Certification Authorities", then OK
echo 5) Next, then Finish
pause
%~dp1\localhost.cer
goto eof

:fail
echo Please make sure you run this script as Administrator, otherwise it will fail.
pause

:eof