@echo off
setlocal
chcp 65001 >nul

set "SCRIPT_DIR=%~dp0"
set "DOTNET_EXE=%SCRIPT_DIR%tools\dotnet\dotnet.exe"
set "PROJ_FILE=%SCRIPT_DIR%src\BTBatteryMonitor\BTBatteryMonitor.csproj"

if not exist "%DOTNET_EXE%" (
    echo [Error] Local .NET SDK not found.
    echo Please run setup_env.bat first to install the local build environment.
    pause
    exit /b 1
)

echo ========================================================
echo  BTBatteryMonitor - Starting Application
echo ========================================================
echo.

"%DOTNET_EXE%" run --project "%PROJ_FILE%" %*

endlocal
