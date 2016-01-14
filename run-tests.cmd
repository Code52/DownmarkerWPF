@ECHO OFF

SET build="%~dp0\build.cmd"
SET xunit="%~dp0\tools\xunit\xunit.console.clr4.x86.exe"

CALL %build% Release "x86"

CALL %xunit% "%~dp0src\MarkPad.Tests\bin\Release\MarkPad.Tests.dll"
CALL %xunit% "%~dp0src\MarkPad.UITests\bin\Release\MarkPad.UITests.dll"

IF NOT ERRORLEVEL 0 EXIT /B %ERRORLEVEL%
