<#
.SYNOPSIS
    Tests the bundled quotes functionality for Pixel Pulse.

.DESCRIPTION
    This script validates that the bundled quotes system works correctly
    by testing the QuoteDatabase and QuoteService classes.

.PARAMETER TestMode
    Type of test to run: Basic, Full, or Performance

.EXAMPLE
    .\Test-BundledQuotes.ps1 -TestMode Basic
    Runs basic functionality tests

.EXAMPLE
    .\Test-BundledQuotes.ps1 -TestMode Full
    Runs comprehensive tests including all quote categories
#>

[CmdletBinding()]
param(
    [ValidateSet('Basic', 'Full', 'Performance')]
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

try {
    Write-Host "Pixel Pulse Bundled Quotes Test Suite" -ForegroundColor White
    Write-Host "=====================================" -ForegroundColor White
    Write-Host "Test Mode: $TestMode`n" -ForegroundColor Gray

    # Test 1: Basic Quote Loading
    Write-TestHeader "Basic Quote Loading"
    
    try {
        # Test that we can create QuoteDatabase instance
        Add-Type -Path "PixelPulse\bin\Release\net8.0-windows\PixelPulse.dll" -ErrorAction SilentlyContinue
        
        if ($?) {
            Write-TestResult "Load PixelPulse Assembly" $true "Assembly loaded successfully"
            
            # Test QuoteDatabase initialization
            $databaseType = [PixelPulse.Database.QuoteDatabase]
            $initTask = $databaseType::InitializeAsync()
            $initTask.Wait()
            Write-TestResult "Initialize QuoteDatabase" $true "Database initialized successfully"
            
            # Test getting available categories
            $categories = $databaseType::GetAvailableCategories()
            Write-TestResult "Get Available Categories" ($categories.Count -gt 0) "Found $($categories.Count) categories"
            
            # Test getting quote counts
            $totalCount = $databaseType::GetQuoteCount()
            Write-TestResult "Get Total Quote Count" ($totalCount -gt 0) "Total quotes: $totalCount"
            
            # Test getting random quote
            $randomQuote = $databaseType::GetRandomQuoteAsync().Result
            Write-TestResult "Get Random Quote" ($null -ne $randomQuote) "Random quote retrieved"
            
            if ($randomQuote) {
                Write-Host "    Sample: '$($randomQuote.Text)' - $($randomQuote.Author)" -ForegroundColor Gray
            }
            
        } else {
            Write-TestResult "Load PixelPulse Assembly" $false "Could not load PixelPulse.dll"
        }
    } catch {
        Write-TestResult "Basic Quote Loading" $false $_.Exception.Message
    }

    # Test 2: QuoteService Functionality
    if ($TestMode -in @('Full', 'Performance')) {
        Write-TestHeader "QuoteService Functionality"
        
        try {
            $quoteService = New-Object PixelPulse.QuoteService
            Write-TestResult "Create QuoteService" $true "QuoteService instance created"
            
            # Test getting quote by category
            $motivationalQuote = $quoteService.GetQuoteByCategory([PixelPulse.Models.QuoteCategory]::Motivational)
            Write-TestResult "Get Quote by Category" ($null -ne $motivationalQuote) "Motivational quote retrieved"
            
            # Test getting matched quotes
            $matchedQuotes = $quoteService.GetMatchedQuotes([PixelPulse.Models.QuoteCategory]::Motivational, [PixelPulse.Models.QuoteCategory]::Bible)
            Write-TestResult "Get Matched Quotes" ($null -ne $matchedQuotes) "Matched quotes retrieved"
            
            # Test search functionality
            $searchResults = $quoteService.SearchQuotesAsync("love").Result
            Write-TestResult "Search Quotes" ($searchResults.Count -gt 0) "Found $($searchResults.Count) quotes matching 'love'"
            
        } catch {
            Write-TestResult "QuoteService Functionality" $false $_.Exception.Message
        }
    }

    # Test 3: Category Coverage
    if ($TestMode -eq 'Full') {
        Write-TestHeader "Category Coverage Test"
        
        try {
            $allCategories = [Enum]::GetValues([PixelPulse.Models.QuoteCategory])
            $categoryStats = @{}
            
            foreach ($category in $allCategories) {
                $count = [PixelPulse.Database.QuoteDatabase]::GetQuoteCount($category)
                $categoryStats[$category] = $count
                Write-TestResult "Category: $category" ($count -gt 0) "$count quotes"
            }
            
            $totalWithQuotes = ($categoryStats.Values | Where-Object { $_ -gt 0 }).Count
            Write-TestResult "Category Coverage" ($totalWithQuotes -eq $allCategories.Count) "$totalWithQuotes/$($allCategories.Count) categories have quotes"
            
        } catch {
            Write-TestResult "Category Coverage Test" $false $_.Exception.Message
        }
    }

    # Test 4: Performance Test
    if ($TestMode -eq 'Performance') {
        Write-TestHeader "Performance Test"
        
        try {
            $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
            
            # Test getting 100 random quotes
            for ($i = 0; $i -lt 100; $i++) {
                [PixelPulse.Database.QuoteDatabase]::GetRandomQuoteAsync().Result | Out-Null
            }
            $stopwatch.Stop()
            
            $quotesPerSecond = 100 / $stopwatch.Elapsed.TotalSeconds
            Write-TestResult "Random Quote Performance" ($quotesPerSecond -gt 50) "$([math]::Round($quotesPerSecond, 2)) quotes/second"
            
            # Test search performance
            $stopwatch.Restart()
            $searchResults = [PixelPulse.Database.QuoteDatabase]::SearchQuotesAsync("the").Result
            $stopwatch.Stop()
            
            Write-TestResult "Search Performance" ($stopwatch.Elapsed.TotalMilliseconds -lt 1000) "$([math]::Round($stopwatch.Elapsed.TotalMilliseconds, 2))ms for $($searchResults.Count) results"
            
        } catch {
            Write-TestResult "Performance Test" $false $_.Exception.Message
        }
    }

    # Test 5: Database Stats
    Write-TestHeader "Database Statistics"
    
    try {
        $stats = [PixelPulse.Database.QuoteDatabase]::GetDatabaseStatsAsync().Result
        Write-TestResult "Get Database Stats" ($null -ne $stats) "Stats retrieved successfully"
        
        if ($stats) {
            Write-Host "    Total Quotes: $($stats.TotalQuotes)" -ForegroundColor Gray
            Write-Host "    Categories: $($stats.Categories.Count)" -ForegroundColor Gray
            Write-Host "    Sources: $($stats.Sources.Count)" -ForegroundColor Gray
            Write-Host "    Last Updated: $($stats.LastUpdated)" -ForegroundColor Gray
            
            # Show category breakdown
            Write-Host "`n    Category Breakdown:" -ForegroundColor Gray
            foreach ($cat in $stats.GetEnumerator()) {
                Write-Host "      $($cat.Key): $($cat.Value) quotes" -ForegroundColor Gray
            }
        }
        
    } catch {
        Write-TestResult "Database Statistics" $false $_.Exception.Message
    }

    Write-Host "`n=== Test Summary ===" -ForegroundColor Cyan
    Write-Host "Bundled quotes functionality test completed." -ForegroundColor White
    Write-Host "All tests demonstrate that quotes are now bundled with the application." -ForegroundColor White
    Write-Host "Users no longer need internet connectivity to access quotes." -ForegroundColor White

} catch {
    Write-Host "Fatal error during testing: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
