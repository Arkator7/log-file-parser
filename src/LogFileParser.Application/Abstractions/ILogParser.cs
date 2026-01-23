using LogFileParser.Domain.Entities;

namespace LogFileParser.Application.Abstractions;

public interface ILogParser
{
    LogEntry? ParseLine(string line);
}
