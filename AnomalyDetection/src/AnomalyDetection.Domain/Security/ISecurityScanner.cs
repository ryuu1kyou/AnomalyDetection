using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnomalyDetection.Security;

/// <summary>
/// Service for scanning security vulnerabilities and configuration issues
/// </summary>
public interface ISecurityScanner
{
    /// <summary>
    /// Scan system for security vulnerabilities
    /// </summary>
    Task<SecurityScanResult> ScanAsync();

    /// <summary>
    /// Check specific configuration for security issues
    /// </summary>
    Task<List<SecurityIssue>> CheckConfigurationAsync();

    /// <summary>
    /// Validate input data for SQL injection, XSS, etc.
    /// </summary>
    bool ValidateInput(string input, InputValidationType validationType);
}

public enum InputValidationType
{
    SqlInjection,
    XssAttack,
    PathTraversal,
    CommandInjection,
    General
}

public enum SecuritySeverity
{
    Critical,
    High,
    Medium,
    Low
}

public class SecurityScanResult
{
    public DateTime ScanDate { get; set; }
    public int TotalIssues { get; set; }
    public int CriticalIssues { get; set; }
    public int HighIssues { get; set; }
    public int MediumIssues { get; set; }
    public int LowIssues { get; set; }
    public List<SecurityIssue> Issues { get; set; } = new();
    public SecurityScore Score { get; set; } = new();
}

public class SecurityIssue
{
    public string Category { get; set; } = string.Empty; // Authentication, Authorization, Encryption, etc.
    public SecuritySeverity Severity { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public string? AffectedComponent { get; set; }
}

public class SecurityScore
{
    public int OverallScore { get; set; } // 0-100
    public Dictionary<string, int> CategoryScores { get; set; } = new();
}
