@ECHO OFF

SET build="%~dp0\build.cmd"
SET xunit="%~dp0\tools\xunit\xunit.console.clr4.x86.exe"

CALL %build% Testing "x86"

CALL %xunit% "%~dp0src\MarkPad.Tests\bin\Testing\MarkPad.Tests.dll"

IF NOT ERRORLEVEL 0 EXIT /B %ERRORLEVEL%