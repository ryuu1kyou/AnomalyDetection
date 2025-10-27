using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Values;

namespace AnomalyDetection.CanSignals;

public class SignalSpecification : ValueObject
{
    public int StartBit { get; private set; }
    public int Length { get; private set; }
    public SignalDataType DataType { get; private set; }
    public SignalValueRange ValueRange { get; private set; } = default!;
    public SignalByteOrder ByteOrder { get; private set; }

    protected SignalSpecification() { }

    public SignalSpecification(
        int startBit, 
        int length, 
        SignalDataType dataType, 
        SignalValueRange valueRange,
        SignalByteOrder byteOrder = SignalByteOrder.Motorola)
    {
        StartBit = ValidateStartBit(startBit);
        Length = ValidateLength(length);
        DataType = dataType;
        ValueRange = valueRange ?? throw new ArgumentNullException(nameof(valueRange));
        ByteOrder = byteOrder;
        
        ValidateSpecificationConsistency();
    }

    public bool IsCompatibleWith(SignalSpecification other)
    {
        if (other == null)
            return false;
            
        return StartBit == other.StartBit &&
               Length == other.Length &&
               DataType == other.DataType &&
               ByteOrder == other.ByteOrder;
    }

    public int GetEndBit()
    {
        return StartBit + Length - 1;
    }

    public bool OverlapsWith(SignalSpecification other)
    {
        if (other == null)
            return false;
            
        var thisEnd = GetEndBit();
        var otherEnd = other.GetEndBit();
        
        return !(thisEnd < other.StartBit || StartBit > otherEnd);
    }

    public double GetMaxRawValue()
    {
        return DataType switch
        {
            SignalDataType.Unsigned => Math.Pow(2, Length) - 1,
            SignalDataType.Signed => Math.Pow(2, Length - 1) - 1,
            SignalDataType.Boolean => 1,
            _ => double.MaxValue
        };
    }

    public double GetMinRawValue()
    {
        return DataType switch
        {
            SignalDataType.Unsigned => 0,
            SignalDataType.Signed => -Math.Pow(2, Length - 1),
            SignalDataType.Boolean => 0,
            _ => double.MinValue
        };
    }

    private static int ValidateStartBit(int startBit)
    {
        if (startBit < 0 || startBit > 63)
            throw new ArgumentOutOfRangeException(nameof(startBit), "Start bit must be between 0 and 63");
        return startBit;
    }

    private static int ValidateLength(int length)
    {
        if (length < 1 || length > 64)
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be between 1 and 64 bits");
        return length;
    }

    private void ValidateSpecificationConsistency()
    {
        // ビット範囲の妥当性チェック
        if (GetEndBit() > 63)
            throw new ArgumentException("Signal specification exceeds 64-bit boundary");
            
        // データ型とビット長の整合性チェック
        switch (DataType)
        {
            case SignalDataType.Boolean when Length != 1:
                throw new ArgumentException("Boolean signals must be exactly 1 bit");
            case SignalDataType.Float when Length != 32:
                throw new ArgumentException("Float signals must be exactly 32 bits");
            case SignalDataType.Double when Length != 64:
                throw new ArgumentException("Double signals must be exactly 64 bits");
        }
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return StartBit;
        yield return Length;
        yield return DataType;
        yield return ValueRange;
        yield return ByteOrder;
    }
}

public class SignalValueRange : ValueObject
{
    public double MinValue { get; private set; }
    public double MaxValue { get; private set; }

    protected SignalValueRange() { }

    public SignalValueRange(double minValue, double maxValue)
    {
        if (minValue > maxValue)
            throw new ArgumentException("Min value cannot be greater than max value");
            
        MinValue = minValue;
        MaxValue = maxValue;
    }

    public bool IsInRange(double value)
    {
        return value >= MinValue && value <= MaxValue;
    }

    public double GetRange()
    {
        return MaxValue - MinValue;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return MinValue;
        yield return MaxValue;
    }

    public override string ToString()
    {
        return $"[{MinValue}, {MaxValue}]";
    }
}