@ECHO OFF

SET build=%~dp0\build.bat

:: Pass the configuration parameter and all other parameters
CALL %build% Release %*

IF NOT ERRORLEVEL 0 EXIT /B %ERRORLEVEL%