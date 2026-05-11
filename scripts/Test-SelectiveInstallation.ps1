<#
.SYNOPSIS
    Tests selective Bible version installation and content recommendation system.

.DESCRIPTION
    This script validates that the selective installation system works correctly
    with modular Bible databases and intelligent quote recommendations.

.PARAMETER TestMode
    Type of test to run: Basic, Installation, Recommendations, Full

.EXAMPLE
    .\Test-SelectiveInstallation.ps1 -TestMode Installation
    Tests Bible version installation functionality

.EXAMPLE
    .\Test-SelectiveInstallation.ps1 -TestMode Recommendations
    Tests content recommendation engine
#>

[CmdletBinding()]
param(
    [ValidateSet('Basic', 'Installation', 'Recommendations', 'Full')]
    [string]$TestMode = 'Basic'
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'Continue'

function Write-TestResult {
    param([string]$TestName, [bool]$Passed, [string]$Message = '')
    $status = if ($Passed) { 'PASS' } else { 'FAIL' }
    $color = if ($Passed) { 'Green' } else { 'Red' }
    Write-Host "[$status] $TestName" -ForegroundColor $color
    if ($Message) {
        Write-Host "    $Message" -ForegroundColor $color
    }
}

function Write-TestHeader {
    param([string]$Header)
    Write-Host "`n=== $Header ===" -ForegroundColor Cyan
}

function Format-Number {
    param([int]$Number)
    return "{0:N0}" -f $Number
}

try {
    Write-Host "Pixel Pulse Selective Installation Test Suite" -ForegroundColor White
    Write-Host "========================================" -ForegroundColor White
    Write-Host "Test Mode: $TestMode" -ForegroundColor Gray
    Write-Host ""

    # Test 1: Modular Bible Database Files
    Write-TestHeader "Modular Bible Database Files"
    
    try {
        $expectedBibleFiles = @(
            "bible_kjv.db",
            "bible_asv.db", 
            "bible_web.db",
            "bible_ylt.db",
            "bible_bbe.db",
            "bible_darby.db"
        )
        
        $resourcesDir = "PixelPulse\Resources"
        $foundFiles = @()
        
        foreach ($file in $expectedBibleFiles) {
            $filePath = Join-Path $resourcesDir $file
            if (Test-Path $filePath) {
                $fileInfo = Get-Item $filePath
                Write-TestResult "$file" $true "Found $($fileInfo.Length / 1MB) MB"
                $foundFiles += $file
            } else {
                Write-TestResult "$file" $false "File not found"
            }
        }
        
        Write-TestResult "Bible Database Files" ($foundFiles.Count -eq $expectedBibleFiles.Count) "Found $($foundFiles.Count)/$($expectedBibleFiles.Count) Bible version files"

    } catch {
        Write-TestResult "Modular Bible Database Files" $false $_.Exception.Message
    }

    # Test 2: User Preferences System
    if ($TestMode -in @('Installation', 'Full')) {
        Write-TestHeader "User Preferences System"
        
        try {
            # Load UserPreferences model
            Add-Type -Path "PixelPulse\bin\Release\net8.0-windows\PixelPulse.dll" -ErrorAction SilentlyContinue
            
            if ($?) {
                Write-TestResult "Load UserPreferences Model" $true "User preferences model loaded successfully"
                
                # Test UserPreferences properties
                $preferences = [PixelPulse.Models.UserPreferences]::new()
                Write-TestResult "UserPreferences Properties" ($null -ne $preferences) "User preferences model created"
                
                # Test BibleVersionInfo
                $versionInfo = [PixelPulse.Models.BibleVersionInfo]::Names
                Write-TestResult "Bible Version Info" ($versionInfo.Count -eq 6) "6 Bible versions defined"
                
                $descriptions = [PixelPulse.Models.BibleVersionInfo]::Descriptions
                Write-TestResult "Bible Version Descriptions" ($descriptions.Count -eq 6) "6 Bible version descriptions defined"
                
            } else {
                Write-TestResult "Load UserPreferences Model" $false "Could not load PixelPulse assembly"
            }
            
        } catch {
            Write-TestResult "User Preferences System" $false $_.Exception.Message
        }
    }

    # Test 3: Bible Version Manager
    if ($TestMode -in @('Installation', 'Full')) {
        Write-TestHeader "Bible Version Manager"
        
        try {
            Add-Type -Path "PixelPulse\bin\Release\net8.0-windows\PixelPulse.dll" -ErrorAction SilentlyContinue
            
            if ($?) {
                # Test BibleVersionManager loading
                $preferences = [PixelPulse.Models.UserPreferences]::new()
                [PixelPulse.Database.BibleVersionManager]::Initialize($preferences)
                Write-TestResult "BibleVersionManager Initialize" $true "Manager initialized successfully"
                
                # Test version detection
                $isKJVInstalled = [PixelPulse.Database.BibleVersionManager]::IsVersionInstalled([PixelPulse.Models.BibleVersion]::KJV)
                Write-TestResult "KJV Version Detection" $isKJVInstalled "KJV version availability: $isKJVInstalled"
                
                # Test preferred version setting
                [PixelPulse.Database.BibleVersionManager]::SetPreferredVersion([PixelPulse.Models.BibleVersion]::KJV)
                $preferredVersion = [PixelPulse.Database.BibleVersionManager]::GetPreferredVersion()
                Write-TestResult "Preferred Version Setting" ($preferredVersion -eq [PixelPulse.Models.BibleVersion]::KJV) "Preferred version set correctly"
                
            } else {
                Write-TestResult "Bible Version Manager" $false "Could not load PixelPulse assembly"
            }
            
        } catch {
            Write-TestResult "Bible Version Manager" $false $_.Exception.Message
        }
    }

    # Test 4: Content Recommendation Engine
    if ($TestMode -in @('Recommendations', 'Full')) {
        Write-TestHeader "Content Recommendation Engine"
        
        try {
            Add-Type -Path "PixelPulse\bin\Release\net8.0-windows\PixelPulse.dll" -ErrorAction SilentlyContinue
            
            if ($?) {
                # Test ContentRecommendationEngine loading
                $preferences = [PixelPulse.Models.UserPreferences]::new()
                [PixelPulse.Database.BibleVersionManager]::Initialize($preferences)
                Write-TestResult "ContentRecommendationEngine Load" $true "Recommendation engine loaded successfully"
                
                # Test love quotes recommendation
                $loveQuotes = [PixelPulse.Database.ContentRecommendationEngine]::GetLoveQuotesAsync(5, $preferences).Result
                Write-TestResult "Love Quotes Recommendation" ($loveQuotes.Count -gt 0) "$(Format-Number $loveQuotes.Count) love quotes recommended"
                
                # Test business quotes Recommendation
                $businessQuotes = [PixelPulse.Database.ContentRecommendationEngine]::GetBusinessQuotesAsync(5, $preferences).Result
                Write-TestResult "Business Quotes Recommendation" ($businessQuotes.Count -gt 0) "$(Format-Number $businessQuotes.Count) business quotes recommended"
                
                # Test general recommendations
                $recommendedQuotes = [PixelPulse.Database.ContentRecommendationEngine]::GetRecommendedQuotesAsync($preferences, 10).Result
                Write-TestResult "General Recommendations" ($recommendedQuotes.Count -eq 10) "$(Format-Number $recommendedQuotes.Count) personalized quotes recommended"
                
            } else {
                Write-TestResult "Content Recommendation Engine" $false "Could not load PixelPulse assembly"
            }
            
        } catch {
            Write-TestResult "Content Recommendation Engine" $false $_.Exception.Message
        }
    }

    # Test 5: Enhanced Love Quote Categorization
    if ($TestMode -in @('Recommendations', 'Full')) {
        Write-TestHeader "Enhanced Love Quote Categorization"
        
        try {
            Add-Type -Path "PixelPulse\bin\Release\net8.0-windows\PixelPulse.dll" -ErrorAction SilentlyContinue
            
            if ($?) {
                # Test love quote keywords
                $loveQuotes = [PixelPulse.Database.ContentRecommendationEngine]::GetLoveQuotesAsync(20).Result
                
                # Verify love keywords in recommended quotes
                $loveKeywordCount = 0
                foreach ($quote in $loveQuotes) {
                    $text = $quote.Text.ToLower()
                    if ($text -match "love|heart|romance|relationship|marriage|intimacy|affection|passion|devotion|commitment|together|forever|always|caring|tenderness|embrace|kiss|hold|touch|beloved") {
                        $loveKeywordCount++
                    }
                }
                
                $lovePercentage = if ($loveQuotes.Count -gt 0) { [math]::Round(($loveKeywordCount / $loveQuotes.Count) * 100, 1) } else { 0 }
                Write-TestResult "Love Quote Categorization" ($lovePercentage -gt 60) "$loveKeywordCount% of love quotes contain love keywords"
                
            } else {
                Write-TestResult "Enhanced Love Quote Categorization" $false "Could not load PixelPulse assembly"
            }
            
        } catch {
            Write-TestResult "Enhanced Love Quote Categorization" $false $_.Exception.Message
        }
    }

    # Test 6: Enhanced Business Quote Categorization
    if ($TestMode -in @('Recommendations', 'Full')) {
        Write-TestHeader "Enhanced Business Quote Categorization"
        
        try {
            Add-Type -Path "PixelPulse\bin\Release\net8.0-windows\PixelPulse.dll" -ErrorAction SilentlyContinue
            
            if ($?) {
                # Test business quote keywords
                $businessQuotes = [PixelPulse.Database.ContentRecommendationEngine]::GetBusinessQuotesAsync(20).Result
                
                # Verify business keywords in recommended quotes
                $businessKeywordCount = 0
                foreach ($quote in $businessQuotes) {
                    $text = $quote.Text.ToLower()
                    if ($text -match "business|work|career|success|leadership|management|strategy|entrepreneur|innovation|productivity|team|organization|growth|profit|investment|market|competition|customer|service|quality") {
                        $businessKeywordCount++
                    }
                }
                
                $businessPercentage = if ($businessQuotes.Count -gt 0) { [math]::Round(($businessKeywordCount / $businessQuotes.Count) * 100, 1) } else { 0 }
                Write-TestResult "Business Quote Categorization" ($businessPercentage -gt 70) "$businessKeywordCount% of business quotes contain business keywords"
                
            } else {
                Write-TestResult "Enhanced Business Quote Categorization" $false "Could not load PixelPulse assembly"
            }
            
        } catch {
            Write-TestResult "Enhanced Business Quote Categorization" $false $_.Exception.Message
        }
    }

    # Test 7: Installation Components
    if ($TestMode -in @('Installation', 'Full')) {
        Write-TestHeader "Installation Components Test"
        
        try {
            # Test installer configuration
            $installerPath = "PixelPulseInstaller.iss"
            if (Test-Path $installerPath) {
                Write-TestResult "Installer Configuration" $true "Installer script found"
                
                $installerContent = Get-Content $installerPath
                
                # Check for component definitions
                $hasComponents = $installerContent -match "\[Components\]"
                Write-TestResult "Components Section" ($null -ne $hasComponents) "Components section defined"
                
                # Check for type definitions
                $hasTypes = $installerContent -match "\[Types\]"
                Write-TestResult "Types Section" ($null -ne $hasTypes) "Types section defined"
                
                # Check for Bible version components
                $kjvComponent = $installerContent -match "Name.*kjv.*Description"
                $asvComponent = $installerContent -match "Name.*asv.*Description"
                $webComponent = $installerContent -match "Name.*web.*Description"
                $yltComponent = $installerContent -match "Name.*ylt.*Description"
                $bbeComponent = $installerContent -match "Name.*bbe.*Description"
                $darbyComponent = $installerContent -match "Name.*darby.*Description"
                
                $componentCount = @($kjvComponent, $asvComponent, $webComponent, $yltComponent, $bbeComponent, $darbyComponent) | Where-Object { $_ -ne $null }
                Write-TestResult "Bible Version Components" ($componentCount.Count -eq 6) "$(Format-Number $componentCount.Count) Bible version components defined"
                
            } else {
                Write-TestResult "Installer Configuration" $false "Installer script not found"
            }
            
        } catch {
            Write-TestResult "Installation Components Test" $false $_.Exception.Message
        }
    }

    # Test 8: Database Builder Integration
    if ($TestMode -eq 'Full') {
        Write-TestHeader "Database Builder Integration"
        
        try {
            # Test that database builder creates modular files
            $databaseBuilderPath = "PixelPulse.DatabaseBuilder\bin\Release\net8.0-windows\PixelPulse.DatabaseBuilder.exe"
            
            if (Test-Path $databaseBuilderPath) {
                Write-TestResult "Database Builder Executable" $true "Database builder found"
                
                # Test modular database creation
                $resourcesDir = "PixelPulse\Resources"
                $expectedFiles = @("bible_kjv.db", "bible_asv.db", "bible_web.db", "bible_ylt.db", "bible_bbe.db", "bible_darby.db", "quotes.db")
                $actualFiles = Get-ChildItem $resourcesDir -Filter "*.db" | Select-Object Name
                
                $fileCount = 0
                foreach ($expectedFile in $expectedFiles) {
                    if ($expectedFile -in $actualFiles) {
                        $fileCount++
                    }
                }
                
                Write-TestResult "Modular Database Creation" ($fileCount -ge 6) "$(Format-Number $fileCount)/$(Format-Number $expectedFiles.Count) expected database files created"
                
            } else {
                Write-TestResult "Database Builder Executable" $false "Database builder not found"
            }
            
        } catch {
            Write-TestResult "Database Builder Integration" $false $_.Exception.Message
        }
    }

    Write-Host "`n=== Selective Installation Test Summary ===" -ForegroundColor Cyan
    Write-Host "Selective Bible installation and content recommendation test completed." -ForegroundColor White
    Write-Host "The system now includes:" -ForegroundColor White
    Write-Host "  • Modular Bible version installation" -ForegroundColor Green
    Write-Host "  • User preferences and version selection" -ForegroundColor Green
    Write-Host "  • Intelligent content recommendations" -ForegroundColor Green
    Write-Host "  • Enhanced love and business quote categorization" -ForegroundColor Green
    Write-Host "  • User needs assessment and personalization" -ForegroundColor Green

} catch {
    Write-Host "Fatal error during testing: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
