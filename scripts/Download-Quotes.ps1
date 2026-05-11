<#
.SYNOPSIS
    Downloads quotes from external sources and builds the quote database for Pixel Pulse.

.DESCRIPTION
    This script runs the QuoteDownloader to fetch quotes from GitHub repositories
    and APIs, then builds a populated SQLite database for bundling with the installer.

.PARAMETER OutputPath
    Path where the quotes.db file should be created. Defaults to PixelPulse/Resources/

.PARAMETER Force
    Overwrite existing quotes.db file if it exists.

.EXAMPLE
    .\Download-Quotes.ps1
    Downloads quotes and creates database in default location

.EXAMPLE
    .\Download-Quotes.ps1 -OutputPath "C:\Temp" -Force
    Downloads quotes to specified path, overwriting existing file
#>

[CmdletBinding()]
param(
    [string]$OutputPath = "",
    [switch]$Force
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'Continue'

function Write-Status {
    param([string]$Message, [string]$Type = 'Info')
    $color = switch ($Type) {
        'Success' { 'Green' }
        'Error' { 'Red' }
        'Warning' { 'Yellow' }
        default { 'White' }
    }
    Write-Host $Message -ForegroundColor $color
}

try {
    Write-Status "Pixel Pulse Quote Database Builder" 'Info'
    Write-Status "====================================" 'Info'
    Write-Host ""

    # Set default output path if not specified
    if ([string]::IsNullOrEmpty($OutputPath)) {
        $OutputPath = Join-Path $PSScriptRoot "..\PixelPulse\Resources"
    }

    # Ensure output directory exists
    if (-not (Test-Path $OutputPath)) {
        New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
        Write-Status "Created output directory: $OutputPath" 'Info'
    }

    $quotesDbPath = Join-Path $OutputPath "quotes.db"

    # Check if database already exists
    if ((Test-Path $quotesDbPath) -and -not $Force) {
        Write-Status "quotes.db already exists at $quotesDbPath" 'Warning'
        $response = Read-Host "Overwrite existing file? (y/N)"
        if ($response -notmatch '^[Yy]') {
            Write-Status "Operation cancelled by user" 'Warning'
            exit 0
        }
    }

    Write-Status "Building QuoteDownloader..." 'Info'
    
    # Build the DatabaseBuilder project
    $builderPath = Join-Path $PSScriptRoot "..\PixelPulse.DatabaseBuilder"
    $builderProject = Join-Path $builderPath "PixelPulse.DatabaseBuilder.csproj"
    
    if (-not (Test-Path $builderProject)) {
        throw "DatabaseBuilder project not found at $builderProject"
    }

    & dotnet build $builderProject -c Release --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to build DatabaseBuilder project"
    }

    Write-Status "Downloading and processing quotes..." 'Info'
    
    # Run the QuoteDownloader
    $builderExe = Join-Path $builderPath "bin\Release\net8.0\PixelPulse.DatabaseBuilder.exe"
    if (-not (Test-Path $builderExe)) {
        # Try to find the executable in different locations
        $builderExe = Get-ChildItem -Path $builderPath -Recurse -Filter "PixelPulse.DatabaseBuilder.exe" | Select-Object -First 1 -ExpandProperty FullName
        if (-not $builderExe) {
            throw "DatabaseBuilder executable not found"
        }
    }

    & $builderExe $OutputPath
    if ($LASTEXITCODE -ne 0) {
        throw "QuoteDownloader failed with exit code $LASTEXITCODE"
    }

    # Verify the database was created
    if (-not (Test-Path $quotesDbPath)) {
        throw "Database file was not created at $quotesDbPath"
    }

    $fileInfo = Get-Item $quotesDbPath
    Write-Status "Database created successfully!" 'Success'
    Write-Status "Location: $quotesDbPath" 'Info'
    Write-Status "Size: $($fileInfo.Length.ToString('N0')) bytes" 'Info'

    # Test the database by running a quick query
    Write-Status "Verifying database integrity..." 'Info'
    
    # Create a temporary test to verify database works
    $testQuery = @"
using System;
using System.IO;
using Microsoft.Data.Sqlite;

class Program {
    static void Main() {
        var dbPath = "$($quotesDbPath.Replace('\', '\\'))";
        using var connection = new SqliteConnection($"Data Source={dbPath}");
        connection.Open();
        
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Quotes";
        
        var count = command.ExecuteScalar();
        Console.WriteLine(\$"Database contains {count} quotes");
        
        command.CommandText = "SELECT Text, Author, Category FROM Quotes LIMIT 3";
        using var reader = command.ExecuteReader();
        
        Console.WriteLine("Sample quotes:");
        while (reader.Read()) {
            Console.WriteLine(\$"  \"{reader[0]}\" - {reader[1]} ({reader[2]})");
        }
    }
}
"@

    $testFile = [System.IO.Path]::GetTempFileName()
    $testFile = [System.IO.Path]::ChangeExtension($testFile, ".cs")
    $testQuery | Out-File -FilePath $testFile -Encoding UTF8

    try {
        & dotnet script $testFile 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Status "Database verification passed!" 'Success'
        } else {
            Write-Status "Database verification failed, but file was created" 'Warning'
        }
    }
    catch {
        Write-Status "Could not verify database (dotnet-script not available), but file was created" 'Warning'
    }
    finally {
        if (Test-Path $testFile) {
            Remove-Item $testFile -Force
        }
    }

    Write-Host ""
    Write-Status "Quote database build completed successfully!" 'Success'
    Write-Status "The quotes.db file is ready to be bundled with the installer." 'Info'

} catch {
    Write-Status "Error: $($_.Exception.Message)" 'Error'
    Write-Status "Stack trace: $($_.Exception.StackTrace)" 'Error'
    exit 1
}
