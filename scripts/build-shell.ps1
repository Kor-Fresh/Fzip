param([string]$Output = 'artifacts/app', [string]$SigningPfx = '.tools/signing/Fzip.LocalPreview.pfx', [switch]$Test)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
Set-Location $root
$compiler = Get-ChildItem .tools/llvm -Filter x86_64-w64-mingw32-clang++.exe -Recurse | Select-Object -First 1 -ExpandProperty FullName
$makeappx = Get-ChildItem .tools/windows-sdk -Filter makeappx.exe -Recurse | Where-Object FullName -Match '\\x64\\' | Select-Object -First 1 -ExpandProperty FullName
if (!$compiler -or !$makeappx) { throw 'LLVM-MinGW and Windows SDK packaging tools are required. See README.md.' }
New-Item -ItemType Directory $Output -Force | Out-Null
& $compiler -std=c++17 -O2 -Wall -Wextra -Werror -shared -static -static-libgcc -static-libstdc++ src/Fzip.Shell/ShellCommand.cpp src/Fzip.Shell/exports.def -o "$Output/Fzip.Shell.dll" -lole32 -lshell32 -lshlwapi -luuid
if ($LASTEXITCODE -ne 0) { throw 'Shell DLL compilation failed' }
$packageRoot = Join-Path $root 'artifacts/shell-package'
New-Item -ItemType Directory "$packageRoot/Assets","$Output/shell" -Force | Out-Null
Copy-Item packaging/shell/AppxManifest.xml "$packageRoot/AppxManifest.xml" -Force
Add-Type -AssemblyName System.Drawing
$source = [System.Drawing.Image]::FromFile((Join-Path $root 'design/branding/fzip-archive-v1.png'))
$logo = New-Object System.Drawing.Bitmap 150,150
$graphics = [System.Drawing.Graphics]::FromImage($logo)
try { $graphics.DrawImage($source,0,0,150,150); $logo.Save("$packageRoot/Assets/Logo.png",[System.Drawing.Imaging.ImageFormat]::Png) }
finally { $graphics.Dispose(); $logo.Dispose(); $source.Dispose() }
# /nv is required for an external-location package: binaries live outside the MSIX.
& $makeappx pack /d $packageRoot /p "$Output/shell/Fzip.Shell.msix" /nv /o
if ($LASTEXITCODE -ne 0) { throw 'Shell package creation failed' }
$signtool = Get-ChildItem .tools/windows-sdk -Filter signtool.exe -Recurse | Where-Object FullName -Match '\\x64\\' | Select-Object -First 1 -ExpandProperty FullName
if (!(Test-Path -LiteralPath $SigningPfx)) { throw 'A signing certificate is required. See README.md; private keys must never be distributed.' }
& $signtool sign /fd SHA256 /f $SigningPfx "$Output/shell/Fzip.Shell.msix"
if ($LASTEXITCODE -ne 0) { throw 'Shell package signing failed' }
Copy-Item .tools/signing/Fzip.LocalPreview.cer "$Output/shell/Fzip.LocalPreview.cer" -Force
Copy-Item scripts/Register-Shell.ps1 "$Output/shell/Register-Shell.ps1" -Force
if ($Test) {
    & $compiler -std=c++17 -O2 -Wall -Wextra -Werror -static -municode tests/ShellTests.cpp -o artifacts/ShellTests.exe -lole32 -lshell32 -luuid
    if ($LASTEXITCODE -ne 0) { throw 'Shell test compilation failed' }
    & artifacts/ShellTests.exe (Join-Path $root "$Output/Fzip.Shell.dll") $root
    if ($LASTEXITCODE -ne 0) { throw 'Shell tests failed' }
}
