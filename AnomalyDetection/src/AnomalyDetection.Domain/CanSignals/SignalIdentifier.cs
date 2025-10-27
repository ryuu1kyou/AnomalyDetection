using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Values;

namespace AnomalyDetection.CanSignals;

public class SignalIdentifier : ValueObject
{
    public string SignalName { get; private set; } = default!;
    public string CanId { get; private set; } = default!;

    protected SignalIdentifier() { }

    public SignalIdentifier(string signalName, string canId)
    {
        SignalName = ValidateSignalName(signalName);
        CanId = ValidateCanId(canId);
    }

    private static string ValidateSignalName(string signalName)
    {
        if (string.IsNullOrWhiteSpace(signalName))
            throw new ArgumentException("Signal name cannot be null or empty", nameof(signalName));
            
        if (signalName.Length > 100)
            throw new ArgumentException("Signal name cannot exceed 100 characters", nameof(signalName));
            
        // CAN信号名の命名規則チェック（英数字、アンダースコア、ハイフンのみ）
        if (!System.Text.RegularExpressions.Regex.IsMatch(signalName, @"^[a-zA-Z0-9_-]+$"))
            throw new ArgumentException("Signal name must contain only alphanumeric characters, underscores, and hyphens", nameof(signalName));
            
        return signalName.Trim();
    }

    private static string ValidateCanId(string canId)
    {
        if (string.IsNullOrWhiteSpace(canId))
            throw new ArgumentException("CAN ID cannot be null or empty", nameof(canId));
        
        // CAN IDの形式チェック（16進数、最大8桁）
        if (!System.Text.RegularExpressions.Regex.IsMatch(canId, @"^[0-9A-Fa-f]{1,8}$"))
            throw new ArgumentException("CAN ID must be a valid hexadecimal value (1-8 digits)", nameof(canId));
            
        return canId.ToUpperInvariant();
    }

    public bool IsStandardCanId()
    {
        // Standard CAN ID: 11-bit (0x000 - 0x7FF)
        var id = Convert.ToUInt32(CanId, 16);
        return id <= 0x7FF;
    }

    public bool IsExtendedCanId()
    {
        // Extended CAN ID: 29-bit (0x00000000 - 0x1FFFFFFF)
        var id = Convert.ToUInt32(CanId, 16);
        return id > 0x7FF && id <= 0x1FFFFFFF;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return SignalName;
        yield return CanId;
    }

    public override string ToString()
    {
        return $"{SignalName} (0x{CanId})";
    }
}