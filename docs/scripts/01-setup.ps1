$solutionName = "LogFileParser"
$rootDir = "log-file-parser"

Write-Host "Creating root directory..."
New-Item -ItemType Directory -Force -Path $rootDir | Out-Null
Set-Location $rootDir

# ----------------
# Create Solution
# ----------------
Write-Host "Creating solution..."
dotnet new sln -n $solutionName

# ----------------
# Create folders
# ----------------
Write-Host "Creating folder structure..."
$folders = @(
    "src",
    "tests",
    "data",
    "docs",
    "docs/scripts"
)

foreach ($folder in $folders) {
    New-Item -ItemType Directory -Force -Path $folder | Out-Null
}

# ----------------
# Create Projects
# ----------------
Write-Host "Creating projects..."

# Core layers
dotnet new classlib -n LogFileParser.Domain         -o src/LogFileParser.Domain
dotnet new classlib -n LogFileParser.Application    -o src/LogFileParser.Application
dotnet new classlib -n LogFileParser.Infrastructure -o src/LogFileParser.Infrastructure

# Presentation layer
dotnet new console  -n LogFileParser.Console        -o src/LogFileParser.Console

# Test projects
dotnet new xunit    -n LogFileParser.Domain.Tests         -o tests/LogFileParser.Domain.Tests
dotnet new xunit    -n LogFileParser.Application.Tests    -o tests/LogFileParser.Application.Tests
dotnet new xunit    -n LogFileParser.Infrastructure.Tests -o tests/LogFileParser.Infrastructure.Tests

# ----------------
# Add projects to solution
# ----------------
Write-Host "Adding projects to solution..."

dotnet sln add src/LogFileParser.Domain/LogFileParser.Domain.csproj
dotnet sln add src/LogFileParser.Application/LogFileParser.Application.csproj
dotnet sln add src/LogFileParser.Infrastructure/LogFileParser.Infrastructure.csproj
dotnet sln add src/LogFileParser.Console/LogFileParser.Console.csproj

dotnet sln add tests/LogFileParser.Domain.Tests/LogFileParser.Domain.Tests.csproj
dotnet sln add tests/LogFileParser.Application.Tests/LogFileParser.Application.Tests.csproj
dotnet sln add tests/LogFileParser.Infrastructure.Tests/LogFileParser.Infrastructure.Tests.csproj

# ----------------
# Add Project References (Following Clean Architecture Dependency Rule)
# ----------------
Write-Host "Adding project references following Clean Architecture principles..."

# Application depends on Domain
dotnet add src/LogFileParser.Application/LogFileParser.Application.csproj reference `
    src/LogFileParser.Domain/LogFileParser.Domain.csproj

# Infrastructure depends on Application (and transitively on Domain)
dotnet add src/LogFileParser.Infrastructure/LogFileParser.Infrastructure.csproj reference `
    src/LogFileParser.Application/LogFileParser.Application.csproj

# Console (Presentation) depends on Application and Infrastructure
dotnet add src/LogFileParser.Console/LogFileParser.Console.csproj reference `
    src/LogFileParser.Application/LogFileParser.Application.csproj `
    src/LogFileParser.Infrastructure/LogFileParser.Infrastructure.csproj

Write-Host "✅ Source project references added successfully."

# ----------------
# Setup Test Projects
# ----------------
Write-Host "Setting up test projects..."

# Domain Tests
Write-Host "Setting up LogFileParser.Domain.Tests..."
dotnet add tests/LogFileParser.Domain.Tests/LogFileParser.Domain.Tests.csproj reference `
    src/LogFileParser.Domain/LogFileParser.Domain.csproj

dotnet add tests/LogFileParser.Domain.Tests/LogFileParser.Domain.Tests.csproj package Moq
dotnet add tests/LogFileParser.Domain.Tests/LogFileParser.Domain.Tests.csproj package Shouldly

# Application Tests
Write-Host "Setting up LogFileParser.Application.Tests..."
dotnet add tests/LogFileParser.Application.Tests/LogFileParser.Application.Tests.csproj reference `
    src/LogFileParser.Application/LogFileParser.Application.csproj

dotnet add tests/LogFileParser.Application.Tests/LogFileParser.Application.Tests.csproj package Moq
dotnet add tests/LogFileParser.Application.Tests/LogFileParser.Application.Tests.csproj package Shouldly

# Infrastructure Tests
Write-Host "Setting up LogFileParser.Infrastructure.Tests..."
dotnet add tests/LogFileParser.Infrastructure.Tests/LogFileParser.Infrastructure.Tests.csproj reference `
    src/LogFileParser.Infrastructure/LogFileParser.Infrastructure.csproj

dotnet add tests/LogFileParser.Infrastructure.Tests/LogFileParser.Infrastructure.Tests.csproj package Moq
dotnet add tests/LogFileParser.Infrastructure.Tests/LogFileParser.Infrastructure.Tests.csproj package Shouldly

Write-Host "✅ Test project references added successfully."

# ----------------
# Add NuGet Packages
# ----------------
Write-Host "Adding NuGet packages..."

# Add Microsoft.Extensions.DependencyInjection to Console app for DI
dotnet add src/LogFileParser.Console/LogFileParser.Console.csproj package Microsoft.Extensions.DependencyInjection

Write-Host "✅ NuGet packages added successfully."

# ----------------
# Enable Central Package Management (Optional)
# ----------------
Write-Host "Setting up Central Package Management..."
if (Get-Command "dotnet-central-pkg-converter" -ErrorAction SilentlyContinue) {
    dotnet central-pkg-converter .
    Write-Host "✅ Central Package Management enabled."
} else {
    Write-Host "⚠️  Skipping Central Package Management (tool not installed)."
    Write-Host "   To enable later, run:"
    Write-Host "   dotnet new tool-manifest"
    Write-Host "   dotnet tool install CentralisedPackageConverter"
    Write-Host "   dotnet central-pkg-converter ."
}

# ----------------
# Create sample data file placeholder
# ----------------
Write-Host "Creating sample data placeholder..."
@"
# Sample Log File
# Place your log file here for testing
# Example format:
# 177.71.128.21 - - [10/Jul/2018:22:21:28 +0200] "GET /intranet-analytics/ HTTP/1.1" 200 3574
"@ | Out-File -Encoding utf8 data/sample.log

# ----------------
# Repo hygiene files
# ----------------
Write-Host "Creating README.md..."
@"
# Log File Parser

A Clean Architecture implementation of an HTTP log file parser.

## Overview
This solution parses Apache/NGINX log files and reports:
- Number of unique IP addresses
- Top 3 most visited URLs
- Top 3 most active IP addresses

## Architecture
This project follows Clean Architecture principles with the following layers:

- **Domain** - Core business entities (no dependencies)
- **Application** - Business logic and use cases (depends on Domain)
- **Infrastructure** - External concerns like file I/O and parsing (depends on Application)
- **Console** - Presentation layer (depends on Application & Infrastructure)

## How to Run
``````bash
# Build the solution
dotnet build

# Run the console app
dotnet run --project src/LogFileParser.Console/LogFileParser.Console.csproj -- data/sample.log

# Run tests
dotnet test
``````

## Project Structure
``````
LogFileParser/
├── src/
│   ├── LogFileParser.Domain/         # Entities and core domain logic
│   ├── LogFileParser.Application/    # Use cases and interfaces
│   ├── LogFileParser.Infrastructure/ # Implementations (parsing, file I/O)
│   └── LogFileParser.Console/        # Console UI
├── tests/
│   ├── LogFileParser.Domain.Tests/
│   ├── LogFileParser.Application.Tests/
│   └── LogFileParser.Infrastructure.Tests/
└── data/
    └── sample.log                    # Test data
``````

## Assumptions
- Log format follows Apache Common Log Format
- Malformed lines are skipped with a warning
- File encoding is UTF-8
- URLs include query strings for uniqueness calculation

## Design Decisions
- Clean Architecture for maintainability and testability
- Regex-based parsing for reliability
- LINQ-based analysis for readability
- Dependency injection for loose coupling

## Future Improvements
- Support for different log formats (NGINX, IIS)
- Streaming for very large files
- Additional statistics and filtering options
- Configuration file support
"@ | Out-File -Encoding utf8 README.md

Write-Host "Creating .gitignore..."
dotnet new gitignore

# ----------------
# Move script to docs
# ----------------
if (Test-Path "..\01-setup.ps1") {
    Move-Item -Path "..\01-setup.ps1" -Destination ".\docs\scripts\" -Force
    Write-Host "✅ Script moved to docs/scripts/"
}

Read-Host "Press Enter to exit"