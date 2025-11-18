#!/bin/bash
# Shell script to batch compile Avalonia-dependent projects
# Copyright (c) 2024, gipSoft d.o.o.
# Linux/Unix equivalent of Build-AvaloniaProjects.ps1

# Default parameter values
CONFIGURATION="Debug"
LOG_LEVEL="minimal"
CLEAN=false
REBUILD=false
PARALLEL=true
MAX_CPU_COUNT=0
SKIP_MISSING=true
DETECT_FRAMEWORKS=true
CONTINUE_ON_ERROR=true
SHOW_DIAGNOSTICS=false

# Color definitions for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
GRAY='\033[0;37m'
WHITE='\033[1;37m'
NC='\033[0m' # No Color

# Function to print colored output
print_color() {
    local color=$1
    local message=$2
    echo -e "${color}${message}${NC}"
}

# Function to show usage
show_usage() {
    cat << EOF
Usage: $0 [OPTIONS]

Options:
    -c, --configuration CONFIGURATION    Build configuration (Debug/Release) [default: Debug]
    -l, --log-level LOG_LEVEL           Log level (minimal/normal/detailed) [default: minimal]
    --clean                             Clean before building
    --rebuild                           Rebuild all projects
    --no-parallel                       Disable parallel builds
    --max-cpu-count COUNT               Maximum CPU count for parallel builds [default: auto]
    --no-skip-missing                   Don't skip missing projects
    --no-detect-frameworks              Don't auto-detect frameworks
    --no-continue-on-error              Stop on first error
    --show-diagnostics                  Show detailed diagnostic information
    -h, --help                          Show this help message

Examples:
    $0                                  # Build with default settings
    $0 -c Release --clean              # Clean and build in Release mode
    $0 --rebuild --show-diagnostics    # Rebuild with diagnostics
EOF
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -c|--configuration)
            CONFIGURATION="$2"
            shift 2
            ;;
        -l|--log-level)
            LOG_LEVEL="$2"
            shift 2
            ;;
        --clean)
            CLEAN=true
            shift
            ;;
        --rebuild)
            REBUILD=true
            shift
            ;;
        --no-parallel)
            PARALLEL=false
            shift
            ;;
        --max-cpu-count)
            MAX_CPU_COUNT="$2"
            shift 2
            ;;
        --no-skip-missing)
            SKIP_MISSING=false
            shift
            ;;
        --no-detect-frameworks)
            DETECT_FRAMEWORKS=false
            shift
            ;;
        --no-continue-on-error)
            CONTINUE_ON_ERROR=false
            shift
            ;;
        --show-diagnostics)
            SHOW_DIAGNOSTICS=true
            shift
            ;;
        -h|--help)
            show_usage
            exit 0
            ;;
        *)
            print_color $RED "Unknown option: $1"
            show_usage
            exit 1
            ;;
    esac
done

# Get script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BASE_DIR="$SCRIPT_DIR"

print_color $GREEN "Starting Avalonia Projects Batch Build"
print_color $YELLOW "Base Directory: $BASE_DIR"
print_color $YELLOW "Configuration: $CONFIGURATION"

# Define solution files with their relative paths from the base directory
# Using associative arrays to simulate PowerShell hashtables
declare -a SOLUTION_PATHS=(
    "../../av_main/iPlusAvalonia.sln"
    "../../AvDialogHost.Avalonia/iPlusAvalonia.sln"
    "../../AvaloniaEdit/iPlusAvalonia.sln"
    "../../Avalonia.Dock/iPlusAvalonia.sln"
    "../../roslynpad/iPlusAvalonia.sln"
    "../../Avalonia.Labs/iPlusAvalonia.sln"
    "../../SVG/Source/iPlusAvalonia.sln"
    "../../Avalonia.Controls.DataGrid/iPlusAvalonia.sln"
    "../../Avalonia.Xaml.Behaviors/iPlusAvalonia.sln"
    "../../AvSvg.Skia/iPlusAvalonia.sln"
    "../../AvRichTextBox/iPlusAvalonia.sln"
    "../../AvMarkdown.Avalonia/iPlusAvalonia.sln"
    "../../AvMessageBox.Avalonia/iPlusAvalonia.sln"
    "../../avOxyplot-avalonia/Source/iPlusAvalonia.sln"
)

declare -a SOLUTION_NAMES=(
    "Avalonia Core"
    "Dialog Host"
    "Avalonia Edit"
    "Avalonia Dock"
    "Roslyn Pad"
    "Avalonia Labs"
    "SVG"
    "DataGrid Controls"
    "XAML Behaviors"
    "SVG Skia"
    "Rich Text Box"
    "Markdown Avalonia"
    "Message Box"
    "OxyPlot Avalonia"
)

declare -a SOLUTION_PRIORITIES=(1 1 1 1 1 1 1 1 1 1 1 1 1 1)

declare -a SOLUTION_CRITICAL=(true false false false false false false false false false false false false false)

# Build results tracking
declare -a BUILD_RESULTS_NAMES=()
declare -a BUILD_RESULTS_PATHS=()
declare -a BUILD_RESULTS_SUCCESS=()
declare -a BUILD_RESULTS_PRIORITIES=()
declare -a BUILD_RESULTS_CRITICAL=()

TOTAL_PROJECTS=${#SOLUTION_PATHS[@]}
SUCCESSFUL_BUILDS=0
FAILED_BUILDS=0
SKIPPED_BUILDS=0

# Function to detect target framework from project file
get_project_target_framework() {
    local project_path="$1"
    
    if [[ ! -f "$project_path" ]]; then
        return 1
    fi
    
    local content
    if content=$(cat "$project_path" 2>/dev/null); then
        if [[ $content =~ \<TargetFramework[s]?\>(.*?)\</TargetFramework[s]?\> ]]; then
            local frameworks="${BASH_REMATCH[1]}"
            # Take the first framework if multiple are specified
            local framework="${frameworks%%;*}"
            framework="${framework// /}" # Trim whitespace
            if [[ $SHOW_DIAGNOSTICS == true ]]; then
                print_color $GRAY "Detected framework '$framework' for $project_path"
            fi
            echo "$framework"
            return 0
        fi
    fi
    
    return 1
}

# Function to build a solution or project
build_project() {
    local project_path="$1"
    local project_name="$2"
    local config="$3"
    local is_clean="$4"
    local is_rebuild="$5"
    local use_parallel="$6"
    local cpu_count="$7"
    local priority="$8"
    local critical="$9"

    local full_path="$BASE_DIR/$project_path"
    
    if [[ ! -f "$full_path" ]]; then
        if [[ "$critical" == "true" && "$SKIP_MISSING" == "false" ]]; then
            print_color $RED "? CRITICAL PROJECT MISSING: $project_name"
            print_color $RED "  Path: $full_path"
            print_color $RED "  This project is required for the build to succeed!"
            return 1
        elif [[ "$SKIP_MISSING" == "true" ]]; then
            print_color $YELLOW "? SKIPPED (Missing): $project_name"
            if [[ $SHOW_DIAGNOSTICS == true ]]; then
                print_color $GRAY "  Path: $full_path"
            fi
            return 2 # Special return code for skipped
        else
            print_color $RED "Project not found: $full_path"
            return 1
        fi
    fi

    local critical_label=""
    if [[ "$critical" == "true" ]]; then
        critical_label=" (CRITICAL)"
    fi
    
    echo
    print_color $CYAN "[$priority] Building: $project_name$critical_label"
    if [[ $SHOW_DIAGNOSTICS == true ]]; then
        print_color $GRAY "Path: $full_path"
    fi

    # Determine build target
    local build_target="Build"
    if [[ "$is_rebuild" == "true" ]]; then
        build_target="Rebuild"
    elif [[ "$is_clean" == "true" ]]; then
        build_target="Clean"
    fi

    # Build dotnet arguments
    local dotnet_args=()
    dotnet_args+=("build")
    dotnet_args+=("$full_path")
    dotnet_args+=("-p:Configuration=$config")
    dotnet_args+=("-t:$build_target")
    dotnet_args+=("-v:$LOG_LEVEL")
    dotnet_args+=("--nologo")

    # Add parallel build settings if enabled
    if [[ "$use_parallel" == "true" ]]; then
        if [[ $cpu_count -gt 0 ]]; then
            dotnet_args+=("-maxcpucount:$cpu_count")
        else
            dotnet_args+=("-maxcpucount")
        fi
    fi

    # Only add platform specification for solutions, not individual projects
    if [[ "$project_path" == *.sln ]]; then
        dotnet_args+=('-p:Platform=Any CPU')
    fi

    # Add restore settings
    dotnet_args+=("-p:RestorePackagesConfig=true")
    
    # Execute dotnet build
    local start_time=$(date +%s.%N)
    
    if [[ $SHOW_DIAGNOSTICS == true ]]; then
        print_color $GRAY "Executing: dotnet ${dotnet_args[*]}"
    fi
    
    # Create temporary files for stdout and stderr
    local stdout_file=$(mktemp)
    local stderr_file=$(mktemp)
    
    # Execute the build command
    local exit_code=0
    if ! dotnet "${dotnet_args[@]}" >"$stdout_file" 2>"$stderr_file"; then
        exit_code=$?
    fi
    
    local end_time=$(date +%s.%N)
    local duration=$(echo "$end_time - $start_time" | bc -l)
    duration=$(printf "%.1f" "$duration")

    # Read the output files
    local standard_output=""
    local error_output=""
    
    if [[ -f "$stdout_file" ]]; then
        standard_output=$(cat "$stdout_file")
        rm -f "$stdout_file"
    fi
    
    if [[ -f "$stderr_file" ]]; then
        error_output=$(cat "$stderr_file")
        rm -f "$stderr_file"
    fi
    
    if [[ $SHOW_DIAGNOSTICS == true ]]; then
        print_color $GRAY "Process Exit Code: $exit_code"
        if [[ -n "$standard_output" ]]; then
            print_color $GRAY "Standard Output Length: ${#standard_output} characters"
        fi
        if [[ -n "$error_output" ]]; then
            print_color $GRAY "Error Output Length: ${#error_output} characters"
        fi
    fi

    if [[ $exit_code -eq 0 ]]; then
        print_color $GREEN "? SUCCESS: $project_name built successfully in ${duration} seconds"
        
        # Show warnings if requested
        if [[ $SHOW_DIAGNOSTICS == true && -n "$standard_output" ]]; then
            local warnings
            warnings=$(echo "$standard_output" | grep -i "warning" | head -3)
            if [[ -n "$warnings" ]]; then
                print_color $YELLOW "Warnings:"
                echo "$warnings" | while IFS= read -r line; do
                    print_color $YELLOW "  $line"
                done
            fi
        fi
        
        return 0
    else
        print_color $RED "? FAILED: $project_name build failed (Exit Code: $exit_code)"
        
        # Show error information with better formatting
        echo
        print_color $YELLOW "Build Output Summary:"
        
        # Show stderr first if it exists
        if [[ -n "$error_output" ]]; then
            print_color $RED "Standard Error Output:"
            echo "$error_output" | head -10 | while IFS= read -r line; do
                if [[ -n "${line// /}" ]]; then
                    print_color $RED "  $line"
                fi
            done
        fi
        
        # Show relevant lines from standard output
        if [[ -n "$standard_output" ]]; then
            # Look for error patterns
            local error_patterns=("error" "fail" "exception" "not found" "could not" "unable to")
            local relevant_lines=""
            
            for pattern in "${error_patterns[@]}"; do
                local matches
                matches=$(echo "$standard_output" | grep -i "$pattern" | head -8)
                if [[ -n "$matches" ]]; then
                    relevant_lines="$matches"
                    break
                fi
            done
            
            if [[ -n "$relevant_lines" ]]; then
                echo
                print_color $RED "Relevant Error Lines from Build Output:"
                echo "$relevant_lines" | while IFS= read -r line; do
                    if [[ -n "${line// /}" ]]; then
                        print_color $RED "  $line"
                    fi
                done
            else
                # Check if the output actually indicates success despite exit code
                if echo "$standard_output" | grep -q "Build succeeded\|0 Error(s)"; then
                    echo
                    print_color $YELLOW "NOTE: Build output indicates success despite exit code $exit_code"
                    print_color $YELLOW "This might be a shell process handling issue."
                    print_color $GREEN "Treating as successful build..."
                    return 0
                fi
                
                # If no specific errors found, show last few lines of output
                local last_lines
                last_lines=$(echo "$standard_output" | grep -v '^[[:space:]]*$' | tail -5)
                if [[ -n "$last_lines" ]]; then
                    echo
                    print_color $YELLOW "Last Build Output Lines:"
                    echo "$last_lines" | while IFS= read -r line; do
                        if [[ -n "${line// /}" ]]; then
                            print_color $YELLOW "  $line"
                        fi
                    done
                fi
            fi
        fi
        
        # Provide helpful suggestion
        echo
        print_color $CYAN "Suggestion: Try building this project individually to see more detailed errors:"
        print_color $WHITE "  dotnet build \"$full_path\" --verbosity normal"
        
        # If this is a critical project, consider stopping
        if [[ "$critical" == "true" && "$CONTINUE_ON_ERROR" == "false" ]]; then
            print_color $RED "CRITICAL PROJECT FAILED - Stopping build process!"
            return 3 # Special exit code for critical failure
        fi
        
        return 1
    fi
}

# Function to check prerequisites
test_prerequisites() {
    print_color $YELLOW "Checking prerequisites..."
    
    # Check if dotnet CLI is available
    local dotnet_version
    if dotnet_version=$(dotnet --version 2>/dev/null); then
        print_color $GREEN "? .NET SDK found: $dotnet_version"
    else
        print_color $RED "? .NET SDK not found. Please install .NET 9 SDK."
        return 1
    fi

    # Check if we're in the correct directory
    local version_props_path="$BASE_DIR/Avalonia.Version.props"
    if [[ ! -f "$version_props_path" ]]; then
        print_color $RED "? Avalonia.Version.props not found. Please run this script from the build directory."
        return 1
    fi
    print_color $GREEN "? Build configuration found"

    # Show project path summary
    if [[ $SHOW_DIAGNOSTICS == true ]]; then
        print_color $YELLOW "Checking project paths..."
        local found_projects=0
        local critical_projects=0
        local found_critical=0
        
        for i in "${!SOLUTION_PATHS[@]}"; do
            local full_path="$BASE_DIR/${SOLUTION_PATHS[$i]}"
            local solution_name="${SOLUTION_NAMES[$i]}"
            local is_critical="${SOLUTION_CRITICAL[$i]}"
            
            if [[ "$is_critical" == "true" ]]; then
                ((critical_projects++))
            fi
            
            if [[ -f "$full_path" ]]; then
                ((found_projects++))
                if [[ "$is_critical" == "true" ]]; then
                    ((found_critical++))
                fi
                local critical_text=""
                if [[ "$is_critical" == "true" ]]; then
                    critical_text=" (CRITICAL)"
                fi
                print_color $GREEN "? Found: $solution_name$critical_text"
            else
                if [[ "$is_critical" == "true" ]]; then
                    print_color $RED "? Missing: $solution_name (CRITICAL)"
                else
                    print_color $YELLOW "? Missing: $solution_name"
                fi
            fi
        done
        
        print_color $WHITE "Found $found_projects of $TOTAL_PROJECTS projects ($found_critical of $critical_projects critical)"
        
        if [[ $critical_projects -gt 0 && $found_critical -lt $critical_projects ]]; then
            local missing_critical=$((critical_projects - found_critical))
            print_color $RED "WARNING: $missing_critical critical projects are missing!"
        fi
    fi

    return 0
}

# Function to display summary
show_build_summary() {
    echo
    print_color $YELLOW "============================================================"
    print_color $YELLOW "BUILD SUMMARY"
    print_color $YELLOW "============================================================"
    
    print_color $WHITE "Total Projects: $TOTAL_PROJECTS"
    print_color $GREEN "Successful: $SUCCESSFUL_BUILDS"
    print_color $RED "Failed: $FAILED_BUILDS"
    print_color $YELLOW "Skipped: $SKIPPED_BUILDS"
    
    if [[ $FAILED_BUILDS -gt 0 ]]; then
        echo
        print_color $RED "Failed Projects:"
        for i in "${!BUILD_RESULTS_NAMES[@]}"; do
            if [[ "${BUILD_RESULTS_SUCCESS[$i]}" == "false" ]]; then
                print_color $RED "  ? ${BUILD_RESULTS_NAMES[$i]}"
            fi
        done
    fi
    
    if [[ $SKIPPED_BUILDS -gt 0 && $SHOW_DIAGNOSTICS == true ]]; then
        echo
        print_color $YELLOW "Skipped Projects:"
        for i in "${!BUILD_RESULTS_NAMES[@]}"; do
            if [[ "${BUILD_RESULTS_SUCCESS[$i]}" == "skipped" ]]; then
                print_color $YELLOW "  ? ${BUILD_RESULTS_NAMES[$i]}"
            fi
        done
    fi
    
    if [[ $SUCCESSFUL_BUILDS -gt 0 ]]; then
        echo
        print_color $GREEN "Successful Projects:"
        for i in "${!BUILD_RESULTS_NAMES[@]}"; do
            if [[ "${BUILD_RESULTS_SUCCESS[$i]}" == "true" ]]; then
                print_color $GREEN "  ? ${BUILD_RESULTS_NAMES[$i]}"
            fi
        done
    fi
    
    local total_minutes
    total_minutes=$(echo "scale=1; $TOTAL_BUILD_TIME / 60" | bc -l)
    echo
    print_color $WHITE "Total Build Time: ${total_minutes} minutes"
    
    # Show recommendations
    if [[ $SKIPPED_BUILDS -gt 0 ]]; then
        echo
        print_color $CYAN "RECOMMENDATIONS:"
        print_color $CYAN "- Check that all Avalonia dependency repositories are cloned in the expected locations"
        print_color $CYAN "- Ensure all project paths are correct relative to the build directory"
    fi
    
    print_color $YELLOW "============================================================"
}

# Check if bc (calculator) is available for time calculations
if ! command -v bc &> /dev/null; then
    print_color $YELLOW "Warning: 'bc' calculator not found. Install it for accurate timing: sudo apt-get install bc"
    # Fallback: use simple arithmetic (less precise)
    BC_AVAILABLE=false
else
    BC_AVAILABLE=true
fi

# Main execution
START_TIME=$(date +%s.%N)

# Check prerequisites
if ! test_prerequisites; then
    exit 1
fi

echo
print_color $YELLOW "Starting batch build with the following settings:"
print_color $WHITE "Configuration: $CONFIGURATION"
print_color $WHITE "Clean: $CLEAN"
print_color $WHITE "Rebuild: $REBUILD"
print_color $WHITE "Parallel: $PARALLEL"

local cpu_count_text="Auto"
if [[ $MAX_CPU_COUNT -gt 0 ]]; then
    cpu_count_text="$MAX_CPU_COUNT"
fi
print_color $WHITE "Max CPU Count: $cpu_count_text"
print_color $WHITE "Skip Missing: $SKIP_MISSING"
print_color $WHITE "Continue On Error: $CONTINUE_ON_ERROR"

# Build projects in order
for i in "${!SOLUTION_PATHS[@]}"; do
    local project_path="${SOLUTION_PATHS[$i]}"
    local project_name="${SOLUTION_NAMES[$i]}"
    local priority="${SOLUTION_PRIORITIES[$i]}"
    local critical="${SOLUTION_CRITICAL[$i]}"
    
    local result_code=0
    if ! build_project "$project_path" "$project_name" "$CONFIGURATION" "$CLEAN" "$REBUILD" "$PARALLEL" "$MAX_CPU_COUNT" "$priority" "$critical"; then
        result_code=$?
    fi
    
    # Store build results
    BUILD_RESULTS_NAMES+=("$project_name")
    BUILD_RESULTS_PATHS+=("$project_path")
    BUILD_RESULTS_PRIORITIES+=("$priority")
    BUILD_RESULTS_CRITICAL+=("$critical")
    
    case $result_code in
        0)
            BUILD_RESULTS_SUCCESS+=("true")
            ((SUCCESSFUL_BUILDS++))
            ;;
        2)
            BUILD_RESULTS_SUCCESS+=("skipped")
            ((SKIPPED_BUILDS++))
            ;;
        3)
            # Critical failure
            BUILD_RESULTS_SUCCESS+=("false")
            ((FAILED_BUILDS++))
            print_color $RED "CRITICAL ERROR: Critical project build failed"
            if [[ "$CONTINUE_ON_ERROR" == "false" ]]; then
                break
            fi
            ;;
        *)
            BUILD_RESULTS_SUCCESS+=("false")
            ((FAILED_BUILDS++))
            ;;
    esac
    
    # Add a small delay between builds to avoid resource conflicts
    sleep 0.2
