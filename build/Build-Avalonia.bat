@echo off
REM Batch file to execute PowerShell build script

echo Starting Avalonia Projects Batch Build...

REM Check if PowerShell is available
powershell -Command "exit 0" >nul 2>&1
if errorlevel 1 (
    echo PowerShell is not available. Please install PowerShell.
    pause
    exit /b 1
)

REM Execute the PowerShell script with default parameters
powershell -ExecutionPolicy Bypass -File "%~dp0Build-AvaloniaProjects.ps1" %*

if errorlevel 1 (
    echo Build failed!
    pause
    exit /b 1
) else (
    echo Build completed successfully!
    pause
)