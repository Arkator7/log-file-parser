using LogFileParser.Application.DTOs;
using LogFileParser.Domain.Entities;

namespace LogFileParser.Application.Abstractions;

public interface ILogAnalyser
{
    LogAnalysisResultDto Analyse(IEnumerable<LogEntry> entries);
}
