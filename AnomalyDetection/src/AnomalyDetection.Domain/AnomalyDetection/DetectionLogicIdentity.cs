using System;
using System.Collections.Generic;
using AnomalyDetection.MultiTenancy;
using Volo.Abp.Domain.Values;

namespace AnomalyDetection.AnomalyDetection;

public class DetectionLogicIdentity : ValueObject
{
    public string Name { get; private set; } = default!;
    public LogicVersion Version { get; private set; } = default!;
    public OemCode OemCode { get; private set; } = default!;

    protected DetectionLogicIdentity() { }

    public DetectionLogicIdentity(string name, LogicVersion version, OemCode oemCode)
    {
        Name = ValidateName(name);
        Version = version ?? throw new ArgumentNullException(nameof(version));
        OemCode = oemCode ?? throw new ArgumentNullException(nameof(oemCode));
    }

    public string GetFullName()
    {
        return $"{OemCode.Code}_{Name}_v{Version}";
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Detection logic name cannot be null or empty", nameof(name));
            
        if (name.Length > 100)
            throw new ArgumentException("Detection logic name cannot exceed 100 characters", nameof(name));
            
        // 命名規則チェック（英数字、アンダースコア、ハイフンのみ）
        if (!System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z0-9_-]+$"))
            throw new ArgumentException("Detection logic name must contain only alphanumeric characters, underscores, and hyphens", nameof(name));
            
        return name.Trim();
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Name;
        yield return Version;
        yield return OemCode;
    }

    public override string ToString()
    {
        return GetFullName();
    }
}

public class LogicVersion : ValueObject
{
    public int Major { get; private set; }
    public int Minor { get; private set; }
    public int Patch { get; private set; }

    protected LogicVersion() { }

    public LogicVersion(int major, int minor, int patch = 0)
    {
        Major = ValidateVersionNumber(major, nameof(major));
        Minor = ValidateVersionNumber(minor, nameof(minor));
        Patch = ValidateVersionNumber(patch, nameof(patch));
    }

    public static LogicVersion Initial()
    {
        return new LogicVersion(1, 0, 0);
    }

    public LogicVersion IncrementPatch()
    {
        return new LogicVersion(Major, Minor, Patch + 1);
    }

    public LogicVersion IncrementMinor()
    {
        return new LogicVersion(Major, Minor + 1, 0);
    }

    public LogicVersion IncrementMajor()
    {
        return new LogicVersion(Major + 1, 0, 0);
    }

    public bool IsNewerThan(LogicVersion other)
    {
        if (other == null)
            return true;
            
        if (Major != other.Major)
            return Major > other.Major;
            
        if (Minor != other.Minor)
            return Minor > other.Minor;
            
        return Patch > other.Patch;
    }

    private static int ValidateVersionNumber(int version, string paramName)
    {
        if (version < 0)
            throw new ArgumentOutOfRangeException(paramName, "Version number cannot be negative");
            
        return version;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Major;
        yield return Minor;
        yield return Patch;
    }

    public override string ToString()
    {
        return $"{Major}.{Minor}.{Patch}";
    }
}