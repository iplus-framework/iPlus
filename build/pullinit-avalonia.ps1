# pullinit-avalonia.ps1
# Second-stage initialization for the Windows VM: clones the Avalonia-related
# repositories that are NOT part of pullinit.ps1 but ARE present on the Linux
# host (/home/damir/SHARED/Devel/iPlusGit/V5) and required by
# build/Build-AvaloniaProjects.ps1.
#
# Run from D:\Devel\iPlusGit\V5 (the repos are cloned into the current dir).
#
# Remote layout (same convention as pullinit.ps1):
#   origin   -> git@github.com:iplus-framework/<repo>.git   (renamed to originV5)
#   originV5 -> git@github.com:iplus-framework/<repo>.git   (fetch only)
#   originV4 -> ssh://dlisak@kajbum.com:/home/git/V4/<repo>.git (push disabled)
#
# NOTE: iPlugIn-EInvoice, iPlugIn-LeibingerJet3 and iPlugIn-SveRacun exist only
# on the V4 server (no GitHub origin) - clone them manually if needed:
#   git clone ssh://dlisak@kajbum.com:/home/git/V4/iPlugIn-EInvoice.git
# NOTE: roslynpad-d6c079 on Linux is a plain copy without .git - copy it manually
# or clone roslynpad and check out the d6c079 commit if that fork is needed.

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
    git clone "git@github.com:iplus-framework/$Repo.git" "$Dir"
    if ($LASTEXITCODE -ne 0) { Write-Host "! clone failed: $Repo" -ForegroundColor Red; return }

    Push-Location $Dir
    git remote rename origin originV5
    git remote add originV4 "ssh://dlisak@kajbum.com:/home/git/V4/$Repo.git"
    git remote set-url --push originV4 no_push
    Pop-Location
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

# --- Additional repos present on the Linux host ---
Clone-Repo "Avalonia.HtmlRenderer"
Clone-Repo "XamlX"                       # Avalonia submodule dependency (external/XamlX)
Clone-Repo "ZUGFeRD-csharp"
Clone-Repo "iplus-documents.io"
Clone-Repo "iPlus-Examples"
Clone-Repo "iPlugIn-Linx"

# efcore is checked out three times on Linux (different branches):
Clone-Repo "efcore" "ef_main_iPlus"
Clone-Repo "efcore" "ef_90_iPlus"
Clone-Repo "efcore" "ef_main_release90"

# --- Repos with submodules (see .gitmodules on the Linux host) ---
# Avalonia.Controls.WebView records its build-common submodule as a relative URL
# (../build-common/), which resolves to the non-existent
# iplus-framework/build-common. Override it with the real upstream repo
# (same commit as Avalonia.Labs / Avalonia.Controls.DataGrid use).
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

Write-Host "`nAll done. Notes:" -ForegroundColor Green
Write-Host " - iPlugIn-EInvoice / iPlugIn-LeibingerJet3 / iPlugIn-SveRacun are V4-only (see header)"
Write-Host " - roslynpad's vendor/roslyn submodule is a multi-GB checkout (dotnet/roslyn)"
