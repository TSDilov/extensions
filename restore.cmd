@echo off
<<<<<<< HEAD
SETLOCAL

set _args=%*
if "%~1"=="-?" set _args=-help
if "%~1"=="/?" set _args=-help

powershell -ExecutionPolicy ByPass -NoProfile -command "& """%~dp0eng\build.ps1""" -restore %_args%"
exit /b %ErrorLevel%
=======
powershell -ExecutionPolicy ByPass -NoProfile -command "& """%~dp0eng\common\Build.ps1""" -restore %*"
exit /b %ErrorLevel%
>>>>>>> 8d8547bffdfbb7a658721bec13b9269774ab215b
