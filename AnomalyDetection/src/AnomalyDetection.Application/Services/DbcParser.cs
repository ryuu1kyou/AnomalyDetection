using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AnomalyDetection.CanSpecification;

namespace AnomalyDetection.Services;

/// <summary>
/// Parser for DBC (CAN database) files
/// Supports Vector CANdb++ format
/// </summary>
public class DbcParser
{
    private readonly ILogger<DbcParser> _logger;

    public DbcParser(ILogger<DbcParser> logger)
    {
        _logger = logger;
    }

    public async Task<DbcParseResult> ParseAsync(Stream stream)
    {
        var result = new DbcParseResult();

        try
        {
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();

            ParseVersion(content, result);
            ParseMessages(content, result);
            ParseSignals(content, result);
            ParseValueDescriptions(content, result);

            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DBC parse error");
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    private void ParseVersion(string content, DbcParseResult result)
    {
        var versionMatch = Regex.Match(content, @"VERSION\s+""([^""]+)""");
        if (versionMatch.Success)
        {
            result.Version = versionMatch.Groups[1].Value;
        }
    }

    private void ParseMessages(string content, DbcParseResult result)
    {
        // BO_ 1234 MessageName: 8 Transmitter
        var messagePattern = @"BO_\s+(\d+)\s+(\w+):\s+(\d+)\s+(\w+)";
        var matches = Regex.Matches(content, messagePattern);

        foreach (Match match in matches)
        {
            var messageId = uint.Parse(match.Groups[1].Value);
            var name = match.Groups[2].Value;
            var dlc = int.Parse(match.Groups[3].Value);
            var transmitter = match.Groups[4].Value;

            var message = new CanSpecMessage(messageId, name, dlc)
            {
                Transmitter = transmitter
            };

            result.Messages.Add(message);
        }
    }

    private void ParseSignals(string content, DbcParseResult result)
    {
        // SG_ SignalName : 0|8@1+ (1,0) [0|255] "unit" Receiver
        var signalPattern = @"SG_\s+(\w+)\s*:\s*(\d+)\|(\d+)@([01])([+-])\s*\(([^,]+),([^)]+)\)\s*\[([^|]+)\|([^\]]+)\]\s*""([^""]*)""\s*(\w+)";
        var lines = content.Split('\n');

        CanSpecMessage? currentMessage = null;

        foreach (var line in lines)
        {
            // Track current message context
            var messageMatch = Regex.Match(line, @"BO_\s+(\d+)\s+");
            if (messageMatch.Success)
            {
                var msgId = uint.Parse(messageMatch.Groups[1].Value);
                currentMessage = result.Messages.FirstOrDefault(m => m.MessageId == msgId);
                continue;
            }

            var signalMatch = Regex.Match(line, signalPattern);
            if (signalMatch.Success && currentMessage != null)
            {
                var signal = new CanSpecSignal(
                    signalMatch.Groups[1].Value,
                    int.Parse(signalMatch.Groups[2].Value),
                    int.Parse(signalMatch.Groups[3].Value)
                )
                {
                    MessageId = currentMessage.Id,
                    IsBigEndian = signalMatch.Groups[4].Value == "0",
                    IsSigned = signalMatch.Groups[5].Value == "-",
                    Factor = ParseDouble(signalMatch.Groups[6].Value),
                    Offset = ParseDouble(signalMatch.Groups[7].Value),
                    Min = ParseDouble(signalMatch.Groups[8].Value),
                    Max = ParseDouble(signalMatch.Groups[9].Value),
                    Unit = signalMatch.Groups[10].Value,
                    Receiver = signalMatch.Groups[11].Value
                };

                currentMessage.Signals.Add(signal);
            }
        }
    }

    private void ParseValueDescriptions(string content, DbcParseResult result)
    {
        // VAL_ 1234 SignalName 0 "Off" 1 "On" ;
        var valPattern = @"VAL_\s+(\d+)\s+(\w+)\s+(.+?)\s*;";
        var matches = Regex.Matches(content, valPattern);

        foreach (Match match in matches)
        {
            var messageId = uint.Parse(match.Groups[1].Value);
            var signalName = match.Groups[2].Value;
            var values = match.Groups[3].Value;

            var message = result.Messages.FirstOrDefault(m => m.MessageId == messageId);
            var signal = message?.Signals.FirstOrDefault(s => s.Name == signalName);

            if (signal != null)
            {
                signal.Description = $"Values: {values}";
            }
        }
    }

    private double ParseDouble(string value)
    {
        if (double.TryParse(value.Trim(), out var result))
            return result;
        return 0.0;
    }
}

public class DbcParseResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Version { get; set; }
    public List<CanSpecMessage> Messages { get; set; } = new();

    public int MessageCount => Messages.Count;
    public int SignalCount => Messages.Sum(m => m.Signals.Count);
}
