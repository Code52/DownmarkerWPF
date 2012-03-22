@ECHO OFF

SET build="%~dp0\build.bat"
SET copy="copy"

:: Pass the configuration parameter and all other parameters
CALL %build% Release "Mixed Platforms"

CALL xcopy "%~dp0src\MarkPad\bin\Release\*" "%~dp0artifacts\" /s /e /Y

IF NOT ERRORLEVEL 0 EXIT /B %ERRORLEVEL%
