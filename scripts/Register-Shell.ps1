param([switch]$Remove, [string]$AppDirectory = (Split-Path $PSScriptRoot -Parent))
$ErrorActionPreference = 'Stop'
$packageName = 'KorFresh.Fzip.Preview'
$publisher = 'CN=KorFresh Fzip Local Preview'
$logDir = Join-Path $env:LOCALAPPDATA 'Fzip'
New-Item -ItemType Directory $logDir -Force | Out-Null
$logFile = Join-Path $logDir 'shell-registration.log'
try {
    $installed = @(Get-AppxPackage -Name $packageName)
    foreach ($package in $installed) {
        if ($package.Publisher -ne $publisher) { throw 'Unexpected publisher for Fzip shell package.' }
    }
    if ($Remove) {
        foreach ($package in $installed) { Remove-AppxPackage -Package $package.PackageFullName -ErrorAction Stop }
        "$(Get-Date -Format o) Removed Fzip preview shell package." | Out-File $logFile -Append -Encoding utf8
        exit 0
    }
    if ([Environment]::OSVersion.Version.Build -lt 22000) { throw 'The modern context menu requires Windows 11.' }
    $AppDirectory = (Resolve-Path -LiteralPath $AppDirectory).Path
    $msix = Join-Path $AppDirectory 'shell/Fzip.Shell.msix'
    if (!(Test-Path -LiteralPath $msix) -or !(Test-Path -LiteralPath (Join-Path $AppDirectory 'Fzip.Shell.dll'))) { throw 'Fzip shell package files are missing.' }
    # Trust setup is a separate, explicit local-test step. Never import a certificate here.
    $certificate = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2 (Join-Path $AppDirectory 'shell/Fzip.LocalPreview.cer')
    if (!(Test-Path "Cert:\LocalMachine\TrustedPeople\$($certificate.Thumbprint)")) {
        throw "Local preview certificate is not trusted. See README: explicit administrator approval is required to trust certificate $($certificate.Thumbprint). No trust settings were changed."
    }
    foreach ($package in $installed) { Remove-AppxPackage -Package $package.PackageFullName -ErrorAction Stop }
    Add-AppxPackage -Path $msix -ExternalLocation $AppDirectory -ErrorAction Stop
    $registered = Get-AppxPackage -Name $packageName
    if (!$registered) { throw 'Package registration could not be verified.' }
    "$(Get-Date -Format o) Registered $($registered.PackageFullName) -> $AppDirectory" | Out-File $logFile -Append -Encoding utf8
    Write-Output "Registered $($registered.PackageFullName)"
    exit 0
} catch {
    "$(Get-Date -Format o) ERROR: $($_ | Out-String)" | Out-File $logFile -Append -Encoding utf8
    Write-Error $_
    exit 1
}
