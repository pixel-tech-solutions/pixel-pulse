#!/usr/bin/env pwsh
# GitHub Release Asset Updater
# This script deletes old assets and uploads new ones to v1.0.0 release

param(
    [Parameter(Mandatory=$true)]
    [string]$GitHubToken,
    
    [Parameter(Mandatory=$false)]
    [string]$RepoOwner = "pixel-tech-solutions",
    
    [Parameter(Mandatory=$false)]
    [string]$RepoName = "pixel-pulse",
    
    [Parameter(Mandatory=$false)]
    [string]$Tag = "v1.0.0"
)

$ErrorActionPreference = "Stop"

# GitHub API headers
$headers = @{
    "Authorization" = "token $GitHubToken"
    "Accept" = "application/vnd.github.v3+json"
    "Content-Type" = "application/json"
}

Write-Host "[TOOL] GitHub Release Asset Updater" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green
Write-Host "Repository: $RepoOwner/$RepoName" -ForegroundColor Cyan
Write-Host "Release: $Tag" -ForegroundColor Cyan
Write-Host ""

# Get release ID
Write-Host "[INFO] Getting release information..." -ForegroundColor Yellow
$releaseUrl = "https://api.github.com/repos/$RepoOwner/$RepoName/releases/tags/$Tag"
$release = Invoke-RestMethod -Uri $releaseUrl -Method Get -Headers $headers
$releaseId = $release.id

Write-Host "[OK] Release ID: $releaseId" -ForegroundColor Green

# Get current assets
Write-Host "[INFO] Getting current assets..." -ForegroundColor Yellow
$assetsUrl = "https://api.github.com/repos/$RepoOwner/$RepoName/releases/$releaseId/assets"
$assets = Invoke-RestMethod -Uri $assetsUrl -Method Get -Headers $headers

Write-Host "[INFO] Current assets:" -ForegroundColor Cyan
foreach ($asset in $assets) {
    $sizeMB = [math]::Round($asset.size / 1MB, 2)
    Write-Host "  - $($asset.name) ($sizeMB MB)" -ForegroundColor White
}

# Delete old assets
Write-Host ""
Write-Host "[DELETE] Deleting old assets..." -ForegroundColor Red
foreach ($asset in $assets) {
    Write-Host "  Deleting: $($asset.name)" -ForegroundColor Yellow
    $deleteUrl = "https://api.github.com/repos/$RepoOwner/$RepoName/releases/assets/$($asset.id)"
    try {
        Invoke-RestMethod -Uri $deleteUrl -Method Delete -Headers $headers | Out-Null
        Write-Host "    [OK] Deleted" -ForegroundColor Green
    } catch {
        Write-Host "    [ERROR] Failed: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Upload new assets
Write-Host ""
Write-Host "[UPLOAD] Uploading new assets..." -ForegroundColor Green

# Upload installer
$installerPath = "InstallerOutput\PixelPulseInstaller.exe"
if (Test-Path $installerPath) {
    Write-Host "  Uploading: PixelPulseInstaller.exe" -ForegroundColor Yellow
    $uploadUrl = "https://uploads.github.com/repos/$RepoOwner/$RepoName/releases/$releaseId/assets?name=PixelPulseInstaller.exe"
    
    $installerSize = (Get-Item $installerPath).Length
    $installerBytes = [System.IO.File]::ReadAllBytes($installerPath)
    
    $uploadHeaders = $headers.Clone()
    $uploadHeaders["Content-Type"] = "application/octet-stream"
    $uploadHeaders["Content-Length"] = $installerSize
    
    try {
        Invoke-RestMethod -Uri $uploadUrl -Method Post -Headers $uploadHeaders -Body $installerBytes | Out-Null
        $sizeMB = [math]::Round($installerSize / 1MB, 2)
        Write-Host "    [OK] Uploaded ($sizeMB MB)" -ForegroundColor Green
    } catch {
        Write-Host "    [ERROR] Failed: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Upload LICENSE.txt
$licensePath = "LICENSE.txt"
if (Test-Path $licensePath) {
    Write-Host "  Uploading: LICENSE.txt" -ForegroundColor Yellow
    $uploadUrl = "https://uploads.github.com/repos/$RepoOwner/$RepoName/releases/$releaseId/assets?name=LICENSE.txt"
    
    $licenseSize = (Get-Item $licensePath).Length
    $licenseBytes = [System.IO.File]::ReadAllBytes($licensePath)
    
    $uploadHeaders = $headers.Clone()
    $uploadHeaders["Content-Type"] = "text/plain"
    $uploadHeaders["Content-Length"] = $licenseSize
    
    try {
        Invoke-RestMethod -Uri $uploadUrl -Method Post -Headers $uploadHeaders -Body $licenseBytes | Out-Null
        $sizeKB = [math]::Round($licenseSize / 1KB, 2)
        Write-Host "    [OK] Uploaded ($sizeKB KB)" -ForegroundColor Green
    } catch {
        Write-Host "    [ERROR] Failed: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Upload README.md
$readmePath = "README.md"
if (Test-Path $readmePath) {
    Write-Host "  Uploading: README.md" -ForegroundColor Yellow
    $uploadUrl = "https://uploads.github.com/repos/$RepoOwner/$RepoName/releases/$releaseId/assets?name=README.md"
    
    $readmeSize = (Get-Item $readmePath).Length
    $readmeBytes = [System.IO.File]::ReadAllBytes($readmePath)
    
    $uploadHeaders = $headers.Clone()
    $uploadHeaders["Content-Type"] = "text/markdown"
    $uploadHeaders["Content-Length"] = $readmeSize
    
    try {
        Invoke-RestMethod -Uri $uploadUrl -Method Post -Headers $uploadHeaders -Body $readmeBytes | Out-Null
        $sizeKB = [math]::Round($readmeSize / 1KB, 2)
        Write-Host "    [OK] Uploaded ($sizeKB KB)" -ForegroundColor Green
    } catch {
        Write-Host "    [ERROR] Failed: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "[SUCCESS] Release asset update completed!" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green
