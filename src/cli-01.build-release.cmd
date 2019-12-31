echo off
set ui-interaction=yes
set batch-build=yes
::
rmdir /Q /S "%~dp0..\nuget-output"
dotnet build "%~dp0eon-lib\eon-lib.sln" --verbosity minimal --no-incremental --force --configuration=Release -p:Eon-PublishNuGetPackages="True"
if /I [%ui-interaction%] equ [no] (timeout /t 0) else (pause)