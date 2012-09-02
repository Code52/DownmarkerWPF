@ECHO OFF

SET msbuild="%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"

IF '%1'=='' (SET configuration=Debug) ELSE (SET configuration=%1)
IF '%2'=='' (SET platform="x86") ELSE (SET platform=%2)

:: Build the solution. Override the platform to account for running
:: from Visual Studio Tools command prompt (x64). Log quietly to the 
:: console and verbosely to a file.
%msbuild% src/MarkPad.sln /t:Rebuild /nologo /property:Platform=%platform% /property:Configuration=%configuration% /verbosity:minimal /flp:verbosity=diagnostic

IF NOT ERRORLEVEL 0 EXIT /B %ERRORLEVEL%