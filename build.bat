@echo off
setlocal
chcp 65001 >nul

set "SCRIPT_DIR=%~dp0"
set "DOTNET_EXE=%SCRIPT_DIR%tools\dotnet\dotnet.exe"
set "SLN_FILE=%SCRIPT_DIR%BTBatteryMonitor.sln"

if not exist "%DOTNET_EXE%" (
    echo [Error] Local .NET SDK not found.
    echo Please run setup_env.bat first to install the local build environment.
    pause
    exit /b 1
)

echo ========================================================
echo  BTBatteryMonitor - Building Project (Debug)
echo ========================================================
echo.

"%DOTNET_EXE%" build "%SLN_FILE%" -c Debug %*

if %ERRORLEVEL% equ 0 (
    echo.
    echo [Success] Build succeeded!
) else (
    echo.
    echo [Error] Build failed with error code %ERRORLEVEL%.
    exit /b %ERRORLEVEL%
)

endlocal
