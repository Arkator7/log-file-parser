namespace LogFileParser.Domain.Entities;

public class LogEntry
{
    public required string IpAddress { get; set; }
    public required string Identity { get; set; }
    public required string Username { get; set; }
    public required DateTimeOffset Timestamp { get; set; }
    public required string Method { get; set; }
    public required string Url { get; set; }
    public required string Protocol { get; set; }
    public required int StatusCode { get; set; }
    public required long BytesSent { get; set; }
    public required string Referer { get; set; }
    public required string UserAgent { get; set; }
}
