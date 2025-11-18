@echo off
setlocal enabledelayedexpansion

REM Batch file to execute PowerShell build script for Avalonia projects
REM Usage: Build-Avalonia.bat [Configuration] [Additional Parameters]

echo ===============================================================
echo Avalonia Projects Batch Build Launcher
echo ===============================================================
echo.

REM Set default configuration
set DEFAULT_CONFIG=Debug
set BUILD_CONFIG=%DEFAULT_CONFIG%

REM Parse command line arguments
if not "%1"=="" set BUILD_CONFIG=%1

echo Configuration: %BUILD_CONFIG%
echo.

REM Check if PowerShell is available
echo Checking PowerShell availability...
powershell -Command "Write-Host 'PowerShell is available'" >nul 2>&1
if errorlevel 1 (
    echo ERROR: PowerShell is not available or not in PATH.
    echo Please install PowerShell or ensure it's properly configured.
    pause
    exit /b 1
)
echo ? PowerShell is available

REM Check if script file exists
set SCRIPT_PATH=%~dp0Build-AvaloniaProjects.ps1
if not exist "%SCRIPT_PATH%" (
    echo ERROR: PowerShell script not found at: %SCRIPT_PATH%
    pause
    exit /b 1
)
echo ? PowerShell script found

REM Check if .NET SDK is available
echo Checking .NET SDK availability...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET SDK not found. Please install .NET 9 SDK.
    pause
    exit /b 1
)
for /f "tokens=*" %%a in ('dotnet --version 2^>nul') do set DOTNET_VERSION=%%a
echo ? .NET SDK found: %DOTNET_VERSION%

echo.
echo Starting PowerShell build script...
echo ===============================================================

REM Execute the PowerShell script with all parameters passed through
powershell -ExecutionPolicy Bypass -NoProfile -File "%SCRIPT_PATH%" -Configuration %BUILD_CONFIG% %2 %3 %4 %5 %6 %7 %8 %9

set SCRIPT_EXIT_CODE=!errorlevel!

echo.
echo ===============================================================
if !SCRIPT_EXIT_CODE! equ 0 (
    echo BUILD COMPLETED SUCCESSFULLY!
    echo Configuration: %BUILD_CONFIG%
) else (
    echo BUILD FAILED!
    echo Exit code: !SCRIPT_EXIT_CODE!
    echo Configuration: %BUILD_CONFIG%
)
echo ===============================================================

REM Pause only if running interactively (not from another script)
echo %CMDCMDLINE% | find /i "%~0" >nul
if not errorlevel 1 (
    echo.
    pause
)

exit /b !SCRIPT_EXIT_CODE!