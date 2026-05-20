param(
	[Parameter(Mandatory = $false)]
	[string[]]$Hosts = @("localhost"),

	[Parameter(Mandatory = $false)]
	[int]$Port = 8740,

	[Parameter(Mandatory = $false)]
	[string]$FriendlyName = "iPlus Dev TLS",

	[Parameter(Mandatory = $false)]
	[Guid]$AppId = "11111111-2222-3333-4444-555555555555",

	[Parameter(Mandatory = $false)]
	[int]$ValidYears = 5,

	[Parameter(Mandatory = $false)]
	[switch]$SkipUrlAcl,

	[Parameter(Mandatory = $false)]
	[switch]$IncludeWildcardPlus,

	[Parameter(Mandatory = $false)]
	[string]$AclUser = "Everyone"
)

$ErrorActionPreference = "Stop"

# Resolve localized account name from well-known SID (e.g. "Jeder" on German Windows)
try {
	$resolvedName = ([System.Security.Principal.SecurityIdentifier]::new("S-1-1-0")).Translate([System.Security.Principal.NTAccount]).Value
	if ($AclUser -eq "Everyone" -and $resolvedName -ne "Everyone") {
		Write-Host "Resolved 'Everyone' to localized name: $resolvedName"
		$AclUser = $resolvedName
	}
} catch { }

function Test-IsAdministrator {
	$currentIdentity = [Security.Principal.WindowsIdentity]::GetCurrent()
	$principal = [Security.Principal.WindowsPrincipal]::new($currentIdentity)
	return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Get-BindingKey {
	param([string]$HostName, [int]$Port)

	$parsedIp = $null
	if ([System.Net.IPAddress]::TryParse($HostName, [ref]$parsedIp)) {
		return "ipport=$HostName`:$Port"
	}

	return "hostnameport=$HostName`:$Port"
}

if (-not (Test-IsAdministrator)) {
	throw "Run this script as Administrator (required for LocalMachine cert stores and netsh bindings)."
}

if (-not $Hosts -or $Hosts.Count -eq 0) {
	throw "Provide at least one host in -Hosts."
}

$Hosts = $Hosts | ForEach-Object { $_.Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique
if ($Hosts.Count -eq 0) {
	throw "Hosts list is empty after cleanup."
}

$primaryCn = $Hosts[0]
$sanItems = foreach ($hostName in $Hosts) {
	$parsedIp = $null
	if ([System.Net.IPAddress]::TryParse($hostName, [ref]$parsedIp)) {
		"ipaddress=$hostName"
	}
	else {
		"dns=$hostName"
	}
}

$sanExtension = "2.5.29.17={text}" + ($sanItems -join "&")

Write-Host "Creating self-signed certificate..."
$cert = New-SelfSignedCertificate `
	-Subject "CN=$primaryCn" `
	-TextExtension $sanExtension `
	-CertStoreLocation "cert:\LocalMachine\My" `
	-FriendlyName $FriendlyName `
	-NotAfter (Get-Date).AddYears($ValidYears) `
	-KeyExportPolicy Exportable `
	-KeyAlgorithm RSA `
	-KeyLength 2048 `
	-HashAlgorithm SHA256

$thumb = ($cert.Thumbprint -replace "\s", "")

Write-Host "Trusting certificate in LocalMachine\\Root..."
$exportDir = "C:\temp"
$exportPath = Join-Path $exportDir "localhost-dev.cer"
New-Item -ItemType Directory -Force -Path $exportDir | Out-Null
Export-Certificate -Cert $cert -FilePath $exportPath | Out-Null
Import-Certificate -FilePath $exportPath -CertStoreLocation "cert:\LocalMachine\Root" | Out-Null

if ($IncludeWildcardPlus) {
	$wildcardBindingKey = "ipport=0.0.0.0`:$Port"
	Write-Host "Configuring SSL binding for wildcard prefix (+) via $wildcardBindingKey ..."
	try {
		& netsh http delete sslcert $wildcardBindingKey | Out-Null
	}
	catch {
	}

	& netsh http add sslcert $wildcardBindingKey "certhash=$thumb" "certstorename=MY" "appid={$AppId}" | Out-Null

	if (-not $SkipUrlAcl) {
		$wildcardUrl = "https://+:$Port/"
		$wildcardUrlHttp = "http://+:$Port/"
		Write-Host "Configuring URL ACL for $wildcardUrl ..."
		try { & netsh http delete urlacl "url=$wildcardUrl" 2>&1 | Out-Null } catch { }
		try { & netsh http delete urlacl "url=$wildcardUrlHttp" 2>&1 | Out-Null } catch { }

		& netsh http add urlacl "url=$wildcardUrl" "user=$AclUser" | Out-Null
	}
}

foreach ($hostName in $Hosts) {
	$bindingKey = Get-BindingKey -HostName $hostName -Port $Port

	Write-Host "Configuring SSL binding for $bindingKey ..."
	try {
		& netsh http delete sslcert $bindingKey | Out-Null
	}
	catch {
	}

	& netsh http add sslcert $bindingKey "certhash=$thumb" "certstorename=MY" "appid={$AppId}" | Out-Null

	if (-not $SkipUrlAcl) {
		$url = "https://${hostName}:$Port/"
		$urlHttp = "http://${hostName}:$Port/"
		Write-Host "Configuring URL ACL for $url ..."
		try { & netsh http delete urlacl "url=$url" 2>&1 | Out-Null } catch { }
		try { & netsh http delete urlacl "url=$urlHttp" 2>&1 | Out-Null } catch { }

		& netsh http add urlacl "url=$url" "user=$AclUser" | Out-Null
	}
}

Write-Host ""
Write-Host "Done."
Write-Host "Thumbprint: $thumb"
Write-Host "Hosts: $($Hosts -join ', ')"
Write-Host "Include wildcard (+): $IncludeWildcardPlus"
Write-Host "Port: $Port"
Write-Host "AppId: {$AppId}"
Write-Host "Test example: https://$($Hosts[0]):$Port/"
