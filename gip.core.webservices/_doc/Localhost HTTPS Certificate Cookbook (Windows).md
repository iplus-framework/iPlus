# Localhost HTTPS Certificate Cookbook (Windows)

This guide shows how to create a local development certificate for `https://localhost` and bind it to your iPlus JSON service port.

> Commands are for **PowerShell**. Run PowerShell as **Administrator** for `netsh` steps.

## 1) Create a self-signed certificate for localhost + computer name + IP

Replace placeholders before running:
- `<my-computer-name>` (example: `DEVBOX01`)
- `<my-ip-address>` (example: `192.168.1.50`)

```powershell
$cert = New-SelfSignedCertificate `
  -DnsName "localhost","<my-computer-name>","<my-ip-address>" `
  -CertStoreLocation "cert:\LocalMachine\My" `
  -FriendlyName "iPlus Localhost Dev TLS" `
  -NotAfter (Get-Date).AddYears(5) `
  -KeyExportPolicy Exportable `
  -KeyAlgorithm RSA `
  -KeyLength 2048 `
  -HashAlgorithm SHA256

$cert.Thumbprint
```

## 2) Trust the certificate on this machine

```powershell
New-Item -ItemType Directory -Force C:\temp | Out-Null
Export-Certificate -Cert $cert -FilePath C:\temp\localhost-dev.cer | Out-Null
Import-Certificate -FilePath C:\temp\localhost-dev.cer -CertStoreLocation "cert:\LocalMachine\Root" | Out-Null
```

This imports the cert into **Trusted Root Certification Authorities** (Local Machine).

## 3) Bind certificate to HTTPS port (HTTP.SYS)

Choose your service port (example: `8740`).

```powershell
$thumb = $cert.Thumbprint
$appId = "{11111111-2222-3333-4444-555555555555}"   # any stable GUID for your app

netsh http add sslcert hostnameport=localhost:8740 certhash=$thumb certstorename=MY appid=$appId
netsh http add sslcert hostnameport=<my-computer-name>:8740 certhash=$thumb certstorename=MY appid=$appId
netsh http add sslcert ipport=<my-ip-address>:8740 certhash=$thumb certstorename=MY appid=$appId
```

If an old binding exists, remove it first:

```powershell
netsh http delete sslcert hostnameport=localhost:8740
netsh http delete sslcert hostnameport=<my-computer-name>:8740
netsh http delete sslcert ipport=<my-ip-address>:8740
```

## 4) (If needed) reserve URL ACL for non-admin service run

If service start fails with access denied, reserve the URL:

```powershell
netsh http add urlacl url=https://localhost:8740/ user=Everyone
netsh http add urlacl url=https://127.0.0.1:8740/ user=Everyone
netsh http add urlacl url=https://<my-computer-name>:8740/ user=Everyone
netsh http add urlacl url=https://<my-ip-address>:8740/ user=Everyone
```

If already present and you need to recreate:

```powershell
netsh http delete urlacl url=https://localhost:8740/
netsh http delete urlacl url=https://127.0.0.1:8740/
netsh http delete urlacl url=https://<my-computer-name>:8740/
netsh http delete urlacl url=https://<my-ip-address>:8740/
```

## 5) Verify binding

```powershell
netsh http show sslcert hostnameport=localhost:8740
netsh http show sslcert hostnameport=<my-computer-name>:8740
netsh http show sslcert ipport=<my-ip-address>:8740
```

## 6) Enable HTTPS in iPlus + test client

1. In `PAJsonServiceHost`, set config flag:
   - `UseHTTPS = true`
2. In test client (`NavJsonTestServiceClient`), pass:
   - `useHttps = true`
3. Test endpoint with:

```powershell
Invoke-WebRequest https://localhost:8740/ -UseBasicParsing
```

## Notes / pitfalls

- Certificate name must match host: use `localhost` in URL.
- If you call by computer name or IP, they must be included in certificate SAN.
- `127.0.0.1` is different from `localhost` and should also be included in SAN if you use it as URL host.
- For OAuth/JWT, HTTPS is strongly recommended in all environments.
- **HTTP ↔ HTTPS URL ACL conflict**: If a previous (non-HTTPS) setup registered `http://+:<port>/`, WCF will fail to start with `HttpListenerException: Failed to listen on prefix 'https://+:<port>/' because it conflicts with an existing registration`. The `generate-certificate.ps1` script automatically removes old `http://` URL ACLs before creating `https://` ones. If you encounter this manually, delete the stale entry: `netsh http delete urlacl url=http://+:<port>/`.
- WCF `WebServiceHost` internally converts a hostname-based BaseAddress (e.g. `https://mypc:8740/`) into a strong wildcard `https://+:8740/` when registering with HTTP.sys. That is why the `+` wildcard SSL binding and URL ACL are needed.

## Cleanup (optional)

To fully remove the certificate, SSL bindings and URL ACL entries use the `remove-certificate.ps1` script (see below).

Manual single-binding removal example:

```powershell
netsh http delete sslcert hostnameport=localhost:8740
Remove-Item C:\temp\localhost-dev.cer -ErrorAction SilentlyContinue
```

## Script usage (recommended)

Instead of running all manual steps, you can use the provided scripts.

### generate-certificate.ps1

- `gip.core.webservices\_doc\generate-certificate.ps1`

Run PowerShell as **Administrator**, open this folder, then run:

```powershell
.\generate-certificate.ps1 -Hosts localhost,<my-computer-name>,<my-ip-address> -Port 8740
```

Example with concrete values:

```powershell
.\generate-certificate.ps1 -Hosts localhost,DEVBOX01,192.168.1.50 -Port 8740
```

Optional parameters:

- `-FriendlyName "iPlus Dev TLS"`
- `-AppId "11111111-2222-3333-4444-555555555555"`
- `-ValidYears 5`
- `-SkipUrlAcl` (if URL ACL entries are not needed)
- `-IncludeWildcardPlus` — also creates a wildcard SSL binding (`ipport=0.0.0.0:<port>`) and URL ACL for `https://+:<port>/`, useful when the service listens on all interfaces

The script will:

1. Create a self-signed certificate with SAN entries for all hosts.
2. Trust it in `LocalMachine\Root`.
3. Create/replace `sslcert` bindings for each host on the selected port.
4. (If `-IncludeWildcardPlus`) create/replace the wildcard `0.0.0.0` SSL binding and `https://+:<port>/` URL ACL.
5. Remove any old `http://` URL ACL entries for the same hosts/port (prevents HTTP↔HTTPS conflict).
6. Create/replace `https://` URL ACL entries (unless `-SkipUrlAcl` is used).

After script finishes, test with:

```powershell
Invoke-WebRequest https://localhost:8740/ -UseBasicParsing
```

### remove-certificate.ps1

- `gip.core.webservices\_doc\remove-certificate.ps1`

Removes SSL bindings, URL ACL entries, and the certificate from `LocalMachine\My` and `LocalMachine\Root`.

```powershell
.\remove-certificate.ps1 -Hosts localhost,<my-computer-name>,<my-ip-address> -Port 8740
```

Example with concrete values:

```powershell
.\remove-certificate.ps1 -Hosts localhost,DEVBOX01,192.168.1.50 -Port 8740
```

Optional parameters:

- `-FriendlyName "iPlus Dev TLS"` — used to find matching certificates by friendly name (default)
- `-Thumbprint <hex>` — remove a specific certificate by thumbprint instead of by friendly name
- `-SkipUrlAcl` (skip URL ACL removal)
- `-IncludeWildcardPlus` — also removes the wildcard SSL binding (`ipport=0.0.0.0:<port>`) and URL ACL for `https://+:<port>/`
- `-WhatIf` — preview changes without actually removing anything

The script removes URL ACL entries for **both** `https://` and `http://` schemes (cleans up legacy non-HTTPS registrations).

