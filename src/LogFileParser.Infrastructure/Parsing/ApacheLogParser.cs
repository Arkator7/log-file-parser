using System.Globalization;
using System.Text.RegularExpressions;
using LogFileParser.Application.Abstractions;
using LogFileParser.Domain.Entities;

namespace LogFileParser.Infrastructure.Parsing;

public class ApacheLogParser : ILogParser
{
    private static readonly string LogPattern =
        @"^(?<ip>\S+) (?<identity>\S+) (?<user>\S+) \[(?<time>.+?)\] ""(?<request>.+?)"" (?<status>\d{3}) (?<bytes>\S+) ""(?<referer>.*?)"" ""(?<agent>.*?)""$";

    private static readonly string RequestPattern = @"^(?<method>\S+)\s+(?<url>\S+)\s+(?<protocol>\S+)$";

    private readonly Regex _logRegex = new(LogPattern, RegexOptions.Compiled);
    private readonly Regex _requestRegex = new(RequestPattern, RegexOptions.Compiled);

    public LogEntry? ParseLine(string line)
    {
        var match = _logRegex.Match(line);

        if (!match.Success) return null;

        var requestString = match.Groups["request"].Value;
        var requestMatch = _requestRegex.Match(requestString);

        if (!requestMatch.Success)
        {
            return null;
        }

        return new LogEntry
        {
            IpAddress = match.Groups["ip"].Value,
            Identity = match.Groups["identity"].Value,
            Username = match.Groups["user"].Value,
            Timestamp = DateTimeOffset.ParseExact(match.Groups["time"].Value, "dd/MMM/yyyy:HH:mm:ss zzz", CultureInfo.InvariantCulture, DateTimeStyles.None),
            Method = requestMatch.Groups["method"].Value,
            Url = requestMatch.Groups["url"].Value,
            Protocol = requestMatch.Groups["protocol"].Value,
            StatusCode = int.Parse(match.Groups["status"].Value),
            BytesSent = match.Groups["bytes"].Value == "-" ? 0 : long.Parse(match.Groups["bytes"].Value),
            Referer = match.Groups["referer"].Value,
            UserAgent = match.Groups["agent"].Value
        };
    }
}
