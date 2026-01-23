using LogFileParser.Application.Abstractions;

namespace LogFileParser.Infrastructure.FileSystem;

public class LogFileReader : ILogFileReader
{
    public IEnumerable<string> ReadLines(string filePath)
    {
        return File.ReadLines(filePath);
    }
}