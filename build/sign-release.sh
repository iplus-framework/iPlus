#!/usr/bin/env bash
#
# Signs the binaries produced by build/publish-release.sh with an Authenticode
# certificate, so that Windows SmartScreen and the runtime's publisher
# verification (gip.core.licensing.TrustedPublisherStore) can tell who built
# them.
#
# Authenticode is the only mechanism that proves *who built* a binary. The
# strong name key (GIP-SNKey.snk) is public, and the iPlusLicense assembly
# metadata is a plain string — neither is a trust signal.
#
# Usage:
#   ./build/sign-release.sh [options]
#
# Options:
#   -m, --mode <mode>       none | self-signed | azure   (default: none)
#   -d, --directory <dir>   Folder with the published binaries
#                           (default: ./artifacts/release)
#   -f, --pfx <file>        Path to a .pfx certificate (self-signed mode)
#   -p, --password <pw>     Password of the .pfx (self-signed mode)
#       --timestamp <url>   RFC3161 timestamp server
#                           (default: http://timestamp.digicert.com)
#   -h, --help              Show this help
#
# Environment (self-signed):
#   WINDOWS_CERTIFICATE_BASE64    Base64 encoded .pfx (CI secret)
#   WINDOWS_CERTIFICATE_PASSWORD  Password of that .pfx
#
# Environment (azure):
#   AZURE_TENANT_ID, AZURE_CLIENT_ID, AZURE_CLIENT_SECRET,
#   AZURE_SIGNING_ENDPOINT, AZURE_SIGNING_ACCOUNT, AZURE_SIGNING_PROFILE
#
# Requirements:
#   - Windows with signtool.exe on PATH (self-signed)
#   - Azure CLI logged in with the Trusted Signing extension (azure)
#
# Notes:
#   - Authenticode only exists on Windows. On Linux/macOS this script reports
#     that signing is not applicable and exits successfully, so that a
#     cross-platform release pipeline does not fail.
#   - The runtime treats an unsigned assembly as "not verifiable" and allows it
#     unless publisher verification is set to "enforce".
#
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
MODE="none"
DIRECTORY="$ROOT/artifacts/release"
PFX=""
PASSWORD=""
TIMESTAMP="http://timestamp.digicert.com"

usage() {
	awk 'NR > 1 { if ($0 !~ /^#/) exit; sub(/^# ?/, ""); print }' "${BASH_SOURCE[0]}"
}

log() {
	echo "==> $*"
}

while [ $# -gt 0 ]; do
	case "$1" in
		-m|--mode) MODE="$2"; shift 2 ;;
		-d|--directory) DIRECTORY="$2"; shift 2 ;;
		-f|--pfx) PFX="$2"; shift 2 ;;
		-p|--password) PASSWORD="$2"; shift 2 ;;
		--timestamp) TIMESTAMP="$2"; shift 2 ;;
		-h|--help) usage; exit 0 ;;
		*) echo "Unknown option: $1" >&2; exit 1 ;;
	esac
done

case "$MODE" in
	none)
		log "Signing disabled (SIGNING_MODE=none). Nothing to do."
		exit 0
		;;
	self-signed|azure) ;;
	*) echo "Unknown signing mode: $MODE" >&2; exit 1 ;;
esac

if [ ! -d "$DIRECTORY" ]; then
	echo "ERROR: directory not found: $DIRECTORY" >&2
	exit 1
fi

if [ "$(uname -s)" != "Windows_NT" ] && [ "${OS:-}" != "Windows_NT" ]; then
	log "Authenticode signing is only available on Windows. Skipping on $(uname -s)."
	exit 0
fi

# Collect the files to sign. Only PE files can carry an Authenticode signature.
mapfile -t FILES < <(find "$DIRECTORY" -type f \( -name '*.exe' -o -name '*.dll' \) | sort)
if [ "${#FILES[@]}" -eq 0 ]; then
	log "No .exe/.dll files found in $DIRECTORY. Nothing to sign."
	exit 0
fi

log "Signing ${#FILES[@]} file(s) in $DIRECTORY (mode: $MODE)"

if [ "$MODE" = "self-signed" ]; then
	if [ -z "$PFX" ]; then
		if [ -n "${WINDOWS_CERTIFICATE_BASE64:-}" ]; then
			PFX="$(mktemp --suffix=.pfx)"
			# shellcheck disable=SC2005
			echo "${WINDOWS_CERTIFICATE_BASE64}" | base64 -d > "$PFX"
			PASSWORD="${WINDOWS_CERTIFICATE_PASSWORD:-}"
		else
			echo "ERROR: self-signed mode needs --pfx or WINDOWS_CERTIFICATE_BASE64." >&2
			exit 1
		fi
	fi

	if [ ! -f "$PFX" ]; then
		echo "ERROR: certificate not found: $PFX" >&2
		exit 1
	fi

	for file in "${FILES[@]}"; do
		signtool.exe sign /f "$PFX" /p "$PASSWORD" /fd SHA256 \
			/tr "$TIMESTAMP" /td SHA256 "$file"
	done
else
	# Azure Artifact Signing (formerly Trusted Signing).
	: "${AZURE_SIGNING_ENDPOINT:?AZURE_SIGNING_ENDPOINT is required}"
	: "${AZURE_SIGNING_ACCOUNT:?AZURE_SIGNING_ACCOUNT is required}"
	: "${AZURE_SIGNING_PROFILE:?AZURE_SIGNING_PROFILE is required}"

	for file in "${FILES[@]}"; do
		az trustedsigning sign \
			--endpoint "$AZURE_SIGNING_ENDPOINT" \
			--account "$AZURE_SIGNING_ACCOUNT" \
			--profile "$AZURE_SIGNING_PROFILE" \
			--file "$file" \
			--timestamp "http://timestamp.acs.microsoft.com"
	done
fi

log "Signing finished."

# Print the thumbprints so that they can be added to
# Licenses/TrustedPublishers.txt.
if command -v powershell.exe >/dev/null 2>&1 || command -v pwsh >/dev/null 2>&1; then
	log "Signer thumbprints (add to Licenses/TrustedPublishers.txt):"
	for file in "${FILES[@]}"; do
		powershell.exe -NoProfile -Command \
			"(Get-AuthenticodeSignature -FilePath '$file').SignerCertificate.Thumbprint" 2>/dev/null \
			| tr -d '\r' | sed '/^$/d' | head -1
	done | sort -u
fi
