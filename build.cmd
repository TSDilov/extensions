<<<<<<< HEAD
@ECHO OFF

SET _args=%*
IF "%~1"=="-?" SET _args=-help
IF "%~1"=="/?" SET _args=-help

IF ["%_args%"] == [""] (
    :: Perform restore and build, IF no args are supplied.
    SET _args=-restore -build
)

powershell -ExecutionPolicy ByPass -NoProfile -command "& """%~dp0eng\build.ps1""" %_args%"
EXIT /b %ERRORLEVEL%
=======
@echo off
powershell -ExecutionPolicy ByPass -NoProfile -command "& """%~dp0eng\common\Build.ps1""" -build -restore -pack %*"
exit /b %ErrorLevel%
>>>>>>> 8d8547bffdfbb7a658721bec13b9269774ab215b
