using LogFileParser.Infrastructure.Parsing;
using Shouldly;

namespace LogFileParser.Infrastructure.Tests.Parsing;

public class ApacheLogParserTests
{
    [Fact]
    public void ParseLine_WithValidLine_ReturnsLogEntry()
    {
        // Arrange
        var logLine = "127.0.0.1 - - [01/Jan/2025:12:00:00 +0000] \"GET /index.html HTTP/1.1\" 200 1234 \"-\" \"Mozilla/5.0\"";

        var lineParser = new ApacheLogParser();

        // Act
        var result = lineParser.ParseLine(logLine);

        // Assert
        result.ShouldNotBeNull();
        result.IpAddress.ShouldBe("127.0.0.1");
        result.Identity.ShouldBe("-");
        result.Username.ShouldBe("-");
        result.Timestamp.ShouldBe(DateTimeOffset.Parse("2025-01-01 12:00:00+00:00"));
        result.Method.ShouldBe("GET");
        result.Url.ShouldBe("/index.html");
        result.Protocol.ShouldBe("HTTP/1.1");
        result.StatusCode.ShouldBe(200);
        result.BytesSent.ShouldBe(1234);
        result.Referer.ShouldBe("-");
        result.UserAgent.ShouldBe("Mozilla/5.0");
    }

    [Fact]
    public void ParseLine_WithInvalidLine_ReturnsNull()
    {
        // Arrange
        var invalidLine = "invalid log line";

        var lineParser = new ApacheLogParser();

        // Act
        var result = lineParser.ParseLine(invalidLine);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void ParseLine_WithHyphenForBytes_ShouldReturnZero()
    {
        // Arrange
        var logLine = @"10.0.0.1 - - [01/Jan/2024:00:00:00 +0000] ""GET /test HTTP/1.1"" 304 - ""-"" ""TestAgent/1.0""";
        var lineParser = new ApacheLogParser();

        // Act
        var result = lineParser.ParseLine(logLine);

        // Assert
        result.ShouldNotBeNull();
        result.BytesSent.ShouldBe(0);
    }

    [Fact]
    public void ParseLine_WithIPv6Address_ShouldParseCorrectly()
    {
        // Arrange
        var logLine = @"2001:0db8:85a3:0000:0000:8a2e:0370:7334 - - [10/Jul/2018:22:21:28 +0200] ""GET /test HTTP/1.1"" 200 100 ""-"" ""Agent""";
        var lineParser = new ApacheLogParser();

        // Act
        var result = lineParser.ParseLine(logLine);

        // Assert
        result.ShouldNotBeNull();
        result.IpAddress.ShouldBe("2001:0db8:85a3:0000:0000:8a2e:0370:7334");
    }

    [Fact]
    public void ParseLine_WithQueryString_ShouldParseCorrectly()
    {
        // Arrange
        var logLine = @"203.0.113.45 - - [20/Dec/2023:10:15:30 +0100] ""GET /search?q=test&page=2 HTTP/1.1"" 200 5000 ""https://google.com"" ""Safari/17.0""";
        var lineParser = new ApacheLogParser();

        // Act
        var result = lineParser.ParseLine(logLine);

        // Assert
        result.ShouldNotBeNull();
        result.Method.ShouldBe("GET");
        result.Url.ShouldBe("/search?q=test&page=2");
        result.Protocol.ShouldBe("HTTP/1.1");
    }

    [Fact]
    public void ParseLine_WithDifferentTimezone_ShouldParseCorrectly()
    {
        // Arrange
        var logLine = @"8.8.8.8 - - [25/Dec/2023:23:59:59 -0500] ""GET /index.html HTTP/1.0"" 200 1234 ""-"" ""Agent""";
        var lineParser = new ApacheLogParser();

        // Act
        var result = lineParser.ParseLine(logLine);

        // Assert
        result.ShouldNotBeNull();
        result.Timestamp.ShouldBe(new DateTimeOffset(2023, 12, 25, 23, 59, 59, TimeSpan.FromHours(-5)));
    }

    [Fact]
    public void ParseLine_WithLargeByteCount_ShouldParseCorrectly()
    {
        // Arrange
        var logLine = @"50.60.70.80 - - [01/Jan/2024:12:00:00 +0000] ""GET /large-file.zip HTTP/1.1"" 200 999999999 ""-"" ""wget/1.20""";
        var lineParser = new ApacheLogParser();

        // Act
        var result = lineParser.ParseLine(logLine);

        // Assert
        result.ShouldNotBeNull();
        result.BytesSent.ShouldBe(999999999);
    }

    [Fact]
    public void ParseLine_WithComplexUserAgent_ShouldParseCorrectly()
    {
        // Arrange
        var logLine = @"192.0.2.1 - - [15/Mar/2024:08:30:00 +0000] ""GET /page HTTP/1.1"" 200 2048 ""-"" ""Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36""";
        var lineParser = new ApacheLogParser();

        // Act
        var result = lineParser.ParseLine(logLine);

        // Assert
        result.ShouldNotBeNull();
        result.UserAgent.ShouldBe("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
    }

    [Fact]
    public void ParseLine_WithEmptyReferer_ShouldParseCorrectly()
    {
        // Arrange
        var logLine = @"100.100.100.100 - - [10/Jul/2018:22:21:28 +0200] ""GET /direct HTTP/1.1"" 200 500 """" ""Agent/1.0""";
        var lineParser = new ApacheLogParser();

        // Act
        var result = lineParser.ParseLine(logLine);

        // Assert
        result.ShouldNotBeNull();
        result.Referer.ShouldBe("");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("This is not a valid log line")]
    [InlineData("177.71.128.21 incomplete log")]
    [InlineData("177.71.128.21 - - [10/Jul/2018:22:21:28")]
    public void ParseLine_WithInvalidLogLine_ShouldReturnNull(string invalidLine)
    {
        // Arrange
        var lineParser = new ApacheLogParser();

        // Act
        var result = lineParser.ParseLine(invalidLine);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void ParseLine_WithMissingFields_ShouldReturnNull()
    {
        // Arrange
        var logLine = @"177.71.128.21 - - [10/Jul/2018:22:21:28 +0200] ""GET /test HTTP/1.1"" 200";
        var lineParser = new ApacheLogParser();

        // Act
        var result = lineParser.ParseLine(logLine);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void ParseLine_WithMissingRequestFields_ShouldReturnNull()
    {
        // Arrange
        var logLine = "127.0.0.1 - - [01/Jan/2025:12:00:00 +0000] \"GET /index.html\" 200 1234 \"-\" \"Mozilla/5.0\"";
        var lineParser = new ApacheLogParser();

        // Act
        var result = lineParser.ParseLine(logLine);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void ParseLine_WithDifferentHttpMethods_ShouldParseCorrectly()
    {
        // Arrange
        var testCases = new[]
        {
            (@"1.1.1.1 - - [10/Jul/2018:22:21:28 +0200] ""POST /api/data HTTP/1.1"" 201 100 ""-"" ""Agent""", "POST"),
            (@"2.2.2.2 - - [10/Jul/2018:22:21:28 +0200] ""PUT /resource/123 HTTP/1.1"" 200 50 ""-"" ""Agent""", "PUT"),
            (@"3.3.3.3 - - [10/Jul/2018:22:21:28 +0200] ""DELETE /item/456 HTTP/1.1"" 204 0 ""-"" ""Agent""", "DELETE"),
            (@"4.4.4.4 - - [10/Jul/2018:22:21:28 +0200] ""PATCH /update HTTP/1.1"" 200 75 ""-"" ""Agent""", "PATCH")
        };

        var lineParser = new ApacheLogParser();

        foreach (var (logLine, expectedMethod) in testCases)
        {
            // Act
            var result = lineParser.ParseLine(logLine);

            // Assert
            result.ShouldNotBeNull();
            result.Method.ShouldBe(expectedMethod);
        }
    }

    [Fact]
    public void ParseLine_WithMultipleConsecutiveSpacesInRequest_ShouldHandleCorrectly()
    {
        var logLine = @"177.71.128.21 - - [10/Jul/2018:22:21:28 +0200] ""GET  /test  HTTP/1.1"" 200 100 ""-"" ""Agent""";
        var lineParser = new ApacheLogParser();

        // Act
        var result = lineParser.ParseLine(logLine);

        // Assert
        result.ShouldNotBeNull();
        result.Method.ShouldBe("GET");
        result.Url.ShouldBe("/test");
        result.Protocol.ShouldBe("HTTP/1.1");
    }
}