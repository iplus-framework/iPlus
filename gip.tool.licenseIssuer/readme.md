# gip.tool.licenseIssuer

Cross-platform command line tool that creates and verifies **offline, signed
iPlus license files** (`*.gip`). It replaces the V4 dongle-bound license manager
(`gip.tool.devLicense` / `gip.tool.devLicenseProvider`, both removed) for
everything that does not need a UI.

## Running the tool

The tool is built into the shared output folder (`bin/Debug/net10.0/` or the
release folder). That folder is **not** on `PATH`, so the executable must be
invoked with an explicit path:

```bash
# from the output folder
./gip.tool.licenseIssuer --help

# from anywhere, using the full path
~/SHARED/Devel/iPlusGit/V5/iPlus/bin/Debug/net10.0/gip.tool.licenseIssuer --help

# or through the .NET host (works on every platform)
dotnet gip.tool.licenseIssuer.dll --help
```

A bare `gip.tool.licenseIssuer ...` fails with `Befehl nicht gefunden` /
`command not found` because the shell only searches `PATH`. All examples below
therefore use the `./` prefix.

## Commands

### `create-keypair`

Creates an RSA key pair (PEM). The private key must be kept offline.

```
./gip.tool.licenseIssuer create-keypair -o ./keys -n gip
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
./gip.tool.licenseIssuer issue-developer \
  --private-key ./keys/gip-private.pem \
  -o ./out/ACME-developer.gip \
  --customer "ACME GmbH" \
  --project-no P-2026-001 \
  --issuer-name gip \
  --developer-public-key ./keys/acme-public.pem \
  --package-namespace acme \
  --expires 2027-12-31
```

`--package-namespace` (repeatable) limits which packages the developer can pick in
`BSOiPlusLicenseManager`: it is the first segment of the `ACPackageName` (`acme` for
`acme.Orders`) or a full package name. Omit it to allow all packages (what gip
itself uses). Without a matching namespace the "Available packages" list is empty.

### `issue-runtime`

Issues a **framework runtime license** — the commercial iPlus edition, sold per
concurrent user. Only gip can issue it (it must be signed with gip's private
key). Without it the installation runs under the open (GPL-3) edition and
product licenses are ineffective.

```
./gip.tool.licenseIssuer issue-runtime \
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
can resolve the matching public key. With `--issuer-license` (the developer's
gip-signed `DeveloperIssuer` license) that license is embedded in the product
license, so the customer only needs to install this one file.

```
./gip.tool.licenseIssuer issue-product \
  --private-key ./keys/acme-private.pem \
  -o ./out/ACME-license.gip \
  --customer "ACME GmbH" \
  --project-no P-2026-001 \
  --issuer-name "ACME GmbH" \
  -p ACME.Mes -p ACME.Mes.Extension \
  --database-fingerprint <fingerprint> \
  --issuer-license ./keys/ACME-developer.gip \
  --max-users 25
```

### `verify`

Verifies a license file offline. Exit code `0` = valid, `2` = invalid,
`1` = usage/IO error.

```
./gip.tool.licenseIssuer verify -l ./out/ACME-license.gip \
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

The customer never has to know the fingerprint. In the license dialog of the
iPlus client they read the activation data and export a file
(`<Customer>-activation data.gip`), which they send to you. You get the
fingerprint from it:

```
./gip.tool.licenseIssuer fingerprint -a "Customer-activation data.gip"
```

Pass the printed value as `--database-fingerprint` to `issue-runtime` /
`issue-product`, or skip this step and pass the file directly with
`--activation-file "Customer-activation data.gip"` (`-a`). The iPlus license manager (`BSOiPlusLicenseManager`) does the same
when you import the activation file.
