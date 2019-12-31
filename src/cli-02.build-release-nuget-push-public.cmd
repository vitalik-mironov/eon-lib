echo off
cls
set /p api-key="Enter ApiKey: "
cd ..\nuget-output
nuget push *.nupkg %api-key% -Source https://api.nuget.org/v3/index.json
echo =====
echo Done.
pause