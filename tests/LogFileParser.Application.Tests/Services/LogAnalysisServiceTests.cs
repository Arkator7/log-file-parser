using LogFileParser.Application.Abstractions;
using LogFileParser.Application.DTOs;
using LogFileParser.Application.Services;
using LogFileParser.Domain.Entities;
using Moq;
using Shouldly;

namespace LogFileParser.Application.Tests.Services;

public class LogAnalysisServiceTests
{
    [Fact]
    public void AnalyseLogFile_WithValidFile_ShouldReadParseAndAnalyse()
    {
        // Arrange
        var filePath = "/path/to/log.file";
        var logLines = new List<string>
        {
            "192.168.1.1 - - [10/Jul/2018:22:21:28 +0200] \"GET /home HTTP/1.1\" 200 1024 \"-\" \"Agent\"",
            "192.168.1.2 - - [10/Jul/2018:22:22:28 +0200] \"GET /about HTTP/1.1\" 200 2048 \"-\" \"Agent\""
        };

        var parsedEntries = new List<LogEntry>
        {
            CreateLogEntry("192.168.1.1", "/home"),
            CreateLogEntry("192.168.1.2", "/about")
        };

        var expectedResult = new LogAnalysisResultDto(
            UniqueIpCount: 2,
            TopUrls: new List<UrlStatistic> { new("/home", 1) },
            TopIpAddresses: new List<IpStatistic> { new("192.168.1.1", 1) }
        );

        var parser = new Mock<ILogParser>();
        parser.Setup(x => x.ParseLine(logLines[0])).Returns(parsedEntries[0]);
        parser.Setup(x => x.ParseLine(logLines[1])).Returns(parsedEntries[1]);

        var analyser = new Mock<ILogAnalyser>();
        analyser.Setup(x => x.Analyse(It.IsAny<List<LogEntry>>())).Returns(expectedResult);

        var fileReader = new Mock<ILogFileReader>();
        fileReader.Setup(x => x.ReadLines(filePath)).Returns(logLines);

        var service = new LogAnalysisService(parser.Object, analyser.Object, fileReader.Object);

        // Act
        var result = service.AnalyseLogFile(filePath);

        // Assert
        result.ShouldBe(expectedResult);

        fileReader.Verify(x => x.ReadLines(filePath), Times.Once);
        parser.Verify(x => x.ParseLine(It.IsAny<string>()), Times.Exactly(2));
        analyser.Verify(x => x.Analyse(It.Is<List<LogEntry>>(list => list.Count == 2)), Times.Once);
    }

    [Fact]
    public void AnalyseLogFile_WithEmptyFile_ShouldReturnEmptyAnalysis()
    {
        // Arrange
        var filePath = "/path/to/empty.log";
        var emptyLines = new List<string>();
        var expectedResult = new LogAnalysisResultDto(
            UniqueIpCount: 0,
            TopUrls: new List<UrlStatistic>(),
            TopIpAddresses: new List<IpStatistic>()
        );

        var analyser = new Mock<ILogAnalyser>();
        analyser.Setup(x => x.Analyse(It.IsAny<List<LogEntry>>())).Returns(expectedResult);

        var fileReader = new Mock<ILogFileReader>();
        fileReader.Setup(x => x.ReadLines(filePath)).Returns(emptyLines);

        var service = new LogAnalysisService(Mock.Of<ILogParser>(), analyser.Object, fileReader.Object);

        // Act
        var result = service.AnalyseLogFile(filePath);

        // Assert
        result.UniqueIpCount.ShouldBe(0);
        analyser.Verify(x => x.Analyse(It.Is<List<LogEntry>>(list => list.Count == 0)), Times.Once);
    }

    [Fact]
    public void AnalyseLogFile_WithMalformedLines_ShouldSkipNullEntries()
    {
        // Arrange
        var filePath = "/path/to/log.file";
        var logLines = new List<string>
        {
            "192.168.1.1 - - [10/Jul/2018:22:21:28 +0200] \"GET /home HTTP/1.1\" 200 1024 \"-\" \"Agent\"",
            "This is an invalid log line",
            "Another bad line",
            "192.168.1.2 - - [10/Jul/2018:22:22:28 +0200] \"GET /about HTTP/1.1\" 200 2048 \"-\" \"Agent\""
        };

        var validEntry1 = CreateLogEntry("192.168.1.1", "/home");
        var validEntry2 = CreateLogEntry("192.168.1.2", "/about");

        var parser = new Mock<ILogParser>();
        parser.Setup(x => x.ParseLine(logLines[0])).Returns(validEntry1);
        parser.Setup(x => x.ParseLine(logLines[1])).Returns((LogEntry?)null);
        parser.Setup(x => x.ParseLine(logLines[2])).Returns((LogEntry?)null);
        parser.Setup(x => x.ParseLine(logLines[3])).Returns(validEntry2);

        var analyser = new Mock<ILogAnalyser>();
        analyser.Setup(x => x.Analyse(It.IsAny<List<LogEntry>>())).Returns(
            new LogAnalysisResultDto(2, new List<UrlStatistic>(), new List<IpStatistic>())
        );

        var fileReader = new Mock<ILogFileReader>();
        fileReader.Setup(x => x.ReadLines(filePath)).Returns(logLines);

        var service = new LogAnalysisService(parser.Object, analyser.Object, fileReader.Object);

        // Act
        var result = service.AnalyseLogFile(filePath);

        // Assert
        parser.Verify(x => x.ParseLine(It.IsAny<string>()), Times.Exactly(4));
        analyser.Verify(x => x.Analyse(It.Is<List<LogEntry>>(list => list.Count == 2)), Times.Once);
    }

    [Fact]
    public void AnalyseLogFile_WithAllMalformedLines_ShouldAnalyseEmptyList()
    {
        // Arrange
        var filePath = "/path/to/bad.log";
        var logLines = new List<string>
        {
            "Invalid line 1",
            "Invalid line 2",
            "Invalid line 3"
        };

        var parser = new Mock<ILogParser>();
        parser.Setup(x => x.ParseLine(It.IsAny<string>())).Returns((LogEntry?)null);

        var analyser = new Mock<ILogAnalyser>();
        analyser.Setup(x => x.Analyse(It.IsAny<List<LogEntry>>())).Returns(
            new LogAnalysisResultDto(0, new List<UrlStatistic>(), new List<IpStatistic>())
        );

        var fileReader = new Mock<ILogFileReader>();
        fileReader.Setup(x => x.ReadLines(filePath)).Returns(logLines);

        var service = new LogAnalysisService(parser.Object, analyser.Object, fileReader.Object);

        // Act
        var result = service.AnalyseLogFile(filePath);

        // Assert
        analyser.Verify(x => x.Analyse(It.Is<List<LogEntry>>(list => list.Count == 0)), Times.Once);
    }

    [Fact]
    public void AnalyseLogFile_ShouldPassCorrectFilePathToReader()
    {
        // Arrange
        var filePath = "/custom/path/to/application.log";

        var analyser = new Mock<ILogAnalyser>();
        analyser.Setup(x => x.Analyse(It.IsAny<List<LogEntry>>())).Returns(
            new LogAnalysisResultDto(0, new List<UrlStatistic>(), new List<IpStatistic>())
        );

        var fileReader = new Mock<ILogFileReader>();
        fileReader.Setup(x => x.ReadLines(filePath)).Returns(new List<string>());

        var service = new LogAnalysisService(Mock.Of<ILogParser>(), analyser.Object, fileReader.Object);

        // Act
        service.AnalyseLogFile(filePath);

        // Assert
        fileReader.Verify(x => x.ReadLines(filePath), Times.Once);
    }

    [Fact]
    public void AnalyseLogFile_WithLargeFile_ShouldProcessAllLines()
    {
        // Arrange
        var filePath = "/path/to/large.log";
        var logLines = Enumerable.Range(1, 1000)
            .Select(i => $"192.168.1.{i % 255} - - [10/Jul/2018:22:21:28 +0200] \"GET /page{i} HTTP/1.1\" 200 1024 \"-\" \"Agent\"")
            .ToList();

        var parsedEntries = Enumerable.Range(1, 1000)
            .Select(i => CreateLogEntry($"192.168.1.{i % 255}", $"/page{i}"))
            .ToList();

        var parser = new Mock<ILogParser>();
        var analyser = new Mock<ILogAnalyser>();

        var fileReader = new Mock<ILogFileReader>();
        fileReader.Setup(x => x.ReadLines(filePath)).Returns(logLines);
        var service = new LogAnalysisService(parser.Object, analyser.Object, fileReader.Object);
        
        for (int i = 0; i < 1000; i++)
        {
            var localIndex = i; // Capture for closure
            parser.Setup(x => x.ParseLine(logLines[localIndex])).Returns(parsedEntries[localIndex]);
        }

        analyser.Setup(x => x.Analyse(It.IsAny<List<LogEntry>>())).Returns(
            new LogAnalysisResultDto(255, new List<UrlStatistic>(), new List<IpStatistic>())
        );

        // Act
        service.AnalyseLogFile(filePath);

        // Assert
        parser.Verify(x => x.ParseLine(It.IsAny<string>()), Times.Exactly(1000));
        analyser.Verify(x => x.Analyse(It.Is<List<LogEntry>>(list => list.Count == 1000)), Times.Once);
    }

    [Fact]
    public void AnalyseLogFile_ShouldFilterNullsBeforeAnalysis()
    {
        // Arrange
        var filePath = "/path/to/mixed.log";
        var logLines = new List<string> { "line1", "line2", "line3", "line4", "line5" };

        var parser = new Mock<ILogParser>();
        parser.Setup(x => x.ParseLine("line1")).Returns(CreateLogEntry("1.1.1.1", "/a"));
        parser.Setup(x => x.ParseLine("line2")).Returns((LogEntry?)null);
        parser.Setup(x => x.ParseLine("line3")).Returns(CreateLogEntry("2.2.2.2", "/b"));
        parser.Setup(x => x.ParseLine("line4")).Returns((LogEntry?)null);
        parser.Setup(x => x.ParseLine("line5")).Returns(CreateLogEntry("3.3.3.3", "/c"));

        LogAnalysisResultDto? capturedAnalysisInput = null;
        var analyser = new Mock<ILogAnalyser>();
        analyser.Setup(x => x.Analyse(It.IsAny<List<LogEntry>>()))
            .Callback<IEnumerable<LogEntry>>(entries => {
                var list = entries.ToList();
                capturedAnalysisInput = new LogAnalysisResultDto(
                    list.Count,
                    new List<UrlStatistic>(),
                    new List<IpStatistic>()
                );
            })
            .Returns(new LogAnalysisResultDto(3, new List<UrlStatistic>(), new List<IpStatistic>()));

        var fileReader = new Mock<ILogFileReader>();
        fileReader.Setup(x => x.ReadLines(filePath)).Returns(logLines);

        var service = new LogAnalysisService(parser.Object, analyser.Object, fileReader.Object);

        // Act
        service.AnalyseLogFile(filePath);

        // Assert
        analyser.Verify(x => x.Analyse(It.Is<List<LogEntry>>(list => 
            list.Count == 3 && 
            list.All(e => e != null)
        )), Times.Once);
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