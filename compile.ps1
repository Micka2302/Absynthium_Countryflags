#!/usr/bin/env pwsh
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

$project = Join-Path $root 'Absynthium_Countryflags.csproj'
$configuration = 'Release'
$targetFramework = 'net8.0'
$pluginName = 'Absynthium_Countryflags'
$buildOutput = Join-Path $root "bin/$configuration/$targetFramework"
$compiledRoot = Join-Path $root 'compiled'
$cssRoot = Join-Path $compiledRoot 'addons/counterstrikesharp'
$pluginTarget = Join-Path $cssRoot "plugins/$pluginName"
$configTarget = Join-Path $cssRoot "configs/plugins/$pluginName"
$workshopAddonName = 'addons_absynthium'
$zipPath = Join-Path $compiledRoot "$pluginName.zip"

function Invoke-Checked {
    param(
        [Parameter(Mandatory = $true)]
        [scriptblock]$Command,
        [Parameter(Mandatory = $true)]
        [string]$Description
    )

    & $Command
    if ($LASTEXITCODE -ne 0) {
        throw "$Description failed with exit code $LASTEXITCODE"
    }
}

function Sync-ClassicWorkshopAssets {
    param(
        [Parameter(Mandatory = $true)]
        [string]$AddonRoot
    )

    $statusIconsRoot = Join-Path $AddonRoot 'panorama/images/econ/status_icons'
    $classicSource = Join-Path $statusIconsRoot 'classic_flags'

    if (-not (Test-Path $classicSource)) {
        throw "Classic workshop assets not found: $classicSource"
    }

    New-Item -ItemType Directory -Path $statusIconsRoot -Force | Out-Null
    Get-ChildItem -LiteralPath $statusIconsRoot -File -Include '*.png', '*.vtex', '*.vtex_c' | Remove-Item -Force
    Copy-Item -Path (Join-Path $classicSource '*.png') -Destination $statusIconsRoot -Force
    Copy-Item -Path (Join-Path $classicSource '*.vtex') -Destination $statusIconsRoot -Force

    $utf8NoBom = [System.Text.UTF8Encoding]::new($false)
    foreach ($vtex in Get-ChildItem -LiteralPath $statusIconsRoot -Filter '*.vtex' -File) {
        $name = $vtex.BaseName
        $png = if ($name.EndsWith('_png')) { $name.Substring(0, $name.Length - 4) + '.png' } else { $name + '.png' }
        $relative = "panorama/images/econ/status_icons/$png"
        $text = [System.IO.File]::ReadAllText($vtex.FullName)
        $updated = [regex]::Replace($text, '"m_fileName"\s+"string"\s+"[^"]+"', '"m_fileName" "string" "' + $relative + '"')
        [System.IO.File]::WriteAllText($vtex.FullName, $updated, $utf8NoBom)
    }

    Write-Host "[OK] Active workshop assets: classic"
}

$pluginFiles = @(
    "$pluginName.dll",
    "$pluginName.deps.json",
    'MaxMind.Db.dll'
)

$configFiles = @(
    "$pluginName.json"
)

Remove-Item -Recurse -Force $compiledRoot -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $pluginTarget -Force | Out-Null
New-Item -ItemType Directory -Path $configTarget -Force | Out-Null

Invoke-Checked { dotnet restore $project } 'dotnet restore'
Invoke-Checked { dotnet build $project -c $configuration --no-restore --nologo } 'dotnet build'

if (-not (Test-Path $buildOutput)) {
    throw "Build output not found at $buildOutput"
}

foreach ($file in $pluginFiles) {
    $source = Join-Path $buildOutput $file
    if (-not (Test-Path $source)) {
        throw "Required plugin file not found: $source"
    }

    Copy-Item -LiteralPath $source -Destination $pluginTarget -Force
}

foreach ($file in $configFiles) {
    $source = Join-Path $root $file
    if (-not (Test-Path $source)) {
        throw "Required config file not found: $source"
    }

    Copy-Item -LiteralPath $source -Destination $configTarget -Force
}

$workshopSource = Join-Path $root "csgo_addons/$workshopAddonName"
$workshopTarget = Join-Path $compiledRoot "csgo_addons/$workshopAddonName"
if (Test-Path $workshopSource) {
    Sync-ClassicWorkshopAssets -AddonRoot $workshopSource
    New-Item -ItemType Directory -Path (Split-Path -Parent $workshopTarget) -Force | Out-Null
    Copy-Item -LiteralPath $workshopSource -Destination (Split-Path -Parent $workshopTarget) -Recurse -Force
} else {
    Write-Host "[WARN] Workshop addon folder csgo_addons/$workshopAddonName not found."
}

$geoLiteSource = Join-Path $root 'GeoLite2-Country.mmdb'
if (Test-Path $geoLiteSource) {
    Copy-Item -LiteralPath $geoLiteSource -Destination $pluginTarget -Force
} else {
    Write-Host '[WARN] GeoLite2-Country.mmdb not found at repository root. Add it beside the plugin DLL on the server.'
}

$unneededPatterns = @(
    '*.pdb',
    '*.xml',
    '*.config',
    'CounterStrikeSharp.API.dll'
)

foreach ($pattern in $unneededPatterns) {
    Get-ChildItem -Path $pluginTarget -Filter $pattern -File -ErrorAction SilentlyContinue | Remove-Item -Force
}

if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Compress-Archive -Path @(
    (Join-Path $compiledRoot 'addons'),
    (Join-Path $compiledRoot 'csgo_addons')
) -DestinationPath $zipPath

Write-Host '[OK] Build finished.'
Write-Host " - Plugin folder: $pluginTarget"
Write-Host " - Config folder: $configTarget"
Write-Host " - Workshop:      $workshopTarget"
Write-Host " - Zip:           $zipPath"
