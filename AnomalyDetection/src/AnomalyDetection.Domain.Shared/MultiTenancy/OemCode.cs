using System;
using System.Collections.Generic;
using AnomalyDetection.ValueObjects;

namespace AnomalyDetection.MultiTenancy;

public class OemCode : ValueObject
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;

    // Parameterless constructor for EF Core and serialization
    public OemCode() { }

    public OemCode(string code, string name)
    {
        Code = ValidateCode(code);
        Name = ValidateName(name);
    }

    private static string ValidateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("OEM code cannot be null or empty", nameof(code));
            
        if (code.Length < 2 || code.Length > 10)
            throw new ArgumentException("OEM code must be between 2 and 10 characters", nameof(code));
            
        if (!System.Text.RegularExpressions.Regex.IsMatch(code, @"^[A-Z0-9]+$"))
            throw new ArgumentException("OEM code must contain only uppercase letters and numbers", nameof(code));
            
        return code;
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("OEM name cannot be null or empty", nameof(name));
            
        if (name.Length > 100)
            throw new ArgumentException("OEM name cannot exceed 100 characters", nameof(name));
            
        return name.Trim();
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Code;
        yield return Name;
    }

    public override string ToString()
    {
        return $"{Code} - {Name}";
    }
}