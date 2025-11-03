using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Volo.Abp.DependencyInjection;

namespace AnomalyDetection.Security;

/// <summary>
/// Security scanner implementation for vulnerability detection
/// </summary>
public class SecurityScanner : ISecurityScanner, ITransientDependency
{
    private readonly IConfiguration _configuration;
    private readonly IEncryptionService _encryptionService;

    // Regex patterns for security validation
    private static readonly Regex SqlInjectionPattern = new Regex(
        @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|EXECUTE|UNION|DECLARE)\b)|(')|(--)|(;)|(/\*)|(\*/)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex XssPattern = new Regex(
        @"<script|javascript:|onerror|onload|<iframe|eval\(|expression\(|vbscript:",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex PathTraversalPattern = new Regex(
        @"\.\.|%2e%2e|\.%2e|%2e\.|\\|/",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public SecurityScanner(IConfiguration configuration, IEncryptionService encryptionService)
    {
        _configuration = configuration;
        _encryptionService = encryptionService;
    }

    public async Task<SecurityScanResult> ScanAsync()
    {
        var issues = new List<SecurityIssue>();

        // Scan configuration
        var configIssues = await CheckConfigurationAsync();
        issues.AddRange(configIssues);

        // Check encryption settings
        issues.AddRange(CheckEncryptionSettings());

        // Check authentication settings
        issues.AddRange(CheckAuthenticationSettings());

        // Check CORS settings
        issues.AddRange(CheckCorsSettings());

        // Calculate severity counts
        var result = new SecurityScanResult
        {
            ScanDate = DateTime.UtcNow,
            Issues = issues,
            TotalIssues = issues.Count,
            CriticalIssues = issues.Count(i => i.Severity == SecuritySeverity.Critical),
            HighIssues = issues.Count(i => i.Severity == SecuritySeverity.High),
            MediumIssues = issues.Count(i => i.Severity == SecuritySeverity.Medium),
            LowIssues = issues.Count(i => i.Severity == SecuritySeverity.Low)
        };

        // Calculate security score (100 - weighted issues)
        var score = 100
            - (result.CriticalIssues * 20)
            - (result.HighIssues * 10)
            - (result.MediumIssues * 5)
            - (result.LowIssues * 2);

        result.Score.OverallScore = Math.Max(0, score);

        return result;
    }

    public async Task<List<SecurityIssue>> CheckConfigurationAsync()
    {
        var issues = new List<SecurityIssue>();

        // Check encryption key
        var encryptionKey = _configuration["Security:EncryptionKey"];
        if (string.IsNullOrEmpty(encryptionKey) || encryptionKey == "AnomalyDetection2025SecureKey!")
        {
            issues.Add(new SecurityIssue
            {
                Category = "Encryption",
                Severity = SecuritySeverity.Critical,
                Title = "Weak or Default Encryption Key",
                Description = "Using default or weak encryption key for sensitive data protection",
                Recommendation = "Set a strong, unique encryption key in configuration (e.g., Azure Key Vault)",
                AffectedComponent = "Security:EncryptionKey"
            });
        }

        // Check HTTPS enforcement
        var requireHttps = _configuration.GetValue<bool>("Security:RequireHttps", false);
        if (!requireHttps)
        {
            issues.Add(new SecurityIssue
            {
                Category = "Transport Security",
                Severity = SecuritySeverity.High,
                Title = "HTTPS Not Enforced",
                Description = "Application does not enforce HTTPS connections",
                Recommendation = "Enable HTTPS redirection and set Security:RequireHttps to true",
                AffectedComponent = "Security:RequireHttps"
            });
        }

        // Check API rate limiting
        var rateLimitEnabled = _configuration.GetValue<bool>("Security:RateLimiting:Enabled", false);
        if (!rateLimitEnabled)
        {
            issues.Add(new SecurityIssue
            {
                Category = "API Security",
                Severity = SecuritySeverity.Medium,
                Title = "Rate Limiting Not Enabled",
                Description = "API endpoints are not protected by rate limiting",
                Recommendation = "Enable rate limiting to prevent abuse and DDoS attacks",
                AffectedComponent = "Security:RateLimiting"
            });
        }

        await Task.CompletedTask;
        return issues;
    }

    public bool ValidateInput(string input, InputValidationType validationType)
    {
        if (string.IsNullOrEmpty(input))
        {
            return true; // Empty input is safe
        }

        switch (validationType)
        {
            case InputValidationType.SqlInjection:
                return !SqlInjectionPattern.IsMatch(input);

            case InputValidationType.XssAttack:
                return !XssPattern.IsMatch(input);

            case InputValidationType.PathTraversal:
                return !PathTraversalPattern.IsMatch(input);

            case InputValidationType.CommandInjection:
                // Check for command injection patterns
                var commandPatterns = new[] { "|", "&", ";", "$", "`", "\n", "\r" };
                return !commandPatterns.Any(input.Contains);

            case InputValidationType.General:
                // Combined check
                return ValidateInput(input, InputValidationType.SqlInjection)
                    && ValidateInput(input, InputValidationType.XssAttack)
                    && ValidateInput(input, InputValidationType.PathTraversal)
                    && ValidateInput(input, InputValidationType.CommandInjection);

            default:
                return false;
        }
    }

    private List<SecurityIssue> CheckEncryptionSettings()
    {
        var issues = new List<SecurityIssue>();

        // Check if sensitive data fields are encrypted
        // This is a placeholder - actual implementation would check database configurations

        return issues;
    }

    private List<SecurityIssue> CheckAuthenticationSettings()
    {
        var issues = new List<SecurityIssue>();

        // Check password requirements
        var minPasswordLength = _configuration.GetValue<int>("Identity:Password:RequiredLength", 0);
        if (minPasswordLength < 8)
        {
            issues.Add(new SecurityIssue
            {
                Category = "Authentication",
                Severity = SecuritySeverity.High,
                Title = "Weak Password Requirements",
                Description = $"Minimum password length is {minPasswordLength}, should be at least 8 characters",
                Recommendation = "Set Identity:Password:RequiredLength to at least 8",
                AffectedComponent = "Identity:Password"
            });
        }

        // Check token expiration
        var tokenLifetime = _configuration.GetValue<int>("AuthServer:AccessTokenLifetime", 3600);
        if (tokenLifetime > 3600) // More than 1 hour
        {
            issues.Add(new SecurityIssue
            {
                Category = "Authentication",
                Severity = SecuritySeverity.Medium,
                Title = "Long Access Token Lifetime",
                Description = $"Access tokens are valid for {tokenLifetime} seconds",
                Recommendation = "Reduce token lifetime to maximum 3600 seconds (1 hour)",
                AffectedComponent = "AuthServer:AccessTokenLifetime"
            });
        }

        return issues;
    }

    private List<SecurityIssue> CheckCorsSettings()
    {
        var issues = new List<SecurityIssue>();

        var corsOrigins = _configuration["App:CorsOrigins"];
        if (!string.IsNullOrEmpty(corsOrigins) && corsOrigins.Contains("*"))
        {
            issues.Add(new SecurityIssue
            {
                Category = "Configuration",
                Severity = SecuritySeverity.High,
                Title = "Wildcard CORS Origin Allowed",
                Description = "CORS is configured to allow all origins (*)",
                Recommendation = "Specify explicit allowed origins instead of wildcard",
                AffectedComponent = "App:CorsOrigins"
            });
        }

        return issues;
    }
}
