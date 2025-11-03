using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace AnomalyDetection.Security;

public class SecurityAppService : ApplicationService, ISecurityAppService
{
    private readonly ISecurityScanner _securityScanner;
    private readonly IApiKeyManager _apiKeyManager;

    public SecurityAppService(
        ISecurityScanner securityScanner,
        IApiKeyManager apiKeyManager)
    {
        _securityScanner = securityScanner;
        _apiKeyManager = apiKeyManager;
    }

    public async Task<SecurityScanResultDto> RunSecurityScanAsync()
    {
        var scanResult = await _securityScanner.ScanAsync();

        return new SecurityScanResultDto
        {
            ScanDate = DateTime.UtcNow,
            TotalIssues = scanResult.Issues.Count,
            CriticalIssues = scanResult.Issues.Count(i => i.Severity == SecuritySeverity.Critical),
            HighIssues = scanResult.Issues.Count(i => i.Severity == SecuritySeverity.High),
            MediumIssues = scanResult.Issues.Count(i => i.Severity == SecuritySeverity.Medium),
            LowIssues = scanResult.Issues.Count(i => i.Severity == SecuritySeverity.Low),
            Issues = scanResult.Issues.Select(issue => new SecurityIssueDto
            {
                Category = issue.Category,
                Severity = issue.Severity.ToString(),
                Title = issue.Title,
                Description = issue.Description,
                Recommendation = issue.Recommendation,
                AffectedComponent = issue.AffectedComponent
            }).ToList(),
            OverallScore = scanResult.Score.OverallScore
        };
    }

    public async Task<ApiKeyDto> GenerateApiKeyAsync(CreateApiKeyDto input)
    {
        var apiKey = await _apiKeyManager.GenerateApiKeyAsync(input.Identifier, input.Scope);

        return new ApiKeyDto
        {
            ApiKey = apiKey,
            Identifier = input.Identifier,
            Scope = input.Scope,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddYears(1)
        };
    }

    public async Task<ApiKeyValidationResultDto> ValidateApiKeyAsync(string apiKey)
    {
        var validationResult = await _apiKeyManager.ValidateApiKeyAsync(apiKey);

        return new ApiKeyValidationResultDto
        {
            IsValid = validationResult.IsValid,
            Identifier = validationResult.Identifier,
            Scope = validationResult.Scope,
            ExpiresAt = validationResult.ExpiresAt,
            ErrorMessage = validationResult.ErrorMessage
        };
    }

    public async Task RevokeApiKeyAsync(string apiKey)
    {
        await _apiKeyManager.RevokeApiKeyAsync(apiKey);
    }

    public async Task<ApiKeyDto> RotateApiKeyAsync(string oldApiKey)
    {
        var newApiKey = await _apiKeyManager.RotateApiKeyAsync(oldApiKey);

        // Get validation result to retrieve metadata
        var validationResult = await _apiKeyManager.ValidateApiKeyAsync(newApiKey);

        return new ApiKeyDto
        {
            ApiKey = newApiKey,
            Identifier = validationResult.Identifier ?? string.Empty,
            Scope = validationResult.Scope ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = validationResult.ExpiresAt ?? DateTime.UtcNow.AddYears(1)
        };
    }
}
