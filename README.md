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
```bash
# Build the solution
dotnet build

# Run the console app
dotnet run --project src/LogFileParser.Console/LogFileParser.Console.csproj -- data/programming-task-example-data.log

# Run tests
dotnet test
```

## Project Structure
```
LogFileParser/
├─ src/
│  ├─ LogFileParser.Application/            # Use cases and interfaces
│  ├─ LogFileParser.Console/                # Console UI
│  ├─ LogFileParser.Domain/                 # Entities and core domain logic
│  └─ LogFileParser.Infrastructure/         # Implementations (parsing, file I/O)
└─ tests/
│  ├─ LogFileParser.Application.Tests/
│  ├─ LogFileParser.Domain.Tests/
│  └─ LogFileParser.Infrastructure.Tests/
└─ data/
   └─ programming-task-example-data.log     # Test data
```

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
