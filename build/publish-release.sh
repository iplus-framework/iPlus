#!/usr/bin/env bash
#
# Publishes the ready-to-run iPlus binaries for every platform into ./artifacts/release
# and writes SHA-256 checksums. The output is meant to be attached to a GitHub Release
# (see .github/workflows/release.yml).
#
# This is the *binary* channel. The *source/developer* channel is build/pack-local.sh
# (NuGet packages). One artifact must not serve both audiences.
#
# Usage:
#   ./build/publish-release.sh [options]
#
# Options:
#   -v, --version <ver>     Suite version stamped into the artifact names
#                           (default: IPlusPackageVersion from build/iPlus.Package.props)
#   -o, --output <dir>      Output directory (default: ./artifacts/release)
#   -p, --platform <list>   Comma separated platforms to build:
#                           desktop-win,desktop-linux,browser,android,service
#                           (default: desktop-win,desktop-linux,browser,service)
#   -c, --configuration     Build configuration (default: Release)
#       --self-contained    Publish self-contained (default)
#       --no-self-contained Publish framework-dependent
#       --skip-checksums    Do not write SHA256SUMS
#       --keep-intermediate Keep the raw publish folders next to the archives
#   -h, --help              Show this help
#
# Environment:
#   IPLUS_USE_AVALONIA_FORK   true|false (default: false)
#                             false = consume the iPlus.* Avalonia packages from nuget.org
#                             (required for CI and for anyone without the fork checkout)
#   IPLUS_BUILD_ANDROID_TFM   true|false (default: false)
#                             gip.core.autocomponent adds an -android TFM on Linux; that
#                             breaks non-Android publishes, so it is disabled here.
#                             The android platform enables it automatically.
#
# Requirements:
#   - .NET 10 SDK
#   - workloads: wasm-tools (browser), android (android)
#   - sqlpackage on PATH only for the database script step (see gip.tool.dbscript)
#
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CONFIG="Release"
OUT="$ROOT/artifacts/release"
PLATFORMS="desktop-win,desktop-linux,browser,service"
SELF_CONTAINED="true"
SKIP_CHECKSUMS="false"
KEEP_INTERMEDIATE="false"
VERSION=""

USE_AVALONIA_FORK="${IPLUS_USE_AVALONIA_FORK:-false}"
BUILD_ANDROID_TFM="${IPLUS_BUILD_ANDROID_TFM:-false}"

usage() {
	awk 'NR > 1 { if ($0 !~ /^#/) exit; sub(/^# ?/, ""); print }' "${BASH_SOURCE[0]}"
}

while [ "$#" -gt 0 ]; do
	case "$1" in
		-v|--version) VERSION="${2:?missing value for $1}"; shift 2 ;;
		-o|--output) OUT="${2:?missing value for $1}"; shift 2 ;;
		-p|--platform) PLATFORMS="${2:?missing value for $1}"; shift 2 ;;
		-c|--configuration) CONFIG="${2:?missing value for $1}"; shift 2 ;;
		--self-contained) SELF_CONTAINED="true"; shift ;;
		--no-self-contained) SELF_CONTAINED="false"; shift ;;
		--skip-checksums) SKIP_CHECKSUMS="true"; shift ;;
		--keep-intermediate) KEEP_INTERMEDIATE="true"; shift ;;
		-h|--help) usage; exit 0 ;;
		*) echo "Unknown option: $1" >&2; usage >&2; exit 1 ;;
	esac
done

if [ -z "$VERSION" ]; then
	VERSION="$(sed -n 's/.*<IPlusPackageVersion Condition=.*>\([^<]*\)<\/IPlusPackageVersion>.*/\1/p' \
		"$ROOT/build/iPlus.Package.props" | head -1)"
	VERSION="${VERSION:-5.0.0}"
fi

mkdir -p "$OUT"
WORK="$OUT/.work"
rm -rf "$WORK"
mkdir -p "$WORK"

log() { echo "==> $*"; }

# Creates a zip archive of a publish folder and removes the folder afterwards
# (unless --keep-intermediate was given).
archive() {
	local source="$1"
	local name="$2"
	local target="$OUT/$name.zip"
	log "Archiving $name"
	rm -f "$target"
	(cd "$(dirname "$source")" && zip -qr "$target" "$(basename "$source")")
	if [ "$KEEP_INTERMEDIATE" != "true" ]; then
		rm -rf "$source"
	fi
}

