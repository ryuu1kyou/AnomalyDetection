using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Values;

namespace AnomalyDetection.CanSignals;

public class PhysicalValueConversion : ValueObject
{
    public double Factor { get; private set; }
    public double Offset { get; private set; }
    public string Unit { get; private set; } = default!;

    protected PhysicalValueConversion() { }

    public PhysicalValueConversion(double factor, double offset, string unit)
    {
        Factor = ValidateFactor(factor);
        Offset = offset;
        Unit = ValidateUnit(unit);
    }

    public double ConvertToPhysical(double rawValue)
    {
        return rawValue * Factor + Offset;
    }

    public double ConvertToRaw(double physicalValue)
    {
        if (Math.Abs(Factor) < double.Epsilon)
            throw new InvalidOperationException("Cannot convert to raw value when factor is zero");
            
        return (physicalValue - Offset) / Factor;
    }

    public bool IsLinearConversion()
    {
        return Math.Abs(Factor - 1.0) > double.Epsilon || Math.Abs(Offset) > double.Epsilon;
    }

    public string GetConversionFormula()
    {
        if (!IsLinearConversion())
            return "Physical = Raw";
            
        var factorStr = Math.Abs(Factor - 1.0) < double.Epsilon ? "" : $"{Factor} * ";
        var offsetStr = Math.Abs(Offset) < double.Epsilon ? "" : 
                       Offset > 0 ? $" + {Offset}" : $" - {Math.Abs(Offset)}";
                       
        return $"Physical = {factorStr}Raw{offsetStr} [{Unit}]";
    }

    private static double ValidateFactor(double factor)
    {
        if (double.IsNaN(factor) || double.IsInfinity(factor))
            throw new ArgumentException("Factor cannot be NaN or infinity", nameof(factor));
            
        if (Math.Abs(factor) < double.Epsilon)
            throw new ArgumentException("Factor cannot be zero", nameof(factor));
            
        return factor;
    }

    private static string ValidateUnit(string unit)
    {
        if (unit == null)
            return string.Empty;
            
        if (unit.Length > 50)
            throw new ArgumentException("Unit cannot exceed 50 characters", nameof(unit));
            
        return unit.Trim();
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Factor;
        yield return Offset;
        yield return Unit ?? string.Empty;
    }

    public override string ToString()
    {
        return GetConversionFormula();
    }
}