using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace AnomalyDetection.Security;

/// <summary>
/// In-memory API key manager with encryption support
/// For production, use distributed cache (Redis) or database
/// </summary>
public class ApiKeyManager : IApiKeyManager, ISingletonDependency
{
    private readonly IEncryptionService _encryptionService;
    private readonly ConcurrentDictionary<string, ApiKeyData> _apiKeys;

    public ApiKeyManager(IEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
        _apiKeys = new ConcurrentDictionary<string, ApiKeyData>();
    }

    public async Task<string> GenerateApiKeyAsync(string identifier, string scope)
    {
        var apiKey = $"ak_{_encryptionService.GenerateSecureToken(32)}";
        var hashedKey = _encryptionService.HashData(apiKey);

        var keyData = new ApiKeyData
        {
            HashedKey = hashedKey,
            Identifier = identifier,
            Scope = scope,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddYears(1), // 1 year expiration
            IsActive = true
        };

        _apiKeys.TryAdd(hashedKey, keyData);

        await Task.CompletedTask;
        return apiKey;
    }

    public async Task<ApiKeyValidationResult> ValidateApiKeyAsync(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            return new ApiKeyValidationResult
            {
                IsValid = false,
                ErrorMessage = "API key is required"
            };
        }

        var hashedKey = _encryptionService.HashData(apiKey);

        if (!_apiKeys.TryGetValue(hashedKey, out var keyData))
        {
            return new ApiKeyValidationResult
            {
                IsValid = false,
                ErrorMessage = "Invalid API key"
            };
        }

        if (!keyData.IsActive)
        {
            return new ApiKeyValidationResult
            {
                IsValid = false,
                ErrorMessage = "API key has been revoked"
            };
        }

        if (keyData.ExpiresAt < DateTime.UtcNow)
        {
            return new ApiKeyValidationResult
            {
                IsValid = false,
                ErrorMessage = "API key has expired"
            };
        }

        // Update last used
        keyData.LastUsedAt = DateTime.UtcNow;

        await Task.CompletedTask;
        return new ApiKeyValidationResult
        {
            IsValid = true,
            Identifier = keyData.Identifier,
            Scope = keyData.Scope,
            ExpiresAt = keyData.ExpiresAt
        };
    }

    public async Task RevokeApiKeyAsync(string apiKey)
    {
        var hashedKey = _encryptionService.HashData(apiKey);

        if (_apiKeys.TryGetValue(hashedKey, out var keyData))
        {
            keyData.IsActive = false;
            keyData.RevokedAt = DateTime.UtcNow;
        }

        await Task.CompletedTask;
    }

    public async Task<string> RotateApiKeyAsync(string oldApiKey)
    {
        var validationResult = await ValidateApiKeyAsync(oldApiKey);

        if (!validationResult.IsValid || validationResult.Identifier == null || validationResult.Scope == null)
        {
            throw new InvalidOperationException("Cannot rotate invalid or expired API key");
        }

        // Generate new key with same identifier and scope
        var newApiKey = await GenerateApiKeyAsync(validationResult.Identifier, validationResult.Scope);

        // Mark old key as expiring in 30 days (grace period)
        var hashedOldKey = _encryptionService.HashData(oldApiKey);
        if (_apiKeys.TryGetValue(hashedOldKey, out var oldKeyData))
        {
            oldKeyData.ExpiresAt = DateTime.UtcNow.AddDays(30);
        }

        return newApiKey;
    }

    private class ApiKeyData
    {
        public string HashedKey { get; set; } = string.Empty;
        public string Identifier { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime? RevokedAt { get; set; }
    }
}
