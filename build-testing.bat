@ECHO OFF

SET build="%~dp0\build.bat"
SET xunit="%~dp0\tools\xunit\xunit.console.clr4.exe"

:: Pass the configuration parameter and all other parameters
CALL %build% Testing "Any CPU"

::CALL %xunit% "%~dp0src\MarkPad.Tests\bin\Testing\MarkPad.Tests.dll"

IF NOT ERRORLEVEL 0 EXIT /B %ERRORLEVEL%