# Publishes a project. Extra arguments are appended to the dotnet publish call.
publish() {
	local project="$1"
	local dir="$2"
	shift 2
	dotnet publish "$ROOT/$project" \
		-c "$CONFIG" \
		-p:UseAvaloniaFork="$USE_AVALONIA_FORK" \
		-p:BuildAndroidTargetFramework="$BUILD_ANDROID_TFM" \
		-p:Version="$VERSION" \
		-p:ContinuousIntegrationBuild=true \
		-p:ErrorOnDuplicatePublishOutputFiles=false \
		-o "$dir" \
		"$@"
}

publish_desktop() {
	local rid="$1"
	local name="iplus-$VERSION-desktop-$rid"
	local dir="$WORK/$name"
	log "Publishing desktop ($rid)"
	publish "gip.iplus.client.avui.Desktop/gip.iplus.client.avui.Desktop.csproj" "$dir" \
		-r "$rid" --self-contained "$SELF_CONTAINED" -p:PublishSingleFile=false
	archive "$dir" "$name"
}

publish_browser() {
	local name="iplus-$VERSION-browser"
	local dir="$WORK/$name"
	log "Publishing browser (wasm)"
	publish "gip.iplus.client.avui.Browser/gip.iplus.client.avui.Browser.csproj" "$dir"
	archive "$dir" "$name"
}

publish_android() {
	local name="iplus-$VERSION-android"
	local dir="$WORK/$name"
	log "Publishing android (apk)"
	# The Android client needs the -android TFM of gip.core.autocomponent.
	BUILD_ANDROID_TFM="true" publish \
		"gip.iplus.client.avui.Android/gip.iplus.client.avui.Android.csproj" "$dir" \
		-p:AndroidPackageFormats=apk
	# The APK is the only artifact end users need.
	local apk
	apk="$(find "$dir" -name '*.apk' -type f | head -1)"
	if [ -z "$apk" ]; then
		echo "ERROR: no .apk produced for android" >&2
		return 1
	fi
	cp "$apk" "$OUT/$name.apk"
	if [ "$KEEP_INTERMEDIATE" != "true" ]; then
		rm -rf "$dir"
	fi
}

publish_service() {
	local rid="$1"
	local name="iplus-$VERSION-service-$rid"
	local dir="$WORK/$name"
	log "Publishing service ($rid)"
	publish "gip.iplus.service/gip.iplus.service.csproj" "$dir" \
		-r "$rid" --self-contained "$SELF_CONTAINED"
	archive "$dir" "$name"
}

IFS=',' read -r -a REQUESTED <<<"$PLATFORMS"
for platform in "${REQUESTED[@]}"; do
	case "$platform" in
		desktop-win) publish_desktop win-x64 ;;
		desktop-linux) publish_desktop linux-x64 ;;
		browser) publish_browser ;;
		android) publish_android ;;
		service) publish_service linux-x64 ;;
		*) echo "Unknown platform: $platform" >&2; exit 1 ;;
	esac
done

if [ "$SKIP_CHECKSUMS" != "true" ]; then
	log "Writing SHA256SUMS"
	(
		cd "$OUT"
		# Collect the artifacts explicitly so that a missing platform (e.g. no .apk
		# when android was not built) does not make the checksum command fail.
		files=()
		for f in ./*.zip ./*.apk; do
			[ -e "$f" ] && files+=("$f")
		done
		if [ "${#files[@]}" -eq 0 ]; then
			echo "ERROR: no artifacts were produced" >&2
			exit 1
		fi
		# Portable checksum generation (macOS has shasum, not sha256sum).
		if command -v sha256sum >/dev/null 2>&1; then
			sha256sum "${files[@]}" | sed 's| \./| |'
		else
			shasum -a 256 "${files[@]}" | sed 's| \./| |'
		fi
	) > "$OUT/SHA256SUMS"
fi

if [ "$KEEP_INTERMEDIATE" != "true" ]; then
	rm -rf "$WORK"
fi

log "Done. Artifacts in $OUT"
ls -1 "$OUT"
