$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$ts = Join-Path $root 'thunderstore'
$dll = Join-Path $root 'bin\Release\FusOpt.dll'
$outZip = Join-Path $root 'FusOpt-1.0.1-thunderstore.zip'

if (-not (Test-Path $dll)) {
    Push-Location $root
    dotnet build -c Release
    Pop-Location
}

$stage = Join-Path $env:TEMP ("FusOpt_ts_" + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path (Join-Path $stage 'Mods') | Out-Null
Copy-Item (Join-Path $ts 'manifest.json') $stage
Copy-Item (Join-Path $ts 'README.md') $stage
Copy-Item (Join-Path $ts 'icon.png') $stage
Copy-Item $dll (Join-Path $stage 'Mods\FusOpt.dll')

if (Test-Path $outZip) { Remove-Item $outZip -Force }
Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $outZip -Force
Remove-Item $stage -Recurse -Force
Write-Host "Packed: $outZip"
