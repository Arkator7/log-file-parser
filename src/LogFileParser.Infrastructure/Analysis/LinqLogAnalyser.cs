using LogFileParser.Application.Abstractions;
using LogFileParser.Application.DTOs;
using LogFileParser.Domain.Entities;

namespace LogFileParser.Infrastructure.Analysis;

public class LinqLogAnalyser : ILogAnalyser
{
    public LogAnalysisResultDto Analyse(IEnumerable<LogEntry> entries)
    {
        var entryList = entries.ToList();

        var uniqueIps = entryList
            .Select(e => e.IpAddress)
            .Distinct()
            .Count();

        var topUrls = entryList
            .Select(e => e.Url)
            .GroupBy(url => url)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => new UrlStatistic(g.Key, g.Count()))
            .ToList();

        var topIps = entryList
            .GroupBy(e => e.IpAddress)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => new IpStatistic(g.Key, g.Count()))
            .ToList();

        return new LogAnalysisResultDto(uniqueIps, topUrls, topIps);
    }
}
