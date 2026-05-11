#Requires -Version 5.1
<#
.SYNOPSIS
    Builds the PixelPulse.DatabaseBuilder project and runs it to populate the quotes database.

.DESCRIPTION
    Ensures DatabaseBuilder is built, then runs DatabaseBuilder.exe. The database is created
    in %AppData%\PixelPulse\quotes.db. Progress is shown from the console output.

.PARAMETER SkipBuild
    Do not build DatabaseBuilder; only run the existing executable.

.PARAMETER Configuration
    Build configuration: Debug or Release. Default is Release.

.EXAMPLE
    .\Build-Database.ps1
.EXAMPLE
    .\Build-Database.ps1 -SkipBuild
#>
[CmdletBinding()]
param(
    [switch]$SkipBuild,
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$script:RootDir = $PSScriptRoot -replace '\\scripts$', ''
if (-not $script:RootDir) { $script:RootDir = (Get-Location).Path }

# Refresh PATH to include .NET SDK
$env:PATH = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

function Write-Status {
    param([string]$Message, [string]$Status = 'Info')
    $color = switch ($Status) { 'Success' { 'Green' } 'Error' { 'Red' } default { 'Cyan' } }
    Write-Host $Message -ForegroundColor $color
}

$dbProj = Join-Path $script:RootDir 'PixelPulse.DatabaseBuilder\PixelPulse.DatabaseBuilder.csproj'
$dbExe = Join-Path $script:RootDir "PixelPulse.DatabaseBuilder\bin\$Configuration\net8.0-windows\PixelPulse.DatabaseBuilder.exe"
$dbPath = Join-Path $env:APPDATA 'PixelPulse\quotes.db'

try {
    Set-Location $script:RootDir
    if (-not (Test-Path $dbProj)) { throw "Project not found: $dbProj" }

    if (-not $SkipBuild) {
        # Try to find dotnet
        $dotnetCmd = Get-Command dotnet -ErrorAction SilentlyContinue
        if (-not $dotnetCmd) {
            # Try common installation paths
            $dotnetPaths = @(
                "C:\Program Files\dotnet\dotnet.exe",
                "${env:ProgramFiles(x86)}\dotnet\dotnet.exe"
            )
            $dotnetFound = $false
            foreach ($path in $dotnetPaths) {
                if (Test-Path $path) {
                    $env:PATH = "$(Split-Path $path);$env:PATH"
                    $dotnetFound = $true
                    break
                }
            }
            if (-not $dotnetFound) {
                Write-Status 'dotnet CLI not found. Install .NET 8.0 SDK.' 'Error'
                exit 1
            }
        }
        Write-Status "Building PixelPulse.DatabaseBuilder ($Configuration)..." 'Info'
        dotnet restore $dbProj 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) { throw 'Restore failed.' }
        dotnet build $dbProj --configuration $Configuration --no-restore 2>&1 | Out-Host
        if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
        Write-Status 'DatabaseBuilder build succeeded.' 'Success'
    }

    if (-not (Test-Path $dbExe)) {
        Write-Status "Executable not found: $dbExe. Run without -SkipBuild first." 'Error'
        exit 1
    }

    Write-Status 'Running DatabaseBuilder...' 'Info'
    Write-Status "Database will be created at: $dbPath" 'Info'
    & $dbExe
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0) {
        Write-Status "DatabaseBuilder exited with code $exitCode" 'Error'
        exit $exitCode
    }
    if (Test-Path $dbPath) {
        $size = (Get-Item $dbPath).Length
        Write-Status "Database created: $dbPath ($size bytes)" 'Success'
    } else {
        Write-Status 'Database file not found after run. Check logs.' 'Error'
        exit 1
    }
    exit 0
} catch {
    Write-Status "Error: $_" 'Error'
    exit 1
} finally {
    Set-Location $script:RootDir
}
