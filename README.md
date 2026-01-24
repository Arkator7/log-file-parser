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

# Run the console app and save results in txt file
dotnet run --project src/LogFileParser.Console/LogFileParser.Console.csproj -- data/programming-task-example-data.log > results.txt

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
- Log format follows Apache Common Log format
   - Time format follows `dd/MMM/yyyy:HH:mm:ss zzz` format (e.g. `10/Jul/2018:22:21:28 +0200`)
   - Request format follows `Method Url Protocol` pattern (e.g. `GET /intranet-analytics/ HTTP/1.1`)
- Malformed lines are skipped with a warning
- File encoding is UTF-8
- URLs include query strings for uniqueness calculation
- All HTTP methods (`GET`, `POST`, `PUT`, `DELETE`, etc.) are counted equally for analysis
- Both IPv4 and IPv6 addresses are supported
- "Most active" IP addresses means most requests (not bytes transferred or time spent)
- Status codes (200, 404, 500, etc.) do not affect whether a request is counted in analysis
- The application runs in a single-threaded context (no concurrent file access)

## Design Decisions
- **Clean Architecture** for maintainability and testability
  - Domain layer has no external dependencies
  - Application layer defines interfaces, Infrastructure implements them
  - Dependency flow: Console → Application ← Infrastructure
- **Regex-based parsing** for reliability and handling edge cases
  - Compiled regex for performance optimization
  - Named capture groups for code readability
- **LINQ-based analysis** for readability and maintainability
  - `GroupBy` for aggregation and counting
  - `OrderByDescending` with `Take(3)` for top 3 results
  - `Distinct()` for unique IP counting
- **Dependency injection** for loose coupling and testability
  - Constructor injection throughout
  - Microsoft.Extensions.DependencyInjection for service registration
- **Logging strategy**
  - `ILogger<T>` for structured logging and diagnostics (warnings, errors, debug info)
  - `Console.WriteLine` for program output (allows piping/redirection)
  - `Microsoft.Extensions.Diagnostics.Testing` for testing logging behavior
  - Separation of concerns: diagnostics vs user-facing output
- **Central Package Management**
  - Single source of truth for package versions across all projects
  - Easier dependency updates and version consistency
- **Error handling approach**
  - Graceful degradation: skip malformed lines rather than failing completely
  - Log warnings for skipped lines to aid debugging
  - No exceptions thrown for invalid log entries
- **Testing strategy**
  - Unit tests for individual components (Parser, Analyser)
  - Integration tests using real file I/O with test data files
  - Mocking with Moq for isolating dependencies in Application layer tests
  - Shouldly for expressive, readable assertions
- **Immutability preferences**
  - LogEntry uses `required` properties for clarity and compile-time safety
  - DTOs use `record` types for value semantics and immutability
- **In-memory processing**
  - Entire file is loaded into memory for analysis
  - For production systems with multi-GB files, streaming would be needed

## Trade-offs and Limitations
- **Memory usage**: Current implementation loads all valid entries into memory. For multi-GB log files, a streaming approach would be needed
- **Parse flexibility**: Regex pattern is specific to Apache Common Log Format. Supporting other formats would require strategy pattern or parser factory
- **Top N performance**: Current approach sorts entire dataset. For very large files, a min-heap or priority queue would be more efficient
- **Duplicate detection**: URLs are compared by exact string match (case-sensitive). Normalizing URLs (e.g., lowercasing, removing trailing slashes) was not implemented as it wasn't specified in requirements

## Future Improvements
- Support for different log formats (NGINX, IIS and other custom formats)
- Configuration file for customizing parsing rules and output format
- Additional statistics and filtering options
- Timezone normalization for time-based analysis
- Configuration file support
- Streaming for very large files
  - Process files in a single pass without loading into memory
- Parallel processing for improved performance on large files
  - PLINQ for simple parallelization of LINQ queries, partitioned processing for multi-million line files
- Filter options (by date range, status code, HTTP method)
