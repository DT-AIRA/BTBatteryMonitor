@echo off
setlocal
chcp 65001 >nul

set "SCRIPT_DIR=%~dp0"
set "TOOLS_DIR=%SCRIPT_DIR%tools"
set "DOTNET_EXE=%TOOLS_DIR%\dotnet\dotnet.exe"
set "INNO_DIR=%TOOLS_DIR%\inno"
set "ISCC_EXE=%INNO_DIR%\ISCC.exe"
set "PROJ_FILE=%SCRIPT_DIR%src\BTBatteryMonitor\BTBatteryMonitor.csproj"
set "ISS_FILE=%SCRIPT_DIR%installer\BTBatteryMonitor.iss"
set "PUBLISH_DIR=%SCRIPT_DIR%publish"
set "PUBLISH_FILES_DIR=%SCRIPT_DIR%publish_files"
set "DIST_DIR=%SCRIPT_DIR%dist"

if not exist "%DOTNET_EXE%" (
    echo [Error] Local .NET SDK not found. Please run setup_env.bat first.
    exit /b 1
)

echo ========================================================
echo  BTBatteryMonitor - Publish and Package Pipeline
echo ========================================================
echo.

echo [1/4] Building Self-Contained Single EXE (Release x64)...
"%DOTNET_EXE%" publish "%PROJ_FILE%" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "%PUBLISH_DIR%"
if %ERRORLEVEL% neq 0 (
    echo [Error] Dotnet publish failed.
    exit /b %ERRORLEVEL%
)

echo.
echo [2/4] Building Unpacked Files for Installer (Avoiding Double-Packing)...
"%DOTNET_EXE%" publish "%PROJ_FILE%" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -o "%PUBLISH_FILES_DIR%"
if %ERRORLEVEL% neq 0 (
    echo [Error] Dotnet publish unpacked failed.
    exit /b %ERRORLEVEL%
)

echo.
echo [3/4] Checking Inno Setup compiler in tools\inno...
if exist "%ISCC_EXE%" goto :COMPILE_INSTALLER

echo Downloading Inno Setup (Portable mode)...
powershell -NoProfile -ExecutionPolicy Bypass -Command "[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; Invoke-WebRequest -Uri 'https://github.com/jrsoftware/issrc/releases/download/is-6_3_3/innosetup-6.3.3.exe' -OutFile '%TOOLS_DIR%\inno_setup.exe'"
if %ERRORLEVEL% neq 0 (
    echo [Error] Failed to download Inno Setup.
    exit /b %ERRORLEVEL%
)

echo Extracting Inno Setup into tools\inno...
"%TOOLS_DIR%\inno_setup.exe" /PORTABLE=1 /DIR="%INNO_DIR%" /VERYSILENT /SUPPRESSMSGBOXES /CURRENTUSER /NORESTART
if exist "%TOOLS_DIR%\inno_setup.exe" del "%TOOLS_DIR%\inno_setup.exe"

if not exist "%ISCC_EXE%" (
    echo [Error] ISCC.exe not found after extraction.
    exit /b 1
)

:COMPILE_INSTALLER
echo.
echo [4/4] Compiling Windows Installer (dist\BTBatteryMonitor_Setup.exe)...
if not exist "%DIST_DIR%" mkdir "%DIST_DIR%"
"%ISCC_EXE%" "%ISS_FILE%"
if %ERRORLEVEL% neq 0 (
    echo [Error] Installer compilation failed.
    exit /b %ERRORLEVEL%
)

echo.
echo ========================================================
echo  Publish Complete!
echo  Single EXE : %PUBLISH_DIR%\BTBatteryMonitor.exe
echo  Installer  : %DIST_DIR%\BTBatteryMonitor_Setup.exe
echo ========================================================

endlocal
