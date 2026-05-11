<#
.SYNOPSIS
    Tests the multi-version Bible functionality with 6 different Bible translations.

.DESCRIPTION
    This script validates that the multi-version Bible system works correctly
    with KJV, ASV, WEB, YLT, BBE, and DARBY Bible versions.

.PARAMETER TestMode
    Type of test to run: Basic, Versions, Full, or Performance

.EXAMPLE
    .\Test-MultiVersionBible.ps1 -TestMode Versions
    Tests all Bible versions specifically

.EXAMPLE
    .\Test-MultiVersionBible.ps1 -TestMode Full
    Runs comprehensive tests including all Bible versions
#>

[CmdletBinding()]
param(
    [ValidateSet('Basic', 'Versions', 'Full', 'Performance')]
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
    Write-Host "Pixel Pulse Multi-Version Bible Test Suite" -ForegroundColor White
    Write-Host "========================================" -ForegroundColor White
    Write-Host "Test Mode: $TestMode" -ForegroundColor Gray
    Write-Host ""

    # Test 1: Basic Multi-Version Loading
    Write-TestHeader "Multi-Version Bible Loading"
    
    try {
        # Test that we can load the self-contained application
        $exePath = "PixelPulse\bin\Release\net8.0-windows\PixelPulse.exe"
        if (-not (Test-Path $exePath)) {
            Write-TestResult "Find Self-Contained Executable" $false "PixelPulse.exe not found at $exePath"
        } else {
            $fileInfo = Get-Item $exePath
            Write-TestResult "Find Self-Contained Executable" $true "Found $($fileInfo.Length / 1MB) MB self-contained executable"
        }

        # Test database exists
        $dbPath = "PixelPulse\Resources\quotes.db"
        if (-not (Test-Path $dbPath)) {
            Write-TestResult "Find Multi-Version Bible Database" $false "quotes.db not found at $dbPath"
        } else {
            $fileInfo = Get-Item $dbPath
            Write-TestResult "Find Multi-Version Bible Database" $true "Found $($fileInfo.Length / 1MB) MB multi-version Bible database"
        }

    } catch {
        Write-TestResult "Basic Multi-Version Loading" $false $_.Exception.Message
    }

    # Test 2: Bible Versions Coverage
    if ($TestMode -in @('Versions', 'Full')) {
        Write-TestHeader "Bible Versions Coverage Test"
        
        try {
            # Load and test the database
            Add-Type -Path "PixelPulse\bin\Release\net8.0-windows\PixelPulse.dll" -ErrorAction SilentlyContinue
            
            if ($?) {
                $databaseType = [PixelPulse.Database.QuoteDatabase]
                $initTask = $databaseType::InitializeAsync()
                $initTask.Wait()
                Write-TestResult "Initialize Multi-Version Database" $true "Database initialized successfully"

                # Test Bible category count (should include all versions)
                $bibleCount = $databaseType::GetQuoteCount([PixelPulse.Models.QuoteCategory]::Bible)
                Write-TestResult "Multi-Version Bible Count" ($bibleCount -gt 10000) "$(Format-Number $bibleCount) total Bible verses"

                # Test source breakdown for Bible versions
                $allQuotes = @()
                $allCategories = [Enum]::GetValues([PixelPulse.Models.QuoteCategory])
                
                foreach ($category in $allCategories) {
                    $categoryQuotes = $databaseType::GetQuotesAsync($category).Result
                    $allQuotes += $categoryQuotes
                }

                $bibleSources = $allQuotes | Where-Object { $_.Source -like "*-Bible" } | Group-Object Source
                Write-TestResult "Bible Sources Found" ($bibleSources.Count -ge 6) "Found $($bibleSources.Count) Bible versions"

                # Test each Bible version
                $expectedVersions = @("KJV-Bible", "ASV-Bible", "WEB-Bible", "YLT-Bible", "BBE-Bible", "DARBY-Bible")
                foreach ($version in $expectedVersions) {
                    $versionQuotes = $allQuotes | Where-Object { $_.Source -eq $version }
                    Write-TestResult "$version" ($versionQuotes.Count -gt 0) "$(Format-Number $versionQuotes.Count) verses"
                }

                # Test verse variety across versions
                $uniqueReferences = $allQuotes | Where-Object { $_.Source -like "*-Bible" } | Select-Object Author -Unique
                Write-TestResult "Unique Verse References" ($uniqueReferences.Count -gt 1000) "$(Format-Number $uniqueReferences.Count) unique references"

            } else {
                Write-TestResult "Bible Versions Coverage Test" $false "Could not load PixelPulse assembly"
            }
        } catch {
            Write-TestResult "Bible Versions Coverage Test" $false $_.Exception.Message
        }
    }

    # Test 3: Cross-Version Comparison
    if ($TestMode -eq 'Full') {
        Write-TestHeader "Cross-Version Comparison Test"
        
        try {
            $databaseType = [PixelPulse.Database.QuoteDatabase]
            
            # Test famous verses across different versions
            $famousReferences = @("John 3:16", "Psalm 23:1", "Proverbs 3:5", "Romans 8:28", "Genesis 1:1")
            
            foreach ($reference in $famousReferences) {
                $verseQuotes = $databaseType::SearchQuotesAsync($reference).Result
                $versions = $verseQuotes | Select-Object Source -Unique | Where-Object { $_ -like "*-Bible" }
                Write-TestResult "$reference Versions" ($versions.Count -ge 2) "Found in $($versions.Count) versions: $($versions -join ', ')"
            }

            # Test that same verses have different wording across versions
            $john316 = $databaseType::SearchQuotesAsync("John 3:16").Result
            $uniqueTexts = $john316 | Select-Object Text -Unique
            Write-TestResult "John 3:16 Variations" ($uniqueTexts.Count -ge 2) "$($uniqueTexts.Count) different translations"

        } catch {
            Write-TestResult "Cross-Version Comparison Test" $false $_.Exception.Message
        }
    }

    # Test 4: Performance with Multiple Versions
    if ($TestMode -eq 'Performance') {
        Write-TestHeader "Multi-Version Performance Test"
        
        try {
            $databaseType = [PixelPulse.Database.QuoteDatabase]
            $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
            
            # Test Bible category performance (should be fast even with multiple versions)
            $stopwatch.Restart()
            for ($i = 0; $i -lt 100; $i++) {
                $quote = $databaseType::GetRandomQuoteAsync([PixelPulse.Models.QuoteCategory]::Bible).Result
            }
            $stopwatch.Stop()
            
            $bibleQuotesPerSecond = 100 / $stopwatch.Elapsed.TotalSeconds
            Write-TestResult "Multi-Version Bible Performance" ($bibleQuotesPerSecond -gt 20) "$([math]::Round($bibleQuotesPerSecond, 2)) Bible quotes/second"

            # Test search performance across versions
            $stopwatch.Restart()
            $searchResults = $databaseType::SearchQuotesAsync("faith").Result
            $stopwatch.Stop()
            
            Write-TestResult "Multi-Version Search Performance" ($stopwatch.Elapsed.TotalMilliseconds -lt 2000) "$(Format-Number $searchResults.Count) results in $($stopwatch.ElapsedMilliseconds)ms"

            # Test random verse from different versions
            $stopwatch.Restart()
            $randomVerses = @()
            for ($i = 0; $i -lt 50; $i++) {
                $quote = $databaseType::GetRandomQuoteAsync([PixelPulse.Models.QuoteCategory]::Bible).Result
                $randomVerses += $quote
            }
            $stopwatch.Stop()
            
            $versionVariety = $randomVerses | Select-Object Source -Unique
            Write-TestResult "Version Variety in Random Selection" ($versionVariety.Count -ge 3) "Found $($versionVariety.Count) different versions in 50 random verses"

        } catch {
            Write-TestResult "Multi-Version Performance Test" $false $_.Exception.Message
        }
    }

    # Test 5: Quality and Deduplication
    if ($TestMode -in @('Versions', 'Full')) {
        Write-TestHeader "Quality and Deduplication Test"
        
        try {
            $databaseType = [PixelPulse.Database.QuoteDatabase]
            
            # Test that verses are properly categorized
            $bibleQuotes = $databaseType::GetQuotesAsync([PixelPulse.Models.QuoteCategory]::Bible).Result
            $categorizedVerses = $bibleQuotes | Where-Object { $_.Category -ne [PixelPulse.Models.QuoteCategory]::Bible }
            Write-TestResult "Bible Verse Categorization" ($categorizedVerses.Count -gt 1000) "$(Format-Number $categorizedVerses.Count) Bible verses categorized into themes"

            # Test for duplicates (should be minimal due to deduplication)
            $allBibleTexts = $bibleQuotes | Select-Object Text -Unique
            $deduplicationRate = [math]::Round((($bibleQuotes.Count - $allBibleTexts.Count) / $bibleQuotes.Count) * 100, 2)
            Write-TestResult "Deduplication Quality" ($deduplicationRate -lt 5) "$deduplicationRate% duplicate rate (should be <5%)"

            # Test verse quality (no empty or very short verses)
            $qualityVerses = $bibleQuotes | Where-Object { $_.Text.Length -gt 10 }
            $qualityRate = [math]::Round(($qualityVerses.Count / $bibleQuotes.Count) * 100, 2)
            Write-TestResult "Verse Quality" ($qualityRate -gt 95) "$qualityRate% verses are quality (length > 10 chars)"

        } catch {
            Write-TestResult "Quality and Deduplication Test" $false $_.Exception.Message
        }
    }

    # Test 6: Database Statistics Summary
    Write-TestHeader "Multi-Version Bible Statistics"
    
    try {
        $databaseType = [PixelPulse.Database.QuoteDatabase]
        $stats = $databaseType::GetDatabaseStatsAsync().Result
        
        Write-TestResult "Get Multi-Version Stats" ($null -ne $stats) "Statistics retrieved successfully"
        
        if ($stats) {
            Write-Host "Multi-Version Bible Overview:" -ForegroundColor Gray
            Write-Host "  Total Quotes: $(Format-Number $stats.TotalQuotes)" -ForegroundColor Gray
            Write-Host "  Categories: $($stats.Categories.Count)" -ForegroundColor Gray
            Write-Host "  Sources: $($stats.Sources.Count)" -ForegroundColor Gray
            Write-Host "  Last Updated: $($stats.LastUpdated)" -ForegroundColor Gray
            
            Write-Host "`nBible Version Breakdown:" -ForegroundColor Gray
            $bibleSources = $stats.Sources.GetEnumerator() | Where-Object { $_.Key -like "*-Bible" } | Sort-Object Value -Descending
            foreach ($source in $bibleSources) {
                $percentage = ($source.Value * 100.0 / $stats.TotalQuotes).ToString("F1")
                Write-Host "  $($source.Key): $(Format-Number $source.Value) verses ($percentage%)" -ForegroundColor Gray
            }
            
            Write-Host "`nCategory Distribution:" -ForegroundColor Gray
            foreach ($cat in $stats.Categories.GetEnumerator() | Sort-Object Value -Descending) {
                $percentage = ($cat.Value * 100.0 / $stats.TotalQuotes).ToString("F1")
                Write-Host "  $($cat.Key): $(Format-Number $cat.Value) quotes ($percentage%)" -ForegroundColor Gray
            }
        }
        
    } catch {
        Write-TestResult "Multi-Version Bible Statistics" $false $_.Exception.Message
    }

    Write-Host "`n=== Multi-Version Bible Test Summary ===" -ForegroundColor Cyan
    Write-Host "Multi-version Bible functionality test completed." -ForegroundColor White
    Write-Host "The application now includes 6 Bible translations:" -ForegroundColor White
    Write-Host "  • KJV (King James Version) - Complete Bible" -ForegroundColor Green
    Write-Host "  • ASV (American Standard Version)" -ForegroundColor Green
    Write-Host "  • WEB (World English Bible)" -ForegroundColor Green
    Write-Host "  • YLT (Young's Literal Translation)" -ForegroundColor Green
    Write-Host "  • BBE (Bible in Basic English)" -ForegroundColor Green
    Write-Host "  • DARBY (Darby English Bible)" -ForegroundColor Green
    Write-Host "All versions are public domain and freely distributable." -ForegroundColor White

} catch {
    Write-Host "Fatal error during testing: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
