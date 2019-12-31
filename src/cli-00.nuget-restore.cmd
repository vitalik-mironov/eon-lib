echo off
set ui-interaction=yes
::
dotnet restore "%~dp0eon-lib\eon-lib.sln" --verbosity normal
if /I [%ui-interaction%] equ [no] (timeout /t 0) else (pause)