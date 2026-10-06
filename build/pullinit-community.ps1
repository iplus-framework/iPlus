# pullinit-community.ps1
# One-time setup for external contributors: clones all repositories needed to
# compile iPlus / iPlusMES together with the Avalonia fork sources.
#
# Usage:
#   1. Create a working folder, e.g. C:\Devel\iPlusGit\V5
#   2. Clone this repo (iPlus) into it, or download this script
#   3. Run:  .\pullinit-community.ps1
#   4. Build with:  iPlus\build\Build-AvaloniaProjects.ps1
#
# All repositories are cloned from the public GitHub organization
# https://github.com/iplus-framework - no VPN / V4 server access required.
#
# The script is idempotent: existing repositories are skipped, so it is safe
# to re-run at any time.

$ErrorActionPreference = "Continue"

# Windows has a 260-char path limit by default; roslynpad's vendor/roslyn
# submodule contains longer paths. Enable long paths for git (needs admin once).
if ((git config --system core.longpaths) -ne "true") {
    git config --system core.longpaths true
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Enabled git core.longpaths (system)" -ForegroundColor Green
    } else {
        Write-Host "! Could not set core.longpaths (run this script as Administrator once)" -ForegroundColor Red
        Write-Host "  or set it manually: git config --system core.longpaths true" -ForegroundColor Red
    }
}

function Clone-Repo {
    param([string]$Repo, [string]$Dir = $Repo)

    if (Test-Path "$Dir/.git") {
        Write-Host "= SKIP (already cloned): $Dir" -ForegroundColor Yellow
        return
    }

    Write-Host "+ Cloning $Repo -> $Dir" -ForegroundColor Cyan
    git clone "https://github.com/iplus-framework/$Repo.git" "$Dir"
    if ($LASTEXITCODE -ne 0) { Write-Host "! clone failed: $Repo" -ForegroundColor Red; return }
    Write-Host "? Done: $Dir" -ForegroundColor Green
}

# --- Required by Build-AvaloniaProjects.ps1 ---
Clone-Repo "Avalonia"                    # Avalonia Core (critical)
Clone-Repo "AvDialogHost.Avalonia"       # Dialog Host
Clone-Repo "AvaloniaEdit"                # Avalonia Edit
Clone-Repo "Avalonia.Dock"               # Avalonia Dock
Clone-Repo "roslynpad"                   # Roslyn Pad
Clone-Repo "Avalonia.Labs"               # Avalonia Labs
Clone-Repo "SVG"                         # SVG
Clone-Repo "Avalonia.Controls.DataGrid"  # DataGrid Controls
Clone-Repo "Xaml.Behaviors"              # XAML Behaviors
Clone-Repo "AvSvg.Skia"                  # SVG Skia
Clone-Repo "AvRichTextBox"               # Rich Text Box
Clone-Repo "AvMarkdown.Avalonia"         # Markdown Avalonia
Clone-Repo "AvMessageBox.Avalonia"       # Message Box
Clone-Repo "avOxyplot-avalonia"          # OxyPlot Avalonia
Clone-Repo "scryber.core"                # Scryber Core
Clone-Repo "Avalonia.Controls.WebView"   # WebView Controls

# --- Additional repositories ---
Clone-Repo "Avalonia.HtmlRenderer"
Clone-Repo "XamlX"                       # Avalonia submodule dependency (external/XamlX)
Clone-Repo "ZUGFeRD-csharp"
Clone-Repo "iplus-documents.io"
Clone-Repo "iPlus-Examples"

# --- Repositories with submodules ---
# Avalonia.Controls.WebView records its build-common submodule as a relative URL
# (../build-common/), which resolves to the non-existent
# iplus-framework/build-common. Override it with the real upstream repo.
$WebViewDir = "Avalonia.Controls.WebView"
if (Test-Path "$WebViewDir/.git") {
    git -C $WebViewDir config submodule.build-common.url https://github.com/AvaloniaUI/build-common.git
}

foreach ($repo in "Avalonia","AvSvg.Skia","roslynpad","Avalonia.Labs","Avalonia.Controls.DataGrid","Avalonia.Controls.WebView") {
    if (Test-Path "$repo/.git") {
        Write-Host "+ Initializing submodules: $repo" -ForegroundColor Cyan
        git -C $repo submodule update --init --recursive
        if ($LASTEXITCODE -ne 0) {
            Write-Host "! submodule init failed for $repo (re-run: git -C $repo submodule update --init --recursive)" -ForegroundColor Red
        }
    }
}

Write-Host ""
Write-Host "All repositories are ready." -ForegroundColor Green
Write-Host "Next steps:"
Write-Host "  1. Set UseAvaloniaFork=True in iPlus/build/Avalonia.Version.props"
Write-Host "  2. dotnet workload restore  (in the Avalonia repo)"
Write-Host "  3. iPlus\build\Build-AvaloniaProjects.ps1 -Configuration Release"
Write-Host ""
Write-Host "Note: roslynpad's vendor/roslyn submodule is a multi-GB checkout (dotnet/roslyn)."
