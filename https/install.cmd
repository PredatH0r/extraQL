@echo off

rem If you have Microsoft Windows SDK installed, you can create your own self-signed server authentication certificate by using the "-create" parameter
rem In the password dialog click the "No Password" button. 

if not "%1" == "-create" goto install

:create
makecert.exe -r -a sha1 -n CN=localhost -sky exchange -pe -b 01/01/2000 -e 01/01/2100 -l http://sourceforge.net/projects/extraql/ -eku 1.3.6.1.5.5.7.3.1 -sv localhost.pvk localhost.cer
if errorlevel 1 goto fail
pvk2pfx.exe -pvk localhost.pvk -spc localhost.cer -pfx localhost.pfx
if errorlevel 1 goto fail
del localhost.pvk

:install
rem Install the certificate in the trusted root storage (required, because it has no root. The certificate itself cannot be used to sign other certificates, so this is no security risk):
certmgr.exe -add localhost.cer -s -r localmachine root
if errorlevel 1 goto fail
goto eof

:fail
pause

:eof