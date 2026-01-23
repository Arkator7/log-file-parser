using LogFileParser.Application.Abstractions;
using LogFileParser.Application.DTOs;

namespace LogFileParser.Application.Services;

public class LogAnalysisService
{
    private readonly ILogParser _parser;
    private readonly ILogAnalyser _analyser;
    private readonly ILogFileReader _fileReader;

    public LogAnalysisService(
        ILogParser parser, 
        ILogAnalyser analyser,
        ILogFileReader fileReader)
    {
        _parser = parser;
        _analyser = analyser;
        _fileReader = fileReader;
    }

    public LogAnalysisResultDto AnalyseLogFile(string filePath)
    {
        var lines = _fileReader.ReadLines(filePath);
        var entries = lines
            .Select(_parser.ParseLine)
            .Where(entry => entry != null)
            .ToList();

        return _analyser.Analyse(entries!);
    }
}