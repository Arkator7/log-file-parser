using LogFileParser.Application.Services;
using LogFileParser.Infrastructure.Analysis;
using LogFileParser.Infrastructure.FileSystem;
using LogFileParser.Infrastructure.Parsing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Shouldly;

namespace LogFileParser.Infrastructure.Tests.Integration;

public class LogFileParserIntegrationTests
{
    private const string TestDataFolder = "TestData";

    [Fact]
    public void EndToEnd_WithValidLogFile_ShouldProduceCorrectAnalysis()
    {
        // Arrange
        var parser = new ApacheLogParser();
        var analyser = new LinqLogAnalyser();
        var fileReader = new LogFileReader();
        var logger = new FakeLogger<LogAnalysisService>();

        var service = new LogAnalysisService(parser, analyser, fileReader, logger);

        var logFilePath = GetTestDataPath("valid-log-file.log");

        // Act
        var result = service.AnalyseLogFile(logFilePath);

        // Assert
        result.UniqueIpCount.ShouldBe(3);
        result.TopUrls.Count.ShouldBe(3);
        result.TopUrls[0].Url.ShouldBe("/intranet-analytics/");
        result.TopUrls[0].Count.ShouldBe(3);
        result.TopIpAddresses.Count.ShouldBe(3);
        result.TopIpAddresses[0].IpAddress.ShouldBe("177.71.128.21");
        result.TopIpAddresses[0].Count.ShouldBe(3);
    }

    [Fact]
    public void EndToEnd_WithMixedValidAndInvalidLines_ShouldSkipInvalidLines()
    {
        // Arrange
        var parser = new ApacheLogParser();
        var analyser = new LinqLogAnalyser();
        var fileReader = new LogFileReader();
        var logger = new FakeLogger<LogAnalysisService>();

        var service = new LogAnalysisService(parser, analyser, fileReader, logger);

        var logFilePath = GetTestDataPath("invalid-mixed-with-valid-lines.log");

        // Act
        var result = service.AnalyseLogFile(logFilePath);

        // Assert
        result.UniqueIpCount.ShouldBe(2);
        result.TopUrls[0].Url.ShouldBe("/home");
        result.TopUrls[0].Count.ShouldBe(2);

        var warnings = logger.Collector.GetSnapshot()
            .Where(e => e.Level == LogLevel.Warning)
            .ToList();

        warnings.Count.ShouldBe(4); // 3 individual line warnings + 1 summary warning

        warnings[0].Message.ShouldContain("Skipping malformed line 2");
        warnings[0].Message.ShouldContain("This is an invalid line");
        
        warnings[1].Message.ShouldContain("Skipping malformed line 3");
        warnings[1].Message.ShouldContain("Another invalid line");

        warnings[2].Message.ShouldContain("Skipping malformed line 5");
        warnings[2].Message.ShouldContain("Corrupt data here");

        warnings[3].Message.ShouldContain("Skipped 3 malformed line(s) out of 6 total lines");
    }

    [Fact]
    public void EndToEnd_WithDifferentHttpMethods_ShouldCountAllRequests()
    {
        // Arrange
        var parser = new ApacheLogParser();
        var analyser = new LinqLogAnalyser();
        var fileReader = new LogFileReader();
        var logger = new FakeLogger<LogAnalysisService>();

        var service = new LogAnalysisService(parser, analyser, fileReader, logger);

        var logFilePath = GetTestDataPath("valid-different-http-methods.log");

        // Act
        var result = service.AnalyseLogFile(logFilePath);

        // Assert
        result.UniqueIpCount.ShouldBe(1);
        result.TopIpAddresses[0].Count.ShouldBe(4);
    }

    [Fact]
    public void EndToEnd_WithQueryStrings_ShouldTreatAsSeparateUrls()
    {
        // Arrange
        var parser = new ApacheLogParser();
        var analyser = new LinqLogAnalyser();
        var fileReader = new LogFileReader();
        var logger = new FakeLogger<LogAnalysisService>();

        var service = new LogAnalysisService(parser, analyser, fileReader, logger);

        var logFilePath = GetTestDataPath("valid-with-query-strings.log");

        // Act
        var result = service.AnalyseLogFile(logFilePath);

        // Assert
        result.TopUrls.Count.ShouldBe(3);
        result.TopUrls[0].Url.ShouldBe("/search?q=test");
        result.TopUrls[0].Count.ShouldBe(2);
    }

    [Fact]
    public void EndToEnd_WithIPv6Addresses_ShouldHandleCorrectly()
    {
        // Arrange
        var parser = new ApacheLogParser();
        var analyser = new LinqLogAnalyser();
        var fileReader = new LogFileReader();
        var logger = new FakeLogger<LogAnalysisService>();

        var service = new LogAnalysisService(parser, analyser, fileReader, logger);

        var logFilePath = GetTestDataPath("valid-with-ip6-addresses.log");

        // Act
        var result = service.AnalyseLogFile(logFilePath);

        // Assert
        result.UniqueIpCount.ShouldBe(2);
        result.TopIpAddresses[0].IpAddress.ShouldBe("2001:0db8:85a3:0000:0000:8a2e:0370:7334");
        result.TopIpAddresses[0].Count.ShouldBe(2);
    }

    [Fact]
    public void EndToEnd_WithRealWorldScenario_ShouldProduceExpectedResults()
    {
        // Arrange
        var parser = new ApacheLogParser();
        var analyser = new LinqLogAnalyser();
        var fileReader = new LogFileReader();
        var logger = new FakeLogger<LogAnalysisService>();

        var service = new LogAnalysisService(parser, analyser, fileReader, logger);

        var logFilePath = GetTestDataPath("valid-real-world-data.log");

        // Act
        var result = service.AnalyseLogFile(logFilePath);

        // Assert
        result.UniqueIpCount.ShouldBe(4);

        result.TopUrls.Count.ShouldBe(3);
        result.TopUrls.ShouldContain(u => u.Url == "/intranet-analytics/" && u.Count == 3);
        result.TopUrls.ShouldContain(u => u.Url == "/pricing" && u.Count == 3);

        result.TopIpAddresses.Count.ShouldBe(3);
        result.TopIpAddresses[0].IpAddress.ShouldBe("177.71.128.21");
        result.TopIpAddresses[0].Count.ShouldBe(4);
    }

    [Fact]
    public void EndToEnd_WithDifferentStatusCodes_ShouldCountAllRequests()
    {
        // Arrange
        var parser = new ApacheLogParser();
        var analyser = new LinqLogAnalyser();
        var fileReader = new LogFileReader();
        var logger = new FakeLogger<LogAnalysisService>();

        var service = new LogAnalysisService(parser, analyser, fileReader, logger);

        var logFilePath = GetTestDataPath("valid-with-different-status-codes.log");

        // Act
        var result = service.AnalyseLogFile(logFilePath);

        // Assert
        result.TopIpAddresses[0].Count.ShouldBe(4);
        result.TopUrls.Count.ShouldBe(3);
    }

    [Fact]
    public void EndToEnd_WithProgrammingTaskExampleData_ShouldProduceCorrectAnalysis()
    {
        // Arrange
        var parser = new ApacheLogParser();
        var analyser = new LinqLogAnalyser();
        var fileReader = new LogFileReader();
        var logger = new FakeLogger<LogAnalysisService>();

        var service = new LogAnalysisService(parser, analyser, fileReader, logger);

        var logFilePath = GetTestDataPath("programming-task-example-data.log");

        // Act
        var result = service.AnalyseLogFile(logFilePath);

        // Assert - Business Logic
        result.UniqueIpCount.ShouldBe(11);

        result.TopUrls.Count.ShouldBe(3);
        result.TopUrls[0].Url.ShouldBe("/docs/manage-websites/");
        result.TopUrls[0].Count.ShouldBe(2);
        result.TopUrls[1].Url.ShouldBe("/intranet-analytics/");
        result.TopUrls[1].Count.ShouldBe(1);
        result.TopUrls[2].Url.ShouldBe("http://example.net/faq/");
        result.TopUrls[2].Count.ShouldBe(1);

        result.TopIpAddresses.Count.ShouldBe(3);
        result.TopIpAddresses[0].IpAddress.ShouldBe("168.41.191.40");
        result.TopIpAddresses[0].Count.ShouldBe(4);
        result.TopIpAddresses[1].IpAddress.ShouldBe("177.71.128.21");
        result.TopIpAddresses[1].Count.ShouldBe(3);
        result.TopIpAddresses[2].IpAddress.ShouldBe("50.112.00.11");
        result.TopIpAddresses[2].Count.ShouldBe(3);

        var warnings = logger.Collector.GetSnapshot()
            .Where(e => e.Level == LogLevel.Warning)
            .ToList();
        
        warnings.Count.ShouldBe(3); // 2 individual line warnings + 1 summary warning

        warnings[0].Message.ShouldContain("Skipping malformed line 13");
        warnings[0].Message.ShouldContain("72.44.32.10");
        warnings[0].Message.ShouldContain("junk extra");
        
        warnings[1].Message.ShouldContain("Skipping malformed line 14");
        warnings[1].Message.ShouldContain("168.41.191.9");
        warnings[1].Message.ShouldContain("456 789");

        warnings[2].Message.ShouldContain("Skipped 2 malformed line(s) out of 23 total lines");
    }

    private static string GetTestDataPath(string fileName)
    {
        var assemblyLocation = Path.GetDirectoryName(typeof(LogFileParserIntegrationTests).Assembly.Location);

        if (string.IsNullOrEmpty(assemblyLocation))
        {
            throw new InvalidOperationException("Could not determine test assembly location");
        }

        var testDataPath = Path.Combine(assemblyLocation, TestDataFolder, fileName);

        if (!File.Exists(testDataPath))
        {
            throw new FileNotFoundException($"Test data file not found: {testDataPath}");
        }

        return testDataPath;
    }
}
