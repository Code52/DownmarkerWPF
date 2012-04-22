@ECHO OFF

SET build="%~dp0\build.cmd"
SET copy="copy"

CALL rmdir "%~dp0artifacts\" /s /q

:: Pass the configuration parameter and all other parameters
CALL %build% Release "Mixed Platforms"

:: get build output and copy out to root 
CALL xcopy "%~dp0src\MarkPad\bin\Release\*" "%~dp0artifacts\" /s /e /Y

CALL "C:\Program Files\7-Zip\7z.exe" a -tzip "%~dp0artifacts\MarkPad.zip" "%~dp0artifacts\*"

IF NOT ERRORLEVEL 0 EXIT /B %ERRORLEVEL%
