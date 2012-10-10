@echo off

call "%VS100COMNTOOLS%vsvars32.bat"

msbuild.exe tools\package.proj /t:FullClean

IF NOT ERRORLEVEL 0 EXIT /B %ERRORLEVEL%