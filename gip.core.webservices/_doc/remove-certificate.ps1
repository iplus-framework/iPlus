[CmdletBinding(SupportsShouldProcess = $true)]
param(
	[Parameter(Mandatory = $false)]
	[string[]]$Hosts = @("localhost"),

	[Parameter(Mandatory = $false)]
	[int]$Port = 8740,

	[Parameter(Mandatory = $false)]
	[string]$FriendlyName = "iPlus Dev TLS",

	[Parameter(Mandatory = $false)]
	[string]$Thumbprint,

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

function Remove-CertByThumbprint {
	param([string]$StorePath, [string]$CertThumbprint)

	$normalized = ($CertThumbprint -replace "\s", "").ToUpperInvariant()
	$matches = Get-ChildItem -Path $StorePath | Where-Object { $_.Thumbprint -eq $normalized }

	foreach ($cert in $matches) {
		if ($PSCmdlet.ShouldProcess("$StorePath\\$($cert.Thumbprint)", "Remove certificate")) {
			Remove-Item -Path "$StorePath\\$($cert.Thumbprint)" -Force
			Write-Host "Removed certificate from ${StorePath}: $($cert.Thumbprint)"
		}
	}
}

if (-not (Test-IsAdministrator)) {
	throw "Run this script as Administrator (required for netsh and LocalMachine certificate stores)."
}

$Hosts = $Hosts | ForEach-Object { $_.Trim() } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique
if (-not $Hosts -or $Hosts.Count -eq 0) {
	throw "Provide at least one host in -Hosts."
}

if ($IncludeWildcardPlus) {
	$wildcardBindingKey = "ipport=0.0.0.0`:$Port"
	Write-Host "Removing SSL binding for wildcard prefix (+) via $wildcardBindingKey ..."
	& netsh http delete sslcert $wildcardBindingKey | Out-Null

	if (-not $SkipUrlAcl) {
		foreach ($scheme in @("https", "http")) {
			$wildcardUrl = "${scheme}://+:$Port/"
			Write-Host "Removing URL ACL for $wildcardUrl ..."
			& netsh http delete urlacl "url=$wildcardUrl" 2>&1 | Out-Null
		}
	}
}

foreach ($hostName in $Hosts) {
	$bindingKey = Get-BindingKey -HostName $hostName -Port $Port
	Write-Host "Removing SSL binding for $bindingKey ..."
	& netsh http delete sslcert $bindingKey | Out-Null

	if (-not $SkipUrlAcl) {
		foreach ($scheme in @("https", "http")) {
			$url = "${scheme}://${hostName}:$Port/"
			Write-Host "Removing URL ACL for $url ..."
			& netsh http delete urlacl "url=$url" 2>&1 | Out-Null
		}
	}
}

$thumbprintsToRemove = @()

if (-not [string]::IsNullOrWhiteSpace($Thumbprint)) {
	$thumbprintsToRemove += ($Thumbprint -replace "\s", "").ToUpperInvariant()
}
else {
	$certsByName = Get-ChildItem -Path "cert:\LocalMachine\My" | Where-Object { $_.FriendlyName -eq $FriendlyName }
	if ($certsByName.Count -eq 0) {
		Write-Host "No certificate found in LocalMachine\\My with FriendlyName '$FriendlyName'."
	}
	else {
		$thumbprintsToRemove += $certsByName | Select-Object -ExpandProperty Thumbprint -Unique
	}
}

foreach ($tp in ($thumbprintsToRemove | Select-Object -Unique)) {
	Remove-CertByThumbprint -StorePath "cert:\LocalMachine\My" -CertThumbprint $tp
	Remove-CertByThumbprint -StorePath "cert:\LocalMachine\Root" -CertThumbprint $tp
}

Write-Host ""
Write-Host "Done."
Write-Host "Hosts: $($Hosts -join ', ')"
Write-Host "Include wildcard (+): $IncludeWildcardPlus"
Write-Host "Port: $Port"
if ($thumbprintsToRemove.Count -gt 0) {
	Write-Host "Removed thumbprints: $($thumbprintsToRemove -join ', ')"
}
else {
	Write-Host "No matching certificate thumbprints were removed."
}
