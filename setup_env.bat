@echo off
setlocal
chcp 65001 >nul

echo ========================================================
echo  BTBatteryMonitor - Local Build Environment Setup
echo ========================================================
echo.

set "SCRIPT_DIR=%~dp0"
set "TOOLS_DIR=%SCRIPT_DIR%tools"
set "DOTNET_DIR=%TOOLS_DIR%\dotnet"
set "INSTALLER_SCRIPT=%TOOLS_DIR%\dotnet-install.ps1"

if not exist "%TOOLS_DIR%" mkdir "%TOOLS_DIR%"

if exist "%DOTNET_DIR%\dotnet.exe" (
    echo [Info] .NET SDK is already installed: %DOTNET_DIR%
    goto :VERIFY
)

echo [1/3] Downloading dotnet-install.ps1...
powershell -NoProfile -ExecutionPolicy Bypass -Command "[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile '%INSTALLER_SCRIPT%'"
if %ERRORLEVEL% neq 0 (
    echo [Error] Failed to download installer script.
    exit /b %ERRORLEVEL%
)

echo [2/3] Installing .NET 8 SDK locally into tools\dotnet (NoPath)...
powershell -NoProfile -ExecutionPolicy Bypass -Command "& '%INSTALLER_SCRIPT%' -Channel 8.0 -InstallDir '%DOTNET_DIR%' -NoPath"
if %ERRORLEVEL% neq 0 (
    echo [Error] Failed to install .NET 8 SDK.
    exit /b %ERRORLEVEL%
)

if exist "%INSTALLER_SCRIPT%" del "%INSTALLER_SCRIPT%"

:VERIFY
echo.
echo [3/3] Verifying local .NET SDK version...
if exist "%DOTNET_DIR%\dotnet.exe" (
    "%DOTNET_DIR%\dotnet.exe" --version
    echo.
    echo ========================================================
    echo  Setup completed successfully!
    echo  SDK Location: %DOTNET_DIR%
    echo ========================================================
) else (
    echo [Error] dotnet.exe was not found.
    exit /b 1
)

endlocal
