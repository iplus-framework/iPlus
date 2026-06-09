# OPC UA on WINE: Handoff Status (2026-06-09)

## Scope
This note summarizes all implemented changes for getting the OPC UA server startup working on WINE for V5, the current known failure point, and where to continue.

## Current Outcome
- Native Linux Avalonia: works.
- Native Windows WPF: works.
- WINE (Windows build on Linux): still failing during OPC UA certificate validation.

## Where the active code is
- Source file:
  - iPlus/gip.core.communication/PLC/OPC/OPCUA/OPCUASrvACService.cs
- Runtime marker currently in code:
  - OPCUA_CERT_PATCH_MARKER=2026-06-09f
- Config file:
  - iPlus/gip.core.communication/OPCUAServer.Config.xml

## High-level diagnosis so far
The certificate and key files are present and discoverable on WINE (.der/.pem/.pfx/.pfx.bak are found by diagnostics), but UA validation still fails on private-key access in the CheckApplicationInstanceCertificates path.

This appears to be a WINE crypto/store behavior mismatch rather than a missing file problem.

## Implemented code changes (already programmed)

### 1) Startup diagnostics and patch marker
- Added detailed certificate diagnostics per phase:
  - after-pin, before-check, after-repair, check-success, after-store-fallback
- Added startup marker log to verify loaded assembly and build timestamp.

### 2) Subject normalization and thumbprint pinning
- Subject normalization S -> ST to avoid lookup mismatch.
- Pinning of most recent valid cert with parseable PEM key.
- Enforced thumbprint-only lookup by clearing subject fields (property + reflected backing fields).

### 3) PEM preference and file repair loop
- Convert private key PFX to PEM where needed.
- Rename PFX to .pfx.bak in PEM-preferred mode.
- Retry certificate check with repair steps across attempts.

### 4) Path and config hardening
- Normalize Directory store path separators to Windows style in runtime config object.
- SupportedPrivateKeyFormats ensured at runtime.
- Config updated to include both PEM and PFX:
  - <SupportedPrivateKeyFormats>
    - PEM
    - PFX

### 5) File matching bug fix
- Replaced wildcard matching using *[thumbprint]* with literal filename contains("[THUMBPRINT]") logic.
- Reason: [] in file-glob patterns is treated as character class and can mis-match.

### 6) WINE fallback path on private-key failure
- On error "Cannot access private key for certificate with thumbprint=...":
  - Trigger fallback that promotes certificate handling to X509Store CurrentUser\\My.
  - Keep fallback mode sticky across retries.

### 7) Latest fallback strategy (marker 2026-06-09f)
- Removed fragile PKCS#12 re-import roundtrip from fallback path.
- Fallback now tries:
  1. Build cert+private key directly from DER+PEM.
  2. Attach cert object directly to CertificateIdentifier.
  3. Attempt store.Add(cert) to CurrentUser\\My as best effort.
  4. If store.Add fails, continue with identifier-attached certificate and still switch identifier to X509Store CurrentUser\\My.

## Last user-verified failing state
Last fully reported failing run was with marker 2026-06-09e:
- Files were found correctly in diagnostics.
- First check failed with: Cannot access private key for certificate with thumbprint=...
- Fallback import failed with:
  - Store fallback skipped: failed to import PKCS12 ...
  - LoadPkcs12(DangerousNoLimits) failed: CryptographicException: Erfolg.

After this, code was updated to marker 2026-06-09f to avoid PKCS#12 re-import and rely on direct certificate attachment/store promotion.
This latest path still needs confirmation logs from a full WINE run.

## Key methods to inspect next
In OPCUASrvACService.cs:
- InitOPCUAApp retry flow and fallback trigger
- TryPromotePinnedCertificateToCurrentUserMy
- TryCreateCertificateWithPrivateKeyFromDerPem
- AttachPinnedCertificatePrivateKey
- PinMostRecentApplicationCertificate
- PreferPemPrivateKeysForApplicationStore
- LogApplicationCertificateDiagnostics

## What to run for continuation
1. Build WINE target (already used):
   - Workspace task: Build Windows (wine) in iPlusMES
2. Start under WINE and collect these logs:
   - Marker line (must show 2026-06-09f)
   - Activated Wine certificate fallback ... attach=... import=...
   - CertDiag phase=after-store-fallback ...
   - CheckApplicationInstanceCertificates attempt=... result=...

## Likely next technical branch if still failing on 2026-06-09f
If fallback still fails even with direct identifier attachment, implement a narrowly-scoped WINE-only bypass of CheckApplicationInstanceCertificates private-key gate, while keeping normal validation behavior unchanged on native Linux/Windows.

## Notes
- iPlusMES runtime uses shared communication assembly from iPlus (gip.core.communication).
- Unrelated runtime exceptions such as ACProperty.RestoreRuntimeValue("00:00:00") are not part of the OPC UA certificate blocker.
