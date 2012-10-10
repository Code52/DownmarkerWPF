@ECHO OFF

call "%VS100COMNTOOLS%vsvars32.bat"

msbuild.exe tools\package.proj /p:Configuration=Release

IF NOT ERRORLEVEL 0 EXIT /B %ERRORLEVEL%