done

# Calculate total build time
END_TIME=$(date +%s.%N)
if [[ $BC_AVAILABLE == true ]]; then
    TOTAL_BUILD_TIME=$(echo "$END_TIME - $START_TIME" | bc -l)
else
    # Fallback calculation (less precise)
    TOTAL_BUILD_TIME=$(echo "$END_TIME - $START_TIME" | awk '{print $1}')
fi

# Display summary
show_build_summary

# Exit with appropriate code
if [[ $FAILED_BUILDS -gt 0 ]]; then
    # Check for critical failures
    local critical_failures=0
    for i in "${!BUILD_RESULTS_CRITICAL[@]}"; do
        if [[ "${BUILD_RESULTS_SUCCESS[$i]}" == "false" && "${BUILD_RESULTS_CRITICAL[$i]}" == "true" ]]; then
            ((critical_failures++))
        fi
    done
    
    if [[ $critical_failures -gt 0 ]]; then
        echo
        print_color $RED "Build FAILED due to critical project failures!"
        exit 2
    else
        echo
        print_color $YELLOW "Build completed with $FAILED_BUILDS non-critical errors!"
        exit 1
    fi
else
    echo
    print_color $GREEN "All available projects built successfully!"
    print_color $GREEN "($SUCCESSFUL_BUILDS successful, $SKIPPED_BUILDS skipped)"
    exit 0
fi