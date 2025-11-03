using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace AnomalyDetection.Security;

public interface ISecurityAppService
{
    /// <summary>
    /// Run comprehensive security scan
    /// </summary>
    Task<SecurityScanResultDto> RunSecurityScanAsync();

    /// <summary>
    /// Generate new API key
    /// </summary>
    Task<ApiKeyDto> GenerateApiKeyAsync(CreateApiKeyDto input);

    /// <summary>
    /// Validate API key
    /// </summary>
    Task<ApiKeyValidationResultDto> ValidateApiKeyAsync(string apiKey);

    /// <summary>
    /// Revoke API key
    /// </summary>
    Task RevokeApiKeyAsync(string apiKey);

    /// <summary>
    /// Rotate API key
    /// </summary>
    Task<ApiKeyDto> RotateApiKeyAsync(string oldApiKey);
}

public class SecurityScanResultDto
{
    public System.DateTime ScanDate { get; set; }
    public int TotalIssues { get; set; }
    public int CriticalIssues { get; set; }
    public int HighIssues { get; set; }
    public int MediumIssues { get; set; }
    public int LowIssues { get; set; }
    public System.Collections.Generic.List<SecurityIssueDto> Issues { get; set; } = new();
    public int OverallScore { get; set; }
}

public class SecurityIssueDto
{
    public string Category { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public string? AffectedComponent { get; set; }
}

public class ApiKeyDto
{
    public string ApiKey { get; set; } = string.Empty;
    public string Identifier { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public System.DateTime CreatedAt { get; set; }
    public System.DateTime ExpiresAt { get; set; }
}

public class CreateApiKeyDto
{
    public string Identifier { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
}

public class ApiKeyValidationResultDto
{
    public bool IsValid { get; set; }
    public string? Identifier { get; set; }
    public string? Scope { get; set; }
    public System.DateTime? ExpiresAt { get; set; }
    public string? ErrorMessage { get; set; }
}
