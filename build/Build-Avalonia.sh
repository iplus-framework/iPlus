#!/bin/bash
# Simple wrapper script that calls the main build script
# This provides consistency with the Windows .bat file naming convention

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
exec "$SCRIPT_DIR/build-avalonia-projects.sh" "$@"