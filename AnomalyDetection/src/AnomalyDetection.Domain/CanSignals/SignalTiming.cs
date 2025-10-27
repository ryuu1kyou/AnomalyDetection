using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Values;

namespace AnomalyDetection.CanSignals;

public class SignalTiming : ValueObject
{
    public int CycleTimeMs { get; private set; }
    public int TimeoutMs { get; private set; }
    public SignalSendType SendType { get; private set; }

    protected SignalTiming() { }

    public SignalTiming(int cycleTimeMs, int timeoutMs, SignalSendType sendType = SignalSendType.Cyclic)
    {
        CycleTimeMs = ValidateCycleTime(cycleTimeMs);
        TimeoutMs = ValidateTimeout(timeoutMs);
        SendType = sendType;
        
        ValidateTimingConsistency();
    }

    public bool IsTimeoutExpired(DateTime lastReceived)
    {
        if (SendType == SignalSendType.OnChange)
            return false; // On-change signals don't have timeout
            
        var elapsed = DateTime.UtcNow - lastReceived;
        return elapsed.TotalMilliseconds > TimeoutMs;
    }

    public DateTime GetNextExpectedTime(DateTime lastReceived)
    {
        return SendType switch
        {
            SignalSendType.Cyclic => lastReceived.AddMilliseconds(CycleTimeMs),
            SignalSendType.OnChange => DateTime.MaxValue, // No expected time for on-change
            _ => lastReceived.AddMilliseconds(CycleTimeMs)
        };
    }

    public double GetFrequencyHz()
    {
        return SendType == SignalSendType.Cyclic && CycleTimeMs > 0 
            ? 1000.0 / CycleTimeMs 
            : 0;
    }

    public bool IsHighFrequency()
    {
        return GetFrequencyHz() > 100; // > 100Hz
    }

    private static int ValidateCycleTime(int cycleTimeMs)
    {
        if (cycleTimeMs < 0)
            throw new ArgumentOutOfRangeException(nameof(cycleTimeMs), "Cycle time cannot be negative");
            
        if (cycleTimeMs > 3600000) // 1 hour max
            throw new ArgumentOutOfRangeException(nameof(cycleTimeMs), "Cycle time cannot exceed 1 hour");
            
        return cycleTimeMs;
    }

    private static int ValidateTimeout(int timeoutMs)
    {
        if (timeoutMs < 0)
            throw new ArgumentOutOfRangeException(nameof(timeoutMs), "Timeout cannot be negative");
            
        if (timeoutMs > 3600000) // 1 hour max
            throw new ArgumentOutOfRangeException(nameof(timeoutMs), "Timeout cannot exceed 1 hour");
            
        return timeoutMs;
    }

    private void ValidateTimingConsistency()
    {
        if (SendType == SignalSendType.Cyclic && CycleTimeMs == 0)
            throw new ArgumentException("Cyclic signals must have a positive cycle time");
            
        if (SendType == SignalSendType.Cyclic && TimeoutMs > 0 && TimeoutMs <= CycleTimeMs)
            throw new ArgumentException("Timeout must be greater than cycle time for cyclic signals");
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return CycleTimeMs;
        yield return TimeoutMs;
        yield return SendType;
    }

    public override string ToString()
    {
        return SendType switch
        {
            SignalSendType.Cyclic => $"Cyclic: {CycleTimeMs}ms (timeout: {TimeoutMs}ms)",
            SignalSendType.OnChange => "On Change",
            _ => $"Unknown: {CycleTimeMs}ms"
        };
    }
}

public enum SignalSendType
{
    Cyclic = 0,
    OnChange = 1,
    OnRequest = 2
}