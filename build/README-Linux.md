# Linux Build Scripts

This directory contains Linux/Unix equivalents of the Windows PowerShell build scripts.

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