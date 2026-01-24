using LogFileParser.Domain.Entities;
using LogFileParser.Infrastructure.Analysis;
using Shouldly;

namespace LogFileParser.Infrastructure.Tests.Analysis;

public class LinqLogAnalyserTests
{
    [Fact]
    public void Analyse_WithEmptyList_ShouldReturnZeroForAllMetrics()
    {
        // Arrange
        var entries = new List<LogEntry>();
        var analyser = new LinqLogAnalyser();

        // Act
        var result = analyser.Analyse(entries);

        // Assert
        result.UniqueIpCount.ShouldBe(0);
        result.TopUrls.ShouldBeEmpty();
        result.TopIpAddresses.ShouldBeEmpty();
    }

    [Fact]
    public void Analyse_WithSingleEntry_ShouldReturnCorrectMetrics()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            CreateLogEntry("192.168.1.1", "/home")
        };

        var analyser = new LinqLogAnalyser();

        // Act
        var result = analyser.Analyse(entries);

        // Assert
        result.UniqueIpCount.ShouldBe(1);
        result.TopUrls.Count.ShouldBe(1);
        result.TopUrls[0].Url.ShouldBe("/home");
        result.TopUrls[0].Count.ShouldBe(1);
        result.TopIpAddresses.Count.ShouldBe(1);
        result.TopIpAddresses[0].IpAddress.ShouldBe("192.168.1.1");
        result.TopIpAddresses[0].Count.ShouldBe(1);
    }

    [Fact]
    public void Analyse_WithMultipleUniqueIps_ShouldCountCorrectly()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            CreateLogEntry("192.168.1.1", "/home"),
            CreateLogEntry("192.168.1.2", "/about"),
            CreateLogEntry("192.168.1.3", "/contact"),
            CreateLogEntry("192.168.1.4", "/services")
        };

        var analyser = new LinqLogAnalyser();

        // Act
        var result = analyser.Analyse(entries);

        // Assert
        result.UniqueIpCount.ShouldBe(4);
    }

    [Fact]
    public void Analyse_WithDuplicateIps_ShouldCountUniqueOnly()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            CreateLogEntry("192.168.1.1", "/home"),
            CreateLogEntry("192.168.1.1", "/about"),
            CreateLogEntry("192.168.1.2", "/contact"),
            CreateLogEntry("192.168.1.2", "/services"),
            CreateLogEntry("192.168.1.2", "/home")
        };

        var analyser = new LinqLogAnalyser();

        // Act
        var result = analyser.Analyse(entries);

        // Assert
        result.UniqueIpCount.ShouldBe(2);
    }

    [Fact]
    public void Analyse_ShouldReturnTop3MostVisitedUrls()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            CreateLogEntry("192.168.1.1", "/home"),
            CreateLogEntry("192.168.1.2", "/home"),
            CreateLogEntry("192.168.1.3", "/home"),
            CreateLogEntry("192.168.1.4", "/home"),
            CreateLogEntry("192.168.1.5", "/home"),
            
            CreateLogEntry("192.168.1.1", "/about"),
            CreateLogEntry("192.168.1.2", "/about"),
            CreateLogEntry("192.168.1.3", "/about"),
            
            CreateLogEntry("192.168.1.1", "/contact"),
            CreateLogEntry("192.168.1.2", "/contact"),
            CreateLogEntry("192.168.1.3", "/contact"),
            CreateLogEntry("192.168.1.4", "/contact"),
            
            CreateLogEntry("192.168.1.1", "/services"),
            CreateLogEntry("192.168.1.2", "/services")
        };

        var analyser = new LinqLogAnalyser();

        // Act
        var result = analyser.Analyse(entries);

        // Assert
        result.TopUrls.Count.ShouldBe(3);
        result.TopUrls[0].Url.ShouldBe("/home");
        result.TopUrls[0].Count.ShouldBe(5);
        result.TopUrls[1].Url.ShouldBe("/contact");
        result.TopUrls[1].Count.ShouldBe(4);
        result.TopUrls[2].Url.ShouldBe("/about");
        result.TopUrls[2].Count.ShouldBe(3);
    }

    [Fact]
    public void Analyse_WithFewerThan3Urls_ShouldReturnAllUrls()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            CreateLogEntry("192.168.1.1", "/home"),
            CreateLogEntry("192.168.1.2", "/home"),
            CreateLogEntry("192.168.1.1", "/about")
        };

        var analyser = new LinqLogAnalyser();

        // Act
        var result = analyser.Analyse(entries);

        // Assert
        result.TopUrls.Count.ShouldBe(2);
        result.TopUrls[0].Url.ShouldBe("/home");
        result.TopUrls[0].Count.ShouldBe(2);
        result.TopUrls[1].Url.ShouldBe("/about");
        result.TopUrls[1].Count.ShouldBe(1);
    }

    [Fact]
    public void Analyse_ShouldReturnTop3MostActiveIpAddresses()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            CreateLogEntry("192.168.1.1", "/home"),
            CreateLogEntry("192.168.1.1", "/about"),
            CreateLogEntry("192.168.1.1", "/contact"),
            CreateLogEntry("192.168.1.1", "/services"),
            CreateLogEntry("192.168.1.1", "/pricing"),
            
            CreateLogEntry("192.168.1.2", "/home"),
            CreateLogEntry("192.168.1.2", "/about"),
            CreateLogEntry("192.168.1.2", "/contact"),
            
            CreateLogEntry("192.168.1.3", "/home"),
            CreateLogEntry("192.168.1.3", "/about"),
            CreateLogEntry("192.168.1.3", "/contact"),
            CreateLogEntry("192.168.1.3", "/services"),
            
            CreateLogEntry("192.168.1.4", "/home"),
            CreateLogEntry("192.168.1.4", "/about")
        };

        var analyser = new LinqLogAnalyser();

        // Act
        var result = analyser.Analyse(entries);

        // Assert
        result.TopIpAddresses.Count.ShouldBe(3);
        result.TopIpAddresses[0].IpAddress.ShouldBe("192.168.1.1");
        result.TopIpAddresses[0].Count.ShouldBe(5);
        result.TopIpAddresses[1].IpAddress.ShouldBe("192.168.1.3");
        result.TopIpAddresses[1].Count.ShouldBe(4);
        result.TopIpAddresses[2].IpAddress.ShouldBe("192.168.1.2");
        result.TopIpAddresses[2].Count.ShouldBe(3);
    }

    [Fact]
    public void Analyse_WithFewerThan3Ips_ShouldReturnAllIps()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            CreateLogEntry("192.168.1.1", "/home"),
            CreateLogEntry("192.168.1.1", "/about"),
            CreateLogEntry("192.168.1.2", "/contact")
        };

        var analyser = new LinqLogAnalyser();

        // Act
        var result = analyser.Analyse(entries);

        // Assert
        result.TopIpAddresses.Count.ShouldBe(2);
        result.TopIpAddresses[0].IpAddress.ShouldBe("192.168.1.1");
        result.TopIpAddresses[0].Count.ShouldBe(2);
        result.TopIpAddresses[1].IpAddress.ShouldBe("192.168.1.2");
        result.TopIpAddresses[1].Count.ShouldBe(1);
    }

    [Fact]
    public void Analyse_WithTiedCounts_ShouldReturnDeterministicOrder()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            CreateLogEntry("192.168.1.1", "/home"),
            CreateLogEntry("192.168.1.2", "/about"),
            CreateLogEntry("192.168.1.3", "/contact"),
            CreateLogEntry("192.168.1.4", "/services")
        };

        var analyser = new LinqLogAnalyser();

        // Act
        var result = analyser.Analyse(entries);

        // Assert
        result.TopUrls.Count.ShouldBe(3);
        result.TopUrls.ShouldAllBe(url => url.Count == 1);
    }

    [Fact]
    public void Analyse_WithUrlsContainingQueryStrings_ShouldTreatAsDifferentUrls()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            CreateLogEntry("192.168.1.1", "/search?q=test"),
            CreateLogEntry("192.168.1.2", "/search?q=test"),
            CreateLogEntry("192.168.1.3", "/search?q=other"),
            CreateLogEntry("192.168.1.4", "/search")
        };

        var analyser = new LinqLogAnalyser();

        // Act
        var result = analyser.Analyse(entries);

        // Assert
        result.TopUrls.Count.ShouldBe(3);
        result.TopUrls[0].Url.ShouldBe("/search?q=test");
        result.TopUrls[0].Count.ShouldBe(2);
    }

    [Fact]
    public void Analyse_WithIPv6Addresses_ShouldHandleCorrectly()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            CreateLogEntry("2001:0db8:85a3:0000:0000:8a2e:0370:7334", "/home"),
            CreateLogEntry("2001:0db8:85a3:0000:0000:8a2e:0370:7334", "/about"),
            CreateLogEntry("fe80::1", "/contact")
        };

        var analyser = new LinqLogAnalyser();

        // Act
        var result = analyser.Analyse(entries);

        // Assert
        result.UniqueIpCount.ShouldBe(2);
        result.TopIpAddresses[0].IpAddress.ShouldBe("2001:0db8:85a3:0000:0000:8a2e:0370:7334");
        result.TopIpAddresses[0].Count.ShouldBe(2);
    }

    [Fact]
    public void Analyse_WithLargeDataset_ShouldPerformEfficiently()
    {
        // Arrange
        var entries = new List<LogEntry>();
        for (int i = 0; i < 10000; i++)
        {
            entries.Add(CreateLogEntry($"192.168.{i % 255}.{i % 255}", $"/page{i % 100}"));
        }

        var analyser = new LinqLogAnalyser();

        // Act
        var result = analyser.Analyse(entries);

        // Assert
        result.UniqueIpCount.ShouldBeGreaterThan(0);
        result.TopUrls.Count.ShouldBe(3);
        result.TopIpAddresses.Count.ShouldBe(3);
    }

    [Fact]
    public void Analyse_WithIdenticalRequests_ShouldCountAllOccurrences()
    {
        // Arrange
        var entries = new List<LogEntry>
        {
            CreateLogEntry("192.168.1.1", "/api/endpoint"),
            CreateLogEntry("192.168.1.1", "/api/endpoint"),
            CreateLogEntry("192.168.1.1", "/api/endpoint"),
            CreateLogEntry("192.168.1.1", "/api/endpoint"),
            CreateLogEntry("192.168.1.1", "/api/endpoint")
        };

        var analyser = new LinqLogAnalyser();

        // Act
        var result = analyser.Analyse(entries);

        // Assert
        result.UniqueIpCount.ShouldBe(1);
        result.TopUrls[0].Count.ShouldBe(5);
        result.TopIpAddresses[0].Count.ShouldBe(5);
    }

    private static LogEntry CreateLogEntry(string ipAddress, string url, string request = "GET")
    {
        return new LogEntry
        {
            IpAddress = ipAddress,
            Identity = "-",
            Username = "-",
            Timestamp = DateTimeOffset.UtcNow,
            Method = request,
            Url = url,
            Protocol = "HTTP/1.1",
            StatusCode = 200,
            BytesSent = 1024,
            Referer = "-",
            UserAgent = "TestAgent/1.0"
        };
    }
}