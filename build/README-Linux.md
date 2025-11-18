# Linux Build Scripts

This directory contains Linux/Unix equivalents of the Windows PowerShell build scripts.

## Preparation steps:
Clone all avalonia forks from iplus-framework into the same directory level like iplus.

https://github.com/iplus-framework/XamlX.git
https://github.com/iplus-framework/Avalonia.git
https://github.com/iplus-framework/roslynpad.git
https://github.com/iplus-framework/SVG.git
https://github.com/iplus-framework/AvSvg.Skia.git
https://github.com/iplus-framework/AvRichTextBox.git
https://github.com/iplus-framework/avOxyplot-avalonia.git
https://github.com/iplus-framework/AvMessageBox.Avalonia.git
https://github.com/iplus-framework/AvDialogHost.Avalonia.git
https://github.com/iplus-framework/AvaloniaEdit.git
https://github.com/iplus-framework/Xaml.Behaviors.git
https://github.com/iplus-framework/Avalonia.Dock.git
https://github.com/iplus-framework/Avalonia.Labs.git
https://github.com/iplus-framework/Avalonia.Controls.DataGrid.git

Avalonia uses XamlX for Xaml-Compilation. Replace the "..\external\XamlX" dummy folder in the avalonia ui solution with a symbolic link to the XamlX-Project:
mklink /D "D:\Devel\iPlusGit\V5\av_main\external\XamlX" "D:\Devel\iPlusGit\V5\XamlX"

Do the same for the Datagrid:
mklink /D "D:\Devel\iPlusGit\V5\av_main\external\Avalonia.Controls.DataGrid" "D:\Devel\iPlusGit\V5\Avalonia.Controls.DataGrid"

Call workload restore to get the wasm tools for android, ios and browser:
> dotnet workload restore
> dotnet restore

AvSvg.Skia uses SVG. Replace the "..\externals\SVG" dummy folder in the AvSvg.Skia solution with a symbolic link to the SVG-Project:
mklink /D "D:\Devel\iPlusGit\V5\AvSvg.Skia\externals\SVG" "D:\Devel\iPlusGit\V5\SVG"

**Set the UseAvaloniaFork Property in Avalonia.Version.props to 'True' !!!**

Afterwards run this Build-Script or compile all solutions step by step in visual studio.

## Files

- `build-avalonia-projects.sh` - Main build script (Linux equivalent of `Build-AvaloniaProjects.ps1`)
- `Build-Avalonia.sh` - Simple wrapper script (equivalent of `Build-Avalonia.bat`)

## Usage

### Basic Usage
```bash
# Make scripts executable (first time only)
chmod +x build-avalonia-projects.sh Build-Avalonia.sh

# Run with default settings (Debug configuration)
./build-avalonia-projects.sh

# Or use the wrapper
./Build-Avalonia.sh
```

### Advanced Usage
```bash
# Build in Release configuration
./build-avalonia-projects.sh --configuration Release

# Clean and rebuild
./build-avalonia-projects.sh --clean --rebuild

# Show detailed diagnostics
./build-avalonia-projects.sh --show-diagnostics

# Build with specific CPU count
./build-avalonia-projects.sh --max-cpu-count 4

# Disable parallel builds
./build-avalonia-projects.sh --no-parallel

# Stop on first error (don't continue)
./build-avalonia-projects.sh --no-continue-on-error
```

## Parameter Mapping

| PowerShell Parameter | Linux Parameter | Description |
|---------------------|----------------|-------------|
| `-Configuration` | `-c, --configuration` | Build configuration (Debug/Release) |
| `-LogLevel` | `-l, --log-level` | MSBuild log level |
| `-Clean` | `--clean` | Clean before building |
| `-Rebuild` | `--rebuild` | Rebuild all projects |
| `-Parallel:$false` | `--no-parallel` | Disable parallel builds |
| `-MaxCpuCount` | `--max-cpu-count` | Maximum CPU count |
| `-SkipMissing:$false` | `--no-skip-missing` | Don't skip missing projects |
| `-ContinueOnError:$false` | `--no-continue-on-error` | Stop on first error |
| `-ShowDiagnostics` | `--show-diagnostics` | Show diagnostics |

## Requirements

- .NET 9 SDK installed
- `bash` shell
- `bc` calculator (optional, for precise timing - install with `sudo apt-get install bc`)
- Standard Unix tools: `grep`, `head`, `tail`, `mktemp`

## Key Differences from PowerShell Version

1. **Parameter Syntax**: Uses standard Unix long/short options instead of PowerShell parameter syntax
2. **Path Separators**: Uses forward slashes instead of backslashes (handled automatically by .NET)
3. **Color Output**: Uses ANSI escape codes instead of PowerShell color parameters
4. **Array Handling**: Uses bash arrays instead of PowerShell arrays/hashtables
5. **Temporary Files**: Uses `mktemp` for temporary files instead of PowerShell's automatic cleanup
6. **Error Handling**: Uses bash exit codes and conditional logic instead of PowerShell exception handling

## Output

The script provides the same colored output and build summary as the PowerShell version:
- ? Green for successful builds
- ? Red for failed builds  
- ? Yellow for skipped builds
- Detailed error reporting with suggestions
- Build timing and statistics
- Summary with recommendations

## Exit Codes

- `0`: All builds successful
- `1`: Some non-critical projects failed
- `2`: Critical projects failed

## Troubleshooting

If you encounter permission errors:
```bash
chmod +x build-avalonia-projects.sh Build-Avalonia.sh
```

If timing calculations don't work:
```bash
# Install bc calculator
sudo apt-get install bc        # Ubuntu/Debian
sudo yum install bc            # CentOS/RHEL
sudo pacman -S bc              # Arch Linux
```

For debugging, use the diagnostics flag:
```bash
./build-avalonia-projects.sh --show-diagnostics
```