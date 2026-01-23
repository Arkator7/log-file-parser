namespace LogFileParser.Application.DTOs;

public record LogAnalysisResultDto(
    int UniqueIpCount,
    List<UrlStatistic> TopUrls,
    List<IpStatistic> TopIpAddresses
);

public record UrlStatistic(string Url, int Count);
public record IpStatistic(string IpAddress, int Count);
