#!/usr/bin/env bash
#
# pullinit-community.sh
# One-time setup for external contributors: clones all repositories needed to
# compile iPlus / iPlusMES together with the Avalonia fork sources.
#
# Usage:
#   1. Create a working folder, e.g. ~/Devel/iPlusGit/V5
#   2. Clone this repo (iPlus) into it, or download this script
#   3. Run:  ./pullinit-community.sh
#   4. Build with:  iPlus/build/build-avalonia-projects.sh
#
# All repositories are cloned from the public GitHub organization
# https://github.com/iplus-framework - no VPN / V4 server access required.
#
# The script is idempotent: existing repositories are skipped, so it is safe
# to re-run at any time.

set -u
cd "$(dirname "$0")"

clone_repo() {
    local repo="$1"
    local dir="${2:-$1}"

    if [ -d "$dir/.git" ]; then
        echo "= SKIP (already cloned): $dir"
        return
    fi

    echo "+ Cloning $repo -> $dir"
    if ! git clone "https://github.com/iplus-framework/$repo.git" "$dir"; then
        echo "! clone failed: $repo"
        return
    fi
    echo "? Done: $dir"
}

# --- Required by build-avalonia-projects.sh ---
clone_repo "Avalonia"                    # Avalonia Core (critical)
clone_repo "AvDialogHost.Avalonia"       # Dialog Host
clone_repo "AvaloniaEdit"                # Avalonia Edit
clone_repo "Avalonia.Dock"               # Avalonia Dock
clone_repo "roslynpad"                   # Roslyn Pad
clone_repo "Avalonia.Labs"               # Avalonia Labs
clone_repo "SVG"                         # SVG
clone_repo "Avalonia.Controls.DataGrid"  # DataGrid Controls
clone_repo "Xaml.Behaviors"              # XAML Behaviors
clone_repo "AvSvg.Skia"                  # SVG Skia
clone_repo "AvRichTextBox"               # Rich Text Box
clone_repo "AvMarkdown.Avalonia"         # Markdown Avalonia
clone_repo "AvMessageBox.Avalonia"       # Message Box
clone_repo "avOxyplot-avalonia"          # OxyPlot Avalonia
clone_repo "scryber.core"                # Scryber Core
clone_repo "Avalonia.Controls.WebView"   # WebView Controls

# --- Additional repositories ---
clone_repo "Avalonia.HtmlRenderer"
clone_repo "XamlX"                       # Avalonia submodule dependency (external/XamlX)
clone_repo "ZUGFeRD-csharp"
clone_repo "iplus-documents.io"
clone_repo "iPlus-Examples"

# --- Repositories with submodules ---
# Avalonia.Controls.WebView records its build-common submodule as a relative URL
# (../build-common/), which resolves to the non-existent
# iplus-framework/build-common. Override it with the real upstream repo.
if [ -d "Avalonia.Controls.WebView/.git" ]; then
    git -C Avalonia.Controls.WebView config submodule.build-common.url https://github.com/AvaloniaUI/build-common.git
fi

for repo in Avalonia AvSvg.Skia roslynpad Avalonia.Labs Avalonia.Controls.DataGrid Avalonia.Controls.WebView; do
    if [ -d "$repo/.git" ]; then
        echo "+ Initializing submodules: $repo"
        if ! git -C "$repo" submodule update --init --recursive; then
            echo "! submodule init failed for $repo (re-run: git -C $repo submodule update --init --recursive)"
        fi
    fi
done

echo
echo "All repositories are ready."
echo "Next steps:"
echo "  1. Set UseAvaloniaFork=True in iPlus/build/Avalonia.Version.props"
echo "  2. dotnet workload restore  (in the Avalonia repo)"
echo "  3. iPlus/build/build-avalonia-projects.sh --configuration Release"
echo
echo "Note: roslynpad's vendor/roslyn submodule is a multi-GB checkout (dotnet/roslyn)."
