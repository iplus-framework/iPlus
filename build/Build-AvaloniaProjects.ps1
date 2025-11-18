# PowerShell script to batch compile Avalonia-dependent projects
# Copyright (c) 2024, gipSoft d.o.o.

param(
    [string]$Configuration = "Release",
    [string]$LogLevel = "normal",
    [switch]$Clean = $false,
    [switch]$Rebuild = $false,
    [switch]$Parallel = $true,
    [int]$MaxCpuCount = 0
)

# Set error handling
$ErrorActionPreference = "Continue"
$VerbosePreference = "Continue"

# Base directory (current script location)
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$BaseDir = $ScriptDir

Write-Host "Starting Avalonia Projects Batch Build" -ForegroundColor Green
Write-Host "Base Directory: $BaseDir" -ForegroundColor Yellow
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow

# Define solution files with their relative paths from the base directory
$SolutionFiles = @(
    @{ Path = "..\..\av_main\Avalonia.sln"; Name = "Avalonia Core" },
    @{ Path = "..\..\AvDialogHost.Avalonia\DialogHost.Avalonia.sln"; Name = "Dialog Host" },
    @{ Path = "..\..\AvaloniaEdit\AvaloniaEdit.sln"; Name = "Avalonia Edit" },
    @{ Path = "..\..\Avalonia.Dock\Dock.sln"; Name = "Avalonia Dock" },
    @{ Path = "..\..\roslynpad\src\RoslynPad.Avalonia.sln"; Name = "Roslyn Pad" },
    @{ Path = "..\..\SVG\Source\Svg.sln"; Name = "SVG" },
    @{ Path = "..\..\Avalonia.Controls.DataGrid\Avalonia.Controls.DataGrid.sln"; Name = "DataGrid Controls" },
    @{ Path = "..\..\Avalonia.Xaml.Behaviors\AvaloniaBehaviors.sln"; Name = "XAML Behaviors" },
    @{ Path = "..\..\AvSvg.Skia\Svg.Skia.sln"; Name = "SVG Skia" },
    @{ Path = "..\..\AvRichTextBox\AvRichTextBox.sln"; Name = "Rich Text Box" },
    @{ Path = "..\..\AvMarkdown.Avalonia\Markdown.Avalonia.sln"; Name = "Markdown Avalonia" },
    @{ Path = "..\..\AvMessageBox.Avalonia\MessageBox.Avalonia.sln"; Name = "Message Box" },
    @{ Path = "..\..\avOxyplot-avalonia\Source\OxyPlot.Avalonia.sln"; Name = "OxyPlot Avalonia" }
)

# Special handling for Avalonia.Labs (single project file)
$SingleProjects = @(
    @{ Path = "..\..\Avalonia.Labs\src\Avalonia.Labs.CommandManager\Avalonia.Labs.CommandManager.csproj"; Name = "Avalonia Labs Command Manager" }
)

# Build results tracking
$BuildResults = @()
$TotalProjects = $SolutionFiles.Count + $SingleProjects.Count
$SuccessfulBuilds = 0
$FailedBuilds = 0

# Function to build a solution or project
function Build-Project {
    param(
        [string]$ProjectPath,
        [string]$ProjectName,
        [string]$Config,
        [bool]$IsClean,
        [bool]$IsRebuild,
        [bool]$UseParallel,
        [int]$CpuCount
    )

    $FullPath = Join-Path $BaseDir $ProjectPath
    
    if (-not (Test-Path $FullPath)) {
        Write-Warning "Project not found: $FullPath"
        return $false
    }

    Write-Host "`nBuilding: $ProjectName" -ForegroundColor Cyan
    Write-Host "Path: $FullPath" -ForegroundColor Gray

    # Determine build target
    $BuildTarget = "Build"
    if ($IsRebuild) {
        $BuildTarget = "Rebuild"
    } elseif ($IsClean) {
        $BuildTarget = "Clean"
    }

    # Build MSBuild arguments
    $MSBuildArgs = @(
        "`"$FullPath`"",
        "/p:Configuration=$Config",
        "/p:Platform=`"Any CPU`"",
        "/t:$BuildTarget",
        "/v:$LogLevel",
        "/nologo"
    )

    # Add parallel build settings if enabled
    if ($UseParallel) {
        if ($CpuCount -gt 0) {
            $MSBuildArgs += "/maxcpucount:$CpuCount"
        } else {
            $MSBuildArgs += "/maxcpucount"
        }
    }

    # Add .NET 9 specific settings
    $MSBuildArgs += "/p:TargetFramework=net9.0-windows"
    $MSBuildArgs += "/p:UseWPF=false"
    $MSBuildArgs += "/p:UseWindowsForms=false"

    # Execute MSBuild
    $StartTime = Get-Date
    
    try {
        Write-Host "Executing: dotnet build $($MSBuildArgs -join ' ')" -ForegroundColor Gray
        
        $ProcessInfo = New-Object System.Diagnostics.ProcessStartInfo
        $ProcessInfo.FileName = "dotnet"
        $ProcessInfo.Arguments = "build " + ($MSBuildArgs -join ' ')
        $ProcessInfo.UseShellExecute = $false
        $ProcessInfo.RedirectStandardOutput = $true
        $ProcessInfo.RedirectStandardError = $true
        $ProcessInfo.WorkingDirectory = $BaseDir

        $Process = New-Object System.Diagnostics.Process
        $Process.StartInfo = $ProcessInfo
        
        $stdout = New-Object System.Text.StringBuilder
        $stderr = New-Object System.Text.StringBuilder
        
        $Process.add_OutputDataReceived({ param($sender, $e) if ($e.Data) { $stdout.AppendLine($e.Data) } })
        $Process.add_ErrorDataReceived({ param($sender, $e) if ($e.Data) { $stderr.AppendLine($e.Data) } })
        
        $Process.Start() | Out-Null
        $Process.BeginOutputReadLine()
        $Process.BeginErrorReadLine()
        $Process.WaitForExit()
        
        $ExitCode = $Process.ExitCode
        $EndTime = Get-Date
        $Duration = $EndTime - $StartTime

        if ($ExitCode -eq 0) {
            Write-Host "✓ SUCCESS: $ProjectName built successfully in $($Duration.TotalSeconds.ToString('F1')) seconds" -ForegroundColor Green
            return $true
        } else {
            Write-Host "✗ FAILED: $ProjectName build failed (Exit Code: $ExitCode)" -ForegroundColor Red
            if ($stderr.Length -gt 0) {
                Write-Host "Errors:" -ForegroundColor Red
                Write-Host $stderr.ToString() -ForegroundColor Red
            }
            return $false
        }
    }
    catch {
        Write-Host "✗ EXCEPTION: Error building $ProjectName - $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Function to check prerequisites
function Test-Prerequisites {
    Write-Host "Checking prerequisites..." -ForegroundColor Yellow
    
    # Check if dotnet CLI is available
    try {
        $dotnetVersion = & dotnet --version 2>$null
        Write-Host "✓ .NET SDK found: $dotnetVersion" -ForegroundColor Green
    }
    catch {
        Write-Host "✗ .NET SDK not found. Please install .NET 9 SDK." -ForegroundColor Red
        return $false
    }

    # Check if we're in the correct directory
    $versionPropsPath = Join-Path $BaseDir "Avalonia.Version.props"
    if (-not (Test-Path $versionPropsPath)) {
        Write-Host "✗ Avalonia.Version.props not found. Please run this script from the build directory." -ForegroundColor Red
        return $false
    }
    Write-Host "✓ Build configuration found" -ForegroundColor Green

    return $true
}

# Function to display summary
function Show-BuildSummary {
    param([array]$Results)
    
    Write-Host "`n" + "="*60 -ForegroundColor Yellow
    Write-Host "BUILD SUMMARY" -ForegroundColor Yellow
    Write-Host "="*60 -ForegroundColor Yellow
    
    Write-Host "Total Projects: $TotalProjects" -ForegroundColor White
    Write-Host "Successful: $SuccessfulBuilds" -ForegroundColor Green
    Write-Host "Failed: $FailedBuilds" -ForegroundColor Red
    
    if ($FailedBuilds -gt 0) {
        Write-Host "`nFailed Projects:" -ForegroundColor Red
        foreach ($result in $Results) {
            if (-not $result.Success) {
                Write-Host "  ✗ $($result.Name)" -ForegroundColor Red
            }
        }
    }
    
    Write-Host "`nTotal Build Time: $($script:TotalBuildTime.TotalMinutes.ToString('F1')) minutes" -ForegroundColor White
    Write-Host "="*60 -ForegroundColor Yellow
}

# Main execution
$script:TotalBuildTime = Measure-Command {
    
    # Check prerequisites
    if (-not (Test-Prerequisites)) {
        exit 1
    }

    Write-Host "`nStarting batch build with the following settings:" -ForegroundColor Yellow
    Write-Host "Configuration: $Configuration" -ForegroundColor White
    Write-Host "Clean: $Clean" -ForegroundColor White
    Write-Host "Rebuild: $Rebuild" -ForegroundColor White
    Write-Host "Parallel: $Parallel" -ForegroundColor White
    Write-Host "Max CPU Count: $(if($MaxCpuCount -eq 0) { 'Auto' } else { $MaxCpuCount })" -ForegroundColor White

    # Build solutions
    foreach ($solution in $SolutionFiles) {
        $result = Build-Project -ProjectPath $solution.Path -ProjectName $solution.Name -Config $Configuration -IsClean $Clean -IsRebuild $Rebuild -UseParallel $Parallel -CpuCount $MaxCpuCount
        
        $BuildResults += @{
            Name = $solution.Name
            Path = $solution.Path
            Success = $result
        }
        
        if ($result) {
            $SuccessfulBuilds++
        } else {
            $FailedBuilds++
        }
    }

    # Build single projects
    foreach ($project in $SingleProjects) {
        $result = Build-Project -ProjectPath $project.Path -ProjectName $project.Name -Config $Configuration -IsClean $Clean -IsRebuild $Rebuild -UseParallel $Parallel -CpuCount $MaxCpuCount
        
        $BuildResults += @{
            Name = $project.Name
            Path = $project.Path
            Success = $result
        }
        
        if ($result) {
            $SuccessfulBuilds++
        } else {
            $FailedBuilds++
        }
    }
}

# Display summary
Show-BuildSummary -Results $BuildResults

# Exit with appropriate code
if ($FailedBuilds -gt 0) {
    Write-Host "`nBuild completed with errors!" -ForegroundColor Red
    exit 1
} else {
    Write-Host "`nAll projects built successfully!" -ForegroundColor Green
    exit 0
}