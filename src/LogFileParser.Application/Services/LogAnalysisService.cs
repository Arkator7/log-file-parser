using Microsoft.Extensions.Logging;
using LogFileParser.Application.Abstractions;
using LogFileParser.Application.DTOs;
using LogFileParser.Domain.Entities;

namespace LogFileParser.Application.Services;

public class LogAnalysisService
{
    private readonly ILogParser _parser;
    private readonly ILogAnalyser _analyser;
    private readonly ILogFileReader _fileReader;
    private readonly ILogger<LogAnalysisService> _logger;

    public LogAnalysisService(
        ILogParser parser, 
        ILogAnalyser analyser,
        ILogFileReader fileReader,
        ILogger<LogAnalysisService> logger)
    {
        _parser = parser;
        _analyser = analyser;
        _fileReader = fileReader;
        _logger = logger;
    }

    public LogAnalysisResultDto AnalyseLogFile(string filePath)
    {
        var lines = _fileReader.ReadLines(filePath).ToList();
        
        var results = lines
            .Select((line, index) => ParseLineWithWarning(line, index + 1))
            .ToList();
        
        var entries = results
            .Where(r => r.entry != null)
            .Select(r => r.entry!)
            .ToList();
        
        var malformedCount = results.Count(r => r.isMalformed);
        if (malformedCount > 0)
        {
            _logger.LogWarning("Skipped {MalformedCount} malformed line(s) out of {TotalLines} total lines", malformedCount, lines.Count);
        }

        return _analyser.Analyse(entries);
    }

    private (LogEntry? entry, bool isMalformed) ParseLineWithWarning(string line, int lineNumber)
    {
        var entry = _parser.ParseLine(line);
        if (entry == null)
        {
            _logger.LogWarning("Skipping malformed line {LineNumber}: {Line}", lineNumber, line);
            return (null, true);
        }
        return (entry, false);
    }
}