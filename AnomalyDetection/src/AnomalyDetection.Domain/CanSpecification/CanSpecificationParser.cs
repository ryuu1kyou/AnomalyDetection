using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace AnomalyDetection.CanSpecification;

public interface ICanSpecificationParser : IDomainService
{
    ParseResult Parse(byte[] content, string format);
}

public class CanSpecificationParser : DomainService, ICanSpecificationParser
{
    public ParseResult Parse(byte[] content, string format)
    {
        var result = new ParseResult();

        if (string.Equals(format, "CSV", StringComparison.OrdinalIgnoreCase))
        {
            ParseCsv(content, result);
        }
        else if (string.Equals(format, "JSON", StringComparison.OrdinalIgnoreCase))
        {
            ParseJson(content, result);
        }
        else
        {
            throw new BusinessException($"Unsupported format: {format}. Supported formats are CSV and JSON.");
        }

        return result;
    }

    private void ParseCsv(byte[] content, ParseResult result)
    {
        using var ms = new MemoryStream(content);
        using var reader = new StreamReader(ms, Encoding.UTF8);

        string? line;
        var messages = new Dictionary<uint, CanSpecMessage>();
        bool isFirstLine = true;

        // Expected CSV format:
        // MessageId,SignalName,StartBit,BitLength,IsSigned,IsBigEndian,Min,Max,Factor,Offset,Unit

        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Skip header line
            if (isFirstLine)
            {
                isFirstLine = false;
                if (line.Contains("MessageId") && line.Contains("SignalName"))
                {
                    continue; // This is the header
                }
            }

            var parts = line.Split(',');
            if (parts.Length < 11) continue; // Need at least 11 columns

            try
            {
                // Parse MessageId (can be hex like 0x100 or decimal like 256)
                var messageIdStr = parts[0].Trim();
                uint messageId;
                if (messageIdStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    messageId = Convert.ToUInt32(messageIdStr, 16);
                }
                else
                {
                    if (!uint.TryParse(messageIdStr, out messageId))
                    {
                        continue; // Skip invalid lines
                    }
                }

                var signalName = parts[1].Trim();
                var startBit = int.Parse(parts[2].Trim());
                var bitLength = int.Parse(parts[3].Trim());
                var isSigned = bool.Parse(parts[4].Trim());
                var isBigEndian = bool.Parse(parts[5].Trim());
                var min = double.Parse(parts[6].Trim(), CultureInfo.InvariantCulture);
                var max = double.Parse(parts[7].Trim(), CultureInfo.InvariantCulture);
                var factor = double.Parse(parts[8].Trim(), CultureInfo.InvariantCulture);
                var offset = double.Parse(parts[9].Trim(), CultureInfo.InvariantCulture);
                var unit = parts[10].Trim();

                // Get or create message
                if (!messages.TryGetValue(messageId, out var message))
                {
                    // Use MessageId as hex string for name
                    var messageName = $"MSG_0x{messageId:X}";
                    message = new CanSpecMessage(messageId, messageName, 8); // Default DLC
                    messages[messageId] = message;
                }

                // Create signal with all properties
                var signal = new CanSpecSignal(signalName, startBit, bitLength)
                {
                    IsBigEndian = isBigEndian,
                    IsSigned = isSigned,
                    Factor = factor,
                    Offset = offset,
                    Min = min,
                    Max = max,
                    Unit = unit
                };

                message.Signals.Add(signal);
            }
            catch
            {
                // Skip malformed lines
                continue;
            }
        }

        result.Messages.AddRange(messages.Values);
    }

    private void ParseJson(byte[] content, ParseResult result)
    {
        var json = Encoding.UTF8.GetString(content);

        try
        {
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("messages", out var messagesArray))
            {
                throw new BusinessException("JSON must contain a 'messages' array");
            }

            foreach (var msgElement in messagesArray.EnumerateArray())
            {
                if (!msgElement.TryGetProperty("messageId", out var messageIdProp))
                {
                    continue;
                }

                var messageId = (uint)messageIdProp.GetInt32();
                var messageName = msgElement.TryGetProperty("messageName", out var nameProp)
                    ? nameProp.GetString() ?? $"MSG_0x{messageId:X}"
                    : $"MSG_0x{messageId:X}";

                var message = new CanSpecMessage(messageId, messageName, 8);

                if (msgElement.TryGetProperty("signals", out var signalsArray))
                {
                    foreach (var sigElement in signalsArray.EnumerateArray())
                    {
                        var name = sigElement.GetProperty("name").GetString() ?? "";
                        var startBit = sigElement.GetProperty("startBit").GetInt32();
                        var bitLength = sigElement.GetProperty("bitLength").GetInt32();

                        var signal = new CanSpecSignal(name, startBit, bitLength)
                        {
                            IsSigned = sigElement.TryGetProperty("isSigned", out var signedProp) && signedProp.GetBoolean(),
                            IsBigEndian = sigElement.TryGetProperty("isBigEndian", out var endianProp) && endianProp.GetBoolean(),
                            Min = sigElement.TryGetProperty("min", out var minProp) ? minProp.GetDouble() : 0,
                            Max = sigElement.TryGetProperty("max", out var maxProp) ? maxProp.GetDouble() : 0,
                            Factor = sigElement.TryGetProperty("factor", out var factorProp) ? factorProp.GetDouble() : 1,
                            Offset = sigElement.TryGetProperty("offset", out var offsetProp) ? offsetProp.GetDouble() : 0,
                            Unit = sigElement.TryGetProperty("unit", out var unitProp) ? unitProp.GetString() : ""
                        };

                        message.Signals.Add(signal);
                    }
                }

                result.Messages.Add(message);
            }
        }
        catch (JsonException ex)
        {
            throw new BusinessException("Invalid JSON format: " + ex.Message);
        }
    }
}

public class ParseResult
{
    public List<CanSpecMessage> Messages { get; } = new();
}
