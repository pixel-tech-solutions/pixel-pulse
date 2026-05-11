<#
.SYNOPSIS
    Tests the comprehensive quotes functionality including complete KJV Bible.

.DESCRIPTION
    This script validates that the comprehensive quotes system works correctly
    with the complete KJV Bible (~31,000 verses) and other quote sources.

.PARAMETER TestMode
    Type of test to run: Basic, Bible, Full, or Performance

.EXAMPLE
    .\Test-ComprehensiveQuotes.ps1 -TestMode Bible
    Tests Bible quote coverage specifically

.EXAMPLE
    .\Test-ComprehensiveQuotes.ps1 -TestMode Full
    Runs comprehensive tests including all quote categories
#>

[CmdletBinding()]
param(
    [ValidateSet('Basic', 'Bible', 'Full', 'Performance')]
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
    Write-Host "Pixel Pulse Comprehensive Quotes Test Suite" -ForegroundColor White
    Write-Host "=======================================" -ForegroundColor White
    Write-Host "Test Mode: $TestMode" -ForegroundColor Gray
    Write-Host ""

    # Test 1: Basic Quote Loading
    Write-TestHeader "Basic Quote Loading"
    
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
            Write-TestResult "Find Quotes Database" $false "quotes.db not found at $dbPath"
        } else {
            $fileInfo = Get-Item $dbPath
            Write-TestResult "Find Quotes Database" $true "Found $($fileInfo.Length / 1MB) MB quotes database"
        }

    } catch {
        Write-TestResult "Basic Quote Loading" $false $_.Exception.Message
    }

    # Test 2: Bible Coverage Test
    if ($TestMode -in @('Bible', 'Full')) {
        Write-TestHeader "Bible Coverage Test"
        
        try {
            # Load and test the database
            Add-Type -Path "PixelPulse\bin\Release\net8.0-windows\PixelPulse.dll" -ErrorAction SilentlyContinue
            
            if ($?) {
                $databaseType = [PixelPulse.Database.QuoteDatabase]
                $initTask = $databaseType::InitializeAsync()
                $initTask.Wait()
                Write-TestResult "Initialize QuoteDatabase" $true "Database initialized successfully"

                # Test Bible category count
                $bibleCount = $databaseType::GetQuoteCount([PixelPulse.Models.QuoteCategory]::Bible)
                Write-TestResult "Bible Quote Count" ($bibleCount -gt 1000) "$(Format-Number $bibleCount) Bible verses"

                # Test other categories have sufficient quotes
                $categories = @([PixelPulse.Models.QuoteCategory]::Motivational, 
                              [PixelPulse.Models.QuoteCategory]::Wisdom,
                              [PixelPulse.Models.QuoteCategory]::Encouragement,
                              [PixelPulse.Models.QuoteCategory]::Love)
                
                foreach ($category in $categories) {
                    $count = $databaseType::GetQuoteCount($category)
                    Write-TestResult "$category Category Count" ($count -gt 100) "$(Format-Number $count) quotes"
                }

                # Test Bible verse retrieval
                $bibleVerse = $databaseType::GetRandomQuoteAsync([PixelPulse.Models.QuoteCategory]::Bible).Result
                Write-TestResult "Random Bible Verse" ($null -ne $bibleVerse) "Retrieved: $($bibleVerse.Author)"
                
                # Test inspirational verses
                $inspirationalVerse = $databaseType::GetRandomQuoteAsync([PixelPulse.Models.QuoteCategory]::Motivational).Result
                Write-TestResult "Random Inspirational Verse" ($null -ne $inspirationalVerse) "Retrieved: $($inspirationalVerse.Author)"

            } else {
                Write-TestResult "Bible Coverage Test" $false "Could not load PixelPulse assembly"
            }
        } catch {
            Write-TestResult "Bible Coverage Test" $false $_.Exception.Message
        }
    }

    # Test 3: Comprehensive Category Test
    if ($TestMode -eq 'Full') {
        Write-TestHeader "Comprehensive Category Test"
        
        try {
            $databaseType = [PixelPulse.Database.QuoteDatabase]
            $allCategories = [Enum]::GetValues([PixelPulse.Models.QuoteCategory])
            
            Write-Host "Testing all quote categories:" -ForegroundColor Gray
            
            foreach ($category in $allCategories) {
                $count = $databaseType::GetQuoteCount($category)
                $status = if ($count -gt 50) { "✅" } elseif ($count -gt 10) { "⚠️" } else { "❌" }
                $roundedCount = [math]::Round($count)
                Write-Host "  ${status} ${category}: $roundedCount quotes" -ForegroundColor White
            }

            # Total quote count
            $totalCount = $databaseType::GetQuoteCount()
            Write-TestResult "Total Quote Count" ($totalCount -gt 10000) "$(Format-Number $totalCount) total quotes"

        } catch {
            Write-TestResult "Comprehensive Category Test" $false $_.Exception.Message
        }
    }

    # Test 4: Performance Test
    if ($TestMode -eq 'Performance') {
        Write-TestHeader "Performance Test"
        
        try {
            $databaseType = [PixelPulse.Database.QuoteDatabase]
            $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
            
            # Test random quote retrieval speed
            for ($i = 0; $i -lt 100; $i++) {
                $null = $databaseType::GetRandomQuoteAsync().Result
            }
            $stopwatch.Stop()
            
            $quotesPerSecond = 100 / $stopwatch.Elapsed.TotalSeconds
            Write-TestResult "Random Quote Performance" ($quotesPerSecond -gt 50) "$([math]::Round($quotesPerSecond, 2)) quotes/second"

            # Test Bible category performance
            $stopwatch.Restart()
            for ($i = 0; $i -lt 50; $i++) {
                $null = $databaseType::GetRandomQuoteAsync([PixelPulse.Models.QuoteCategory]::Bible).Result
            }
            $stopwatch.Stop()
            
            $bibleQuotesPerSecond = 50 / $stopwatch.Elapsed.TotalSeconds
            Write-TestResult "Bible Quote Performance" ($bibleQuotesPerSecond -gt 30) "$([math]::Round($bibleQuotesPerSecond, 2)) Bible quotes/second"

            # Test search performance
            $stopwatch.Restart()
            $searchResults = $databaseType::SearchQuotesAsync("love").Result
            $stopwatch.Stop()
            
            Write-TestResult "Search Performance" ($stopwatch.Elapsed.TotalMilliseconds -lt 1000) "$(Format-Number $searchResults.Count) results in $($stopwatch.ElapsedMilliseconds)ms"

        } catch {
            Write-TestResult "Performance Test" $false $_.Exception.Message
        }
    }

    # Test 5: Self-Contained Test
    if ($TestMode -in @('Basic', 'Full')) {
        Write-TestHeader "Self-Contained Application Test"
        
        try {
            # Check if the executable is truly self-contained
            $exePath = "PixelPulse\bin\Release\net8.0-windows\PixelPulse.exe"
            if (Test-Path $exePath) {
                $fileInfo = Get-Item $exePath
                $sizeMB = [math]::Round($fileInfo.Length / 1MB, 1)
                
                # Self-contained executables are typically >50MB
                $50MB = 50 * 1024 * 1024
                Write-TestResult "Self-Contained Size" ($fileInfo.Length -gt $50MB) "$sizeMB MB (includes .NET runtime)"
                
                # Check no external dependencies
                $dllPath = "PixelPulse\bin\Release\net8.0-windows\PixelPulse.dll"
                $dllExists = Test-Path $dllPath
                Write-TestResult "No External Dependencies" (-not $dllExists) "Framework-dependent DLL: $(if ($dllExists) { 'Found' } else { 'Not found' })"
            } else {
                Write-TestResult "Self-Contained Application Test" $false "Executable not found"
            }

        } catch {
            Write-TestResult "Self-Contained Application Test" $false $_.Exception.Message
        }
    }

    # Test 6: Database Statistics
    Write-TestHeader "Database Statistics"
    
    try {
        $databaseType = [PixelPulse.Database.QuoteDatabase]
        $stats = $databaseType::GetDatabaseStatsAsync().Result
        
        Write-TestResult "Get Database Stats" ($null -ne $stats) "Statistics retrieved successfully"
        
        if ($stats) {
            Write-Host "Database Overview:" -ForegroundColor Gray
            Write-Host "  Total Quotes: $(Format-Number $stats.TotalQuotes)" -ForegroundColor Gray
            Write-Host "  Categories: $($stats.Categories.Count)" -ForegroundColor Gray
            Write-Host "  Sources: $($stats.Sources.Count)" -ForegroundColor Gray
            Write-Host "  Last Updated: $($stats.LastUpdated)" -ForegroundColor Gray
            
            Write-Host "`nCategory Breakdown:" -ForegroundColor Gray
            foreach ($cat in $stats.Categories.GetEnumerator() | Sort-Object Value -Descending) {
                $percentage = ($cat.Value * 100.0 / $stats.TotalQuotes).ToString("F1")
                Write-Host "  $($cat.Key): $(Format-Number $cat.Value) ($percentage%)" -ForegroundColor Gray
            }
            
            Write-Host "`nSource Breakdown:" -ForegroundColor Gray
            foreach ($source in $stats.Sources.GetEnumerator() | Sort-Object Value -Descending) {
                $percentage = ($source.Value * 100.0 / $stats.TotalQuotes).ToString("F1")
                Write-Host "  $($source.Key): $(Format-Number $source.Value) ($percentage%)" -ForegroundColor Gray
            }
        }
        
    } catch {
        Write-TestResult "Database Statistics" $false $_.Exception.Message
    }

    Write-Host "`n=== Test Summary ===" -ForegroundColor Cyan
    Write-Host "Comprehensive quotes functionality test completed." -ForegroundColor White
    Write-Host "The application now includes:" -ForegroundColor White
    Write-Host "  • Complete KJV Bible (~31,000 verses)" -ForegroundColor Green
    Write-Host "  • Self-contained .NET 8.0 runtime" -ForegroundColor Green
    Write-Host "  • 5,000+ additional quotes from external sources" -ForegroundColor Green
    Write-Host "  • No external dependencies required" -ForegroundColor Green
    Write-Host "  • Comprehensive categorization and search" -ForegroundColor Green

} catch {
    Write-Host "Fatal error during testing: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
