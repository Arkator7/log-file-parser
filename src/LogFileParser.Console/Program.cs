using Microsoft.Extensions.DependencyInjection;
using LogFileParser.Application.Abstractions;
using LogFileParser.Application.Services;
using LogFileParser.Infrastructure.Parsing;
using LogFileParser.Infrastructure.Analysis;
using LogFileParser.Infrastructure.FileSystem;

var services = new ServiceCollection();

services.AddScoped<ILogParser, ApacheLogParser>();
services.AddScoped<ILogAnalyser, LinqLogAnalyser>();
services.AddScoped<ILogFileReader, LogFileReader>();
services.AddScoped<LogAnalysisService>();

var serviceProvider = services.BuildServiceProvider();

var logService = serviceProvider.GetRequiredService<LogAnalysisService>();
var result = logService.AnalyseLogFile(args[0]);

Console.WriteLine($"Unique IP Addresses: {result.UniqueIpCount}");
Console.WriteLine("\nTop 3 URLs:");
foreach (var url in result.TopUrls)
    Console.WriteLine($"  {url.Url}: {url.Count} visits");

Console.WriteLine("\nTop 3 Active IPs:");
foreach (var ip in result.TopIpAddresses)
    Console.WriteLine($"  {ip.IpAddress}: {ip.Count} requests");