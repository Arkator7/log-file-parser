namespace LogFileParser.Application.Abstractions;

public interface ILogFileReader
{
    IEnumerable<string> ReadLines(string filePath);
}
