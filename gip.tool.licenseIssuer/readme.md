# gip.tool.licenseIssuer

Cross-platform command line tool that creates and verifies **offline, signed
iPlus license files** (`*.gip`). It replaces the V4 dongle-bound license manager
(`gip.tool.devLicense` / `gip.tool.devLicenseProvider`, both removed) for
everything that does not need a UI.

## Commands

### `create-keypair`

Creates an RSA key pair (PEM). The private key must be kept offline.

```
gip.tool.licenseIssuer create-keypair -o ./keys -n gip
```

### `issue-developer`

Issues a developer license. `--end-user` issues an end-user development license
(engineering features only) instead of an issuer license (right to issue product
licenses).

An **issuer** license must embed the developer's public key
(`--developer-public-key`). That key is what end customers use to verify the
product licenses the developer signs with the matching private key — the
two-level PKI chain, so no central signing service is needed.

```
gip.tool.licenseIssuer issue-developer \
  --private-key ./keys/gip-private.pem \
  -o ./out/ACME-developer.gip \
  --customer "ACME GmbH" \
  --project-no P-2026-001 \
  --issuer-name gip \
  --developer-public-key ./keys/acme-public.pem \
  --expires 2027-12-31
```

### `issue-runtime`

Issues a **framework runtime license** — the commercial iPlus edition, sold per
concurrent user. Only gip can issue it (it must be signed with gip's private
key). Without it the installation runs under the open (GPL-3) edition and
product licenses are ineffective.

```
gip.tool.licenseIssuer issue-runtime \
  --private-key ./keys/gip-private.pem \
  -o ./out/ACME-runtime.gip \
  --customer "ACME GmbH" \
  --project-no P-2026-001 \
  --max-users 25 \
  --database-fingerprint <fingerprint> \
  --expires 2027-12-31
```

### `issue-product`

Issues a product license bound to a database fingerprint. Signed with the
**developer's** private key; the license stamps `IssuerKeyId` so the framework
can resolve the matching public key from the developer's issuer license.

```
gip.tool.licenseIssuer issue-product \
  --private-key ./keys/acme-private.pem \
  -o ./out/ACME-license.gip \
  --customer "ACME GmbH" \
  --project-no P-2026-001 \
  --issuer-name "ACME GmbH" \
  -p ACME.Mes -p ACME.Mes.Extension \
  --database-fingerprint <fingerprint> \
  --max-users 25
```

### `verify`

Verifies a license file offline. Exit code `0` = valid, `2` = invalid,
`1` = usage/IO error.

```
gip.tool.licenseIssuer verify -l ./out/ACME-license.gip \
  --database-fingerprint <fingerprint> -p ACME.Mes
```

Without `--public-key` the framework key embedded in `gip.core.licensing` is used.
Product licenses signed by a developer key are verified through the issuer chain
(see `LicenseVerifier`); pass `--public-key` with the developer's public key to
verify such a license standalone.

### `show`

Prints the payload of a license file **without** verifying the signature. Useful
for support cases.

## Getting the database fingerprint

The fingerprint is computed from SQL Server metadata:

```sql
SELECT d.create_date, d.service_broker_guid, r.database_guid
FROM sys.databases d
INNER JOIN sys.database_recovery_status r ON d.database_id = r.database_id
WHERE d.name = '<DatabaseName>';
```

Pass the three values to `DatabaseFingerprint.Compute(...)` (in
`gip.core.licensing`) or use the value that the iPlus license manager shows in
the UI.
