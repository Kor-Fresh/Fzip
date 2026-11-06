param([switch]$Test, [switch]$Installer)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
Set-Location $root
$dotnet = Join-Path $root '.tools\dotnet\dotnet.exe'
if (!(Test-Path $dotnet)) { $dotnet = (Get-Command dotnet -ErrorAction Stop).Source }
$env:DOTNET_CLI_HOME = Join-Path $root '.tools\dotnet-home'
$env:NUGET_PACKAGES = Join-Path $root '.tools\nuget'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
if ($Test) {
    & $dotnet run --project tests/Fzip.Tests -c Release -- $root
    if ($LASTEXITCODE -ne 0) { throw 'Tests failed' }
}
& $dotnet publish src/Fzip.App -c Release -r win-x64 --self-contained true -o artifacts/app -p:PublishSingleFile=false
if ($LASTEXITCODE -ne 0) { throw 'App publish failed' }
& $dotnet publish src/Fzip.Worker -c Release -r win-x64 --self-contained true -o artifacts/app/worker -p:PublishSingleFile=false
if ($LASTEXITCODE -ne 0) { throw 'Worker publish failed' }
New-Item -ItemType Directory artifacts/app/worker/engine,artifacts/app/licenses -Force | Out-Null
Copy-Item vendor/7zip/7z.exe,vendor/7zip/7z.dll artifacts/app/worker/engine -Force
Copy-Item vendor/licenses/* artifacts/app/licenses -Force
Copy-Item THIRD-PARTY-NOTICES.txt artifacts/app/licenses -Force
& "$PSScriptRoot/build-shell.ps1" -Test:$Test
if ($LASTEXITCODE -ne 0) { throw 'Shell integration build failed' }
if ($Test) {
    & $dotnet run --project tests/Fzip.UiChecks -c Release -- (Join-Path $root 'artifacts/ui-0.1.1.png')
    if ($LASTEXITCODE -ne 0) { throw 'UI foreground checks failed' }
}
if ($Installer) {
    $iscc = Join-Path $root '.tools\inno\app\ISCC.exe'
    if (!(Test-Path $iscc)) { throw 'Inno Setup compiler not found at .tools/inno/app/ISCC.exe' }
    & $iscc installer/Fzip.iss
    if ($LASTEXITCODE -ne 0) { throw 'Installer build failed' }
}
Write-Host 'Build ready: artifacts/app/Fzip.exe'
