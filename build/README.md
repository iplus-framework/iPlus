# Avalonia Projects Batch Build Scripts

This directory contains PowerShell scripts to batch compile Avalonia-dependent projects for the iPlus framework.

## Scripts Overview

### 1. `Build-AvaloniaProjects.ps1` - Main Build Script
The comprehensive build script that attempts to build all Avalonia dependencies.

**Key Features:**
- Builds projects in dependency order (priority-based)
- Skips missing projects automatically  
- Supports both Debug and Release configurations
- Continues building other projects even if one fails
- Provides detailed error reporting

**Usage:**
```powershell
# Basic usage (Debug configuration)
.\Build-AvaloniaProjects.ps1

# Release configuration
.\Build-AvaloniaProjects.ps1 -Configuration Release

# With detailed diagnostics
.\Build-AvaloniaProjects.ps1 -ShowDiagnostics

# Clean and rebuild
.\Build-AvaloniaProjects.ps1 -Rebuild

# Stop on first error (don't continue)
.\Build-AvaloniaProjects.ps1 -ContinueOnError:$false
```

### 2. `Diagnose-AvaloniaProjects.ps1` - Project Discovery
Checks which Avalonia projects are available and provides recommendations.

**Usage:**
```powershell
# Basic check
.\Diagnose-AvaloniaProjects.ps1

# Detailed analysis
.\Diagnose-AvaloniaProjects.ps1 -ShowDetails
```

### 3. `Build-AvaloniaCore.ps1` - Minimal Core Build
Builds only the main Avalonia project for testing.

**Usage:**
```powershell
# Test core Avalonia build
.\Build-AvaloniaCore.ps1

# Verbose output
.\Build-AvaloniaCore.ps1 -Verbose
```

### 4. `Build-Avalonia.bat` - Batch File Launcher
Windows batch file that launches the PowerShell script with proper execution policy.

**Usage:**
```cmd
Build-Avalonia.bat
Build-Avalonia.bat Debug
Build-Avalonia.bat Release -ShowDiagnostics
```

## Expected Project Structure

The scripts expect the following directory structure relative to the build directory:

```
iPlus/
??? build/                          # This directory
?   ??? Build-AvaloniaProjects.ps1
?   ??? ...
??? ../av_main/                     # Main Avalonia framework
??? ../AvDialogHost.Avalonia/       # Dialog Host library
??? ../AvaloniaEdit/                # Text editor component
??? ../Avalonia.Dock/               # Docking library
??? ../roslynpad/                   # Roslyn Pad integration
??? ../SVG/                         # SVG support
??? ../Avalonia.Controls.DataGrid/  # DataGrid controls
??? ../Avalonia.Xaml.Behaviors/     # XAML Behaviors
??? ../AvSvg.Skia/                 # SVG Skia renderer
??? ../AvRichTextBox/              # Rich text box
??? ../AvMarkdown.Avalonia/        # Markdown support
??? ../AvMessageBox.Avalonia/      # Message box
??? ../avOxyplot-avalonia/         # OxyPlot charts
??? ../Avalonia.Labs/              # Avalonia Labs components
```

## Key Improvements Over Original Script

1. **No Forced Target Framework**: Removed the problematic `/p:TargetFramework=net9.0-windows` override that was causing build failures
2. **Better Error Handling**: Projects build independently; one failure doesn't stop the entire process
3. **Missing Project Support**: Automatically skips missing projects instead of failing
4. **Priority-Based Building**: Builds core dependencies first
5. **Detailed Diagnostics**: Optional verbose output for troubleshooting
6. **Configuration Matching**: Uses Debug by default to match your local builds

## Troubleshooting

### Build Fails with "Project not found"
Run `.\Diagnose-AvaloniaProjects.ps1 -ShowDetails` to see which projects are missing and their expected locations.

### Build Fails with Target Framework Errors
The script now lets projects use their own target frameworks instead of forcing .NET 9. If you still see framework errors, check that the individual projects are properly configured.

### PowerShell Execution Policy Errors
Use the batch file launcher or run:
```cmd
powershell -ExecutionPolicy Bypass -File ".\Build-AvaloniaProjects.ps1"
```

### Performance Issues
Try reducing parallelism:
```powershell
.\Build-AvaloniaProjects.ps1 -MaxCpuCount 2
```

## Exit Codes

- `0`: Success - All available projects built successfully
- `1`: Partial failure - Some non-critical projects failed
- `2`: Critical failure - Required projects failed to build

## Notes

- Default configuration is now **Debug** to match your local project builds
- Missing projects are automatically skipped unless marked as critical
- Only the main Avalonia project is marked as critical
- The script preserves each project's own target framework settings