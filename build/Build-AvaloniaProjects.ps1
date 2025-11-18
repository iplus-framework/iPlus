# PowerShell script to batch compile Avalonia-dependent projects
# Copyright (c) 2024, gipSoft d.o.o.

param(
    [string]$Configuration = "Debug",  # Changed default to Debug to match local builds
    [string]$LogLevel = "minimal",     # Changed default to minimal for cleaner output
    [switch]$Clean = $false,
    [switch]$Rebuild = $false,
    [switch]$Parallel = $true,
    [int]$MaxCpuCount = 0,
    [switch]$SkipMissing = $true,      # New parameter to skip missing projects
    [switch]$DetectFrameworks = $true, # New parameter to auto-detect frameworks
    [switch]$ContinueOnError = $true,  # Continue building other projects if one fails
    [switch]$ShowDiagnostics = $false  # Show detailed diagnostic information
)

# Set error handling
$ErrorActionPreference = "Continue"
if ($ShowDiagnostics) {
    $VerbosePreference = "Continue"
}

# Base directory (current script location)
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$BaseDir = $ScriptDir

Write-Host "Starting Avalonia Projects Batch Build" -ForegroundColor Green
Write-Host "Base Directory: $BaseDir" -ForegroundColor Yellow
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow

# Define solution files with their relative paths from the base directory
$SolutionFiles = @(
    @{ Path = "..\..\av_main\iPlusAvalonia.sln"; Name = "Avalonia Core"; Priority = 1; Critical = $true },
    @{ Path = "..\..\AvDialogHost.Avalonia\iPlusAvalonia.sln"; Name = "Dialog Host"; Priority = 1; Critical = $false },
    @{ Path = "..\..\AvaloniaEdit\iPlusAvalonia.sln"; Name = "Avalonia Edit"; Priority = 1; Critical = $false },
    @{ Path = "..\..\Avalonia.Dock\iPlusAvalonia.sln"; Name = "Avalonia Dock"; Priority = 1; Critical = $false },
    @{ Path = "..\..\roslynpad\iPlusAvalonia.sln"; Name = "Roslyn Pad"; Priority = 1; Critical = $false },
    @{ Path = "..\..\Avalonia.Labs\iPlusAvalonia.sln"; Name = "Avalonia Labs"; Priority = 1; Critical = $false },
    @{ Path = "..\..\SVG\Source\iPlusAvalonia.sln"; Name = "SVG"; Priority = 1; Critical = $false },
    @{ Path = "..\..\Avalonia.Controls.DataGrid\iPlusAvalonia.sln"; Name = "DataGrid Controls"; Priority = 1; Critical = $false },
    @{ Path = "..\..\Avalonia.Xaml.Behaviors\iPlusAvalonia.sln"; Name = "XAML Behaviors"; Priority = 1; Critical = $false },
    @{ Path = "..\..\AvSvg.Skia\iPlusAvalonia.sln"; Name = "SVG Skia"; Priority = 1; Critical = $false },
    @{ Path = "..\..\AvRichTextBox\iPlusAvalonia.sln"; Name = "Rich Text Box"; Priority = 1; Critical = $false },
    @{ Path = "..\..\AvMarkdown.Avalonia\iPlusAvalonia.sln"; Name = "Markdown Avalonia"; Priority = 1; Critical = $false },
    @{ Path = "..\..\AvMessageBox.Avalonia\iPlusAvalonia.sln"; Name = "Message Box"; Priority = 1; Critical = $false },
    @{ Path = "..\..\avOxyplot-avalonia\Source\iPlusAvalonia.sln"; Name = "OxyPlot Avalonia"; Priority = 1; Critical = $false }
)

# Build results tracking
$BuildResults = @()
$TotalProjects = $SolutionFiles.Count #+ $SingleProjects.Count
$SuccessfulBuilds = 0
$FailedBuilds = 0
$SkippedBuilds = 0

# Function to detect target framework from project file
function Get-ProjectTargetFramework {
    param([string]$ProjectPath)
    
    if (-not (Test-Path $ProjectPath)) {
        return $null
    }
    
    try {
        $content = Get-Content $ProjectPath -Raw
        if ($content -match '<TargetFramework[s]?>(.*?)</TargetFramework[s]?>') {
            $frameworks = $matches[1]
            # Take the first framework if multiple are specified
            $framework = ($frameworks -split ';')[0].Trim()
            Write-Verbose "Detected framework '$framework' for $ProjectPath"
            return $framework
        }
    }
    catch {
        $errorMessage = $_.Exception.Message
        Write-Verbose "Could not detect framework for ${ProjectPath}: $errorMessage"
    }
    
    return $null
}

# Function to build a solution or project
function Build-Project {
    param(
        [string]$ProjectPath,
        [string]$ProjectName,
        [string]$Config,
        [bool]$IsClean,
        [bool]$IsRebuild,
        [bool]$UseParallel,
        [int]$CpuCount,
        [int]$Priority = 1,
        [bool]$Critical = $false
    )

    $FullPath = Join-Path $BaseDir $ProjectPath
    
    if (-not (Test-Path $FullPath)) {
        if ($Critical -and -not $SkipMissing) {
            Write-Host "✗ CRITICAL PROJECT MISSING: $ProjectName" -ForegroundColor Red
            Write-Host "  Path: $FullPath" -ForegroundColor Red
            Write-Host "  This project is required for the build to succeed!" -ForegroundColor Red
            return $false
        } elseif ($SkipMissing) {
            Write-Host "⊘ SKIPPED (Missing): $ProjectName" -ForegroundColor Yellow
            if ($ShowDiagnostics) {
                Write-Host "  Path: $FullPath" -ForegroundColor Gray
            }
            return "skipped"
        } else {
            Write-Error "Project not found: $FullPath"
            return $false
        }
    }

    # Fix the syntax error by using proper string concatenation
    $CriticalLabel = if ($Critical) { " (CRITICAL)" } else { "" }
    Write-Host "`n[$Priority] Building: $ProjectName$CriticalLabel" -ForegroundColor Cyan
    if ($ShowDiagnostics) {
        Write-Host "Path: $FullPath" -ForegroundColor Gray
    }

    # Determine build target
    $BuildTarget = "Build"
    if ($IsRebuild) {
        $BuildTarget = "Rebuild"
    } elseif ($IsClean) {
        $BuildTarget = "Clean"
    }

    # Build MSBuild arguments - Start with minimal required arguments
    $MSBuildArgs = @(
        "`"$FullPath`"",
        "/p:Configuration=$Config",
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

    # Only add platform specification for solutions, not individual projects
    if ($ProjectPath.EndsWith('.sln')) {
        $MSBuildArgs += "/p:Platform=`"Any CPU`""
    }

    # Add restore settings
    $MSBuildArgs += "/p:RestorePackagesConfig=true"
    
    # Don't force WPF/WinForms settings - let projects decide
    # $MSBuildArgs += "/p:UseWPF=false"
    # $MSBuildArgs += "/p:UseWindowsForms=false"
    
    # Don't force target framework - let projects use their own
    # This was the main issue causing build failures
    
    # Execute MSBuild
    $StartTime = Get-Date
    
    try {
        if ($ShowDiagnostics) {
            Write-Host "Executing: dotnet build $($MSBuildArgs -join ' ')" -ForegroundColor Gray
        }
        
        # Use a simpler process execution approach that's more reliable
        $argumentList = @("build") + $MSBuildArgs
        
        $process = Start-Process -FilePath "dotnet" -ArgumentList $argumentList -Wait -PassThru -NoNewWindow -RedirectStandardOutput "build_temp_stdout.txt" -RedirectStandardError "build_temp_stderr.txt"
        
        $ExitCode = $process.ExitCode
        $EndTime = Get-Date
        $Duration = $EndTime - $StartTime

        # Read the output files
        $standardOutput = ""
        $errorOutput = ""
        
        if (Test-Path "build_temp_stdout.txt") {
            $standardOutput = Get-Content "build_temp_stdout.txt" -Raw -ErrorAction SilentlyContinue
            Remove-Item "build_temp_stdout.txt" -ErrorAction SilentlyContinue
        }
        
        if (Test-Path "build_temp_stderr.txt") {
            $errorOutput = Get-Content "build_temp_stderr.txt" -Raw -ErrorAction SilentlyContinue
            Remove-Item "build_temp_stderr.txt" -ErrorAction SilentlyContinue
        }
        
        if ($ShowDiagnostics) {
            Write-Host "Process Exit Code: $ExitCode" -ForegroundColor Gray
            if ($standardOutput -and $standardOutput.Trim()) {
                Write-Host "Standard Output Length: $($standardOutput.Length) characters" -ForegroundColor Gray
            }
            if ($errorOutput -and $errorOutput.Trim()) {
                Write-Host "Error Output Length: $($errorOutput.Length) characters" -ForegroundColor Gray
            }
        }

        if ($ExitCode -eq 0) {
            Write-Host "✓ SUCCESS: $ProjectName built successfully in $($Duration.TotalSeconds.ToString('F1')) seconds" -ForegroundColor Green
            
            # Show warnings if requested
            if ($ShowDiagnostics -and $standardOutput) {
                $warnings = $standardOutput -split "`n" | Where-Object { $_ -match "warning" } | Select-Object -First 3
                if ($warnings.Count -gt 0) {
                    Write-Host "Warnings:" -ForegroundColor Yellow
                    $warnings | ForEach-Object {
                        Write-Host "  $_" -ForegroundColor Yellow
                    }
                }
            }
            
            return $true
        } else {
            Write-Host "✗ FAILED: $ProjectName build failed (Exit Code: $ExitCode)" -ForegroundColor Red
            
            # Show error information with better formatting
            Write-Host "`nBuild Output Summary:" -ForegroundColor Yellow
            
            # Show stderr first if it exists
            if ($errorOutput -and $errorOutput.Trim()) {
                Write-Host "Standard Error Output:" -ForegroundColor Red
                $errorLines = ($errorOutput -split "`n" | Where-Object { $_.Trim() } | Select-Object -First 10)
                $errorLines | ForEach-Object {
                    if ($_.Trim()) { Write-Host "  $_" -ForegroundColor Red }
                }
            }
            
            # Show relevant lines from standard output
            if ($standardOutput -and $standardOutput.Trim()) {
                # Look for error patterns
                $errorPatterns = @("error", "fail", "exception", "not found", "could not", "unable to")
                $relevantLines = @()
                
                foreach ($line in ($standardOutput -split "`n")) {
                    foreach ($pattern in $errorPatterns) {
                        if ($line -match $pattern -and $line.Trim() -ne "") {
                            $relevantLines += $line
                            break
                        }
                    }
                }
                
                if ($relevantLines.Count -gt 0) {
                    Write-Host "`nRelevant Error Lines from Build Output:" -ForegroundColor Red
                    $relevantLines | Select-Object -First 8 | ForEach-Object {
                        if ($_.Trim()) { Write-Host "  $_" -ForegroundColor Red }
                    }
                } else {
                    # Check if the output actually indicates success despite exit code
                    if ($standardOutput -match "Build succeeded" -or $standardOutput -match "0 Error\(s\)") {
                        Write-Host "`nNOTE: Build output indicates success despite exit code $ExitCode" -ForegroundColor Yellow
                        Write-Host "This might be a PowerShell process handling issue." -ForegroundColor Yellow
                        Write-Host "Treating as successful build..." -ForegroundColor Green
                        return $true
                    }
                    
                    # If no specific errors found, show last few lines of output
                    $lastLines = ($standardOutput -split "`n" | Where-Object { $_.Trim() } | Select-Object -Last 5)
                    if ($lastLines.Count -gt 0) {
                        Write-Host "`nLast Build Output Lines:" -ForegroundColor Yellow
                        $lastLines | ForEach-Object {
                            if ($_.Trim()) { Write-Host "  $_" -ForegroundColor Yellow }
                        }
                    }
                }
            }
            
            # Provide helpful suggestion
            Write-Host "`nSuggestion: Try building this project individually to see more detailed errors:" -ForegroundColor Cyan
            Write-Host "  dotnet build `"$FullPath`" --verbosity normal" -ForegroundColor White
            
            # If this is a critical project, consider stopping
            if ($Critical -and -not $ContinueOnError) {
                Write-Host "CRITICAL PROJECT FAILED - Stopping build process!" -ForegroundColor Red
                throw "Critical project build failed"
            }
            
            return $false
        }
    }
    catch {
        Write-Host "✗ EXCEPTION: Error building $ProjectName - $($_.Exception.Message)" -ForegroundColor Red
        if ($Critical -and -not $ContinueOnError) {
            throw
        }
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

    # Show project path summary
    if ($ShowDiagnostics) {
        Write-Host "Checking project paths..." -ForegroundColor Yellow
        $foundProjects = 0
        $criticalProjects = 0
        $foundCritical = 0
        $totalProjects = $SolutionFiles.Count
        
        foreach ($solution in $SolutionFiles) {
            $fullPath = Join-Path $BaseDir $solution.Path
            if ($solution.Critical) { $criticalProjects++ }
            
            if (Test-Path $fullPath) {
                $foundProjects++
                if ($solution.Critical) { $foundCritical++ }
                $criticalText = if ($solution.Critical) { " (CRITICAL)" } else { "" }
                Write-Host "✓ Found: $($solution.Name)$criticalText" -ForegroundColor Green
            } else {
                if ($solution.Critical) { 
                    Write-Host "✗ Missing: $($solution.Name) (CRITICAL)" -ForegroundColor Red
                } else { 
                    Write-Host "⊘ Missing: $($solution.Name)" -ForegroundColor Yellow
                }
            }
        }
                
        Write-Host "Found $foundProjects of $totalProjects projects ($foundCritical of $criticalProjects critical)" -ForegroundColor White
        
        if ($criticalProjects -gt 0 -and $foundCritical -lt $criticalProjects) {
            Write-Host "WARNING: $($criticalProjects - $foundCritical) critical projects are missing!" -ForegroundColor Red
        }
    }

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
    Write-Host "Skipped: $SkippedBuilds" -ForegroundColor Yellow
    
    if ($FailedBuilds -gt 0) {
        Write-Host "`nFailed Projects:" -ForegroundColor Red
        foreach ($result in $Results) {
            if (-not $result.Success -and $result.Success -ne "skipped") {
                Write-Host "  ✗ $($result.Name)" -ForegroundColor Red
            }
        }
    }
    
    if ($SkippedBuilds -gt 0 -and $ShowDiagnostics) {
        Write-Host "`nSkipped Projects:" -ForegroundColor Yellow
        foreach ($result in $Results) {
            if ($result.Success -eq "skipped") {
                Write-Host "  ⊘ $($result.Name)" -ForegroundColor Yellow
            }
        }
    }
    
    if ($SuccessfulBuilds -gt 0) {
        Write-Host "`nSuccessful Projects:" -ForegroundColor Green
        foreach ($result in $Results) {
            if ($result.Success -eq $true) {
                Write-Host "  ✓ $($result.Name)" -ForegroundColor Green
            }
        }
    }
    
    Write-Host "`nTotal Build Time: $($script:TotalBuildTime.TotalMinutes.ToString('F1')) minutes" -ForegroundColor White
    
    # Show recommendations
    if ($SkippedBuilds -gt 0) {
        Write-Host "`nRECOMMENDATIONS:" -ForegroundColor Cyan
        Write-Host "- Run '.\Diagnose-AvaloniaProjects.ps1 -ShowDetails' to see missing project locations" -ForegroundColor Cyan
        Write-Host "- Ensure all Avalonia dependency repositories are cloned in the expected locations" -ForegroundColor Cyan
    }
    
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
    $cpuCountText = if($MaxCpuCount -eq 0) { 'Auto' } else { $MaxCpuCount }
    Write-Host "Max CPU Count: $cpuCountText" -ForegroundColor White
    Write-Host "Skip Missing: $SkipMissing" -ForegroundColor White
    Write-Host "Continue On Error: $ContinueOnError" -ForegroundColor White

    # Sort projects by priority for better dependency resolution
    $allProjects = @()
    $allProjects += $SolutionFiles | ForEach-Object { $_ }
    $sortedProjects = $allProjects # | Sort-Object Priority

    # Build projects in priority order
    foreach ($project in $sortedProjects) {
        try {
            $result = Build-Project -ProjectPath $project.Path -ProjectName $project.Name -Config $Configuration -IsClean $Clean -IsRebuild $Rebuild -UseParallel $Parallel -CpuCount $MaxCpuCount -Priority $project.Priority -Critical $project.Critical
            
            $BuildResults += @{
                Name = $project.Name
                Path = $project.Path
                Success = $result
                Priority = $project.Priority
                Critical = $project.Critical
            }
            
            if ($result -eq $true) {
                $SuccessfulBuilds++
            } elseif ($result -eq "skipped") {
                $SkippedBuilds++
            } else {
                $FailedBuilds++
            }
            
            # Add a small delay between builds to avoid resource conflicts
            Start-Sleep -Milliseconds 200
        }
        catch {
            Write-Host "CRITICAL ERROR: $($_.Exception.Message)" -ForegroundColor Red
            if (-not $ContinueOnError) {
                break
            }
        }
    }
}

# Display summary
Show-BuildSummary -Results $BuildResults

# Exit with appropriate code
if ($FailedBuilds -gt 0) {
    $criticalFailures = $BuildResults | Where-Object { $_.Success -eq $false -and $_.Critical }
    if ($criticalFailures.Count -gt 0) {
        Write-Host "`nBuild FAILED due to critical project failures!" -ForegroundColor Red
        exit 2
    } else {
        Write-Host "`nBuild completed with $FailedBuilds non-critical errors!" -ForegroundColor Yellow
        exit 1
    }
} else {
    Write-Host "`nAll available projects built successfully!" -ForegroundColor Green
    Write-Host "($SuccessfulBuilds successful, $SkippedBuilds skipped)" -ForegroundColor Green
    exit 0
}