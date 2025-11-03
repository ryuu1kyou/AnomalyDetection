using System;
using System.Threading.Tasks;

namespace AnomalyDetection.Security;

/// <summary>
/// Service for managing API keys and authentication tokens securely
/// </summary>
public interface IApiKeyManager
{
    /// <summary>
    /// Generate new API key for tenant or user
    /// </summary>
    Task<string> GenerateApiKeyAsync(string identifier, string scope);

    /// <summary>
    /// Validate API key and get associated metadata
    /// </summary>
    Task<ApiKeyValidationResult> ValidateApiKeyAsync(string apiKey);

    /// <summary>
    /// Revoke API key
    /// </summary>
    Task RevokeApiKeyAsync(string apiKey);

    /// <summary>
    /// Rotate API key (generate new, mark old as expiring)
    /// </summary>
    Task<string> RotateApiKeyAsync(string oldApiKey);
}

public class ApiKeyValidationResult
{
    public bool IsValid { get; set; }
    public string? Identifier { get; set; }
    public string? Scope { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? ErrorMessage { get; set; }
}
