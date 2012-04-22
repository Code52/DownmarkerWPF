@ECHO OFF

CALL rmdir "%~dp0artifacts\" /s /q

SET msbuild="%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"

SET configuration=Release
SET platform="Mixed Platforms"

:: Build the solution. Override the platform to account for running
:: from Visual Studio Tools command prompt (x64). Log quietly to the 
:: console and verbosely to a file.
%msbuild% src/MarkPad.Setup.sln /nologo /property:Platform=%platform% /property:Configuration=%configuration% /verbosity:minimal /flp:verbosity=diagnostic

:: get build output and copy out to root 
CALL xcopy "%~dp0src\MarkPad.Setup\bin\Release\*.msi" "%~dp0artifacts\" /s /e /Y

IF NOT ERRORLEVEL 0 EXIT /B %ERRORLEVEL%