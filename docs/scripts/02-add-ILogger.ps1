# ----------------
# Add ILogger NuGet Packages
# ----------------
Write-Host "Adding Microsoft.Extensions.Logging packages..."

dotnet add src/LogFileParser.Application/LogFileParser.Application.csproj package Microsoft.Extensions.Logging.Abstractions
dotnet add src/LogFileParser.Console/LogFileParser.Console.csproj package Microsoft.Extensions.Logging
dotnet add src/LogFileParser.Console/LogFileParser.Console.csproj package Microsoft.Extensions.Logging.Console
dotnet add tests/LogFileParser.Application.Tests/LogFileParser.Application.Tests.csproj package Microsoft.Extensions.Logging.Abstractions
dotnet add tests/LogFileParser.Infrastructure.Tests/LogFileParser.Infrastructure.Tests.csproj package Microsoft.Extensions.Diagnostics.Testing
dotnet add tests/LogFileParser.Infrastructure.Tests/LogFileParser.Infrastructure.Tests.csproj package Microsoft.Extensions.Logging
dotnet add tests/LogFileParser.Infrastructure.Tests/LogFileParser.Infrastructure.Tests.csproj package Microsoft.Extensions.Logging.Testing

Write-Host "✅ Logging packages added successfully."
