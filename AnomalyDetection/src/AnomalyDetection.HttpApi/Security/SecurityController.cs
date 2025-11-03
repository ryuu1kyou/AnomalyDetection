using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace AnomalyDetection.Security;

[Route("api/security")]
[Authorize]
public class SecurityController : AbpControllerBase
{
    private readonly ISecurityAppService _securityAppService;

    public SecurityController(ISecurityAppService securityAppService)
    {
        _securityAppService = securityAppService;
    }

    /// <summary>
    /// Run comprehensive security scan
    /// </summary>
    [HttpPost("scan")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<SecurityScanResultDto> ScanAsync()
    {
        return await _securityAppService.RunSecurityScanAsync();
    }

    /// <summary>
    /// Generate new API key
    /// </summary>
    [HttpPost("api-keys")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ApiKeyDto> GenerateApiKeyAsync([FromBody] CreateApiKeyDto input)
    {
        return await _securityAppService.GenerateApiKeyAsync(input);
    }

    /// <summary>
    /// Validate API key
    /// </summary>
    [HttpPost("api-keys/validate")]
    public async Task<ApiKeyValidationResultDto> ValidateApiKeyAsync([FromBody] ValidateApiKeyRequest request)
    {
        return await _securityAppService.ValidateApiKeyAsync(request.ApiKey);
    }

    /// <summary>
    /// Revoke API key
    /// </summary>
    [HttpDelete("api-keys/{apiKey}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task RevokeApiKeyAsync(string apiKey)
    {
        await _securityAppService.RevokeApiKeyAsync(apiKey);
    }

    /// <summary>
    /// Rotate API key
    /// </summary>
    [HttpPost("api-keys/{apiKey}/rotate")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ApiKeyDto> RotateApiKeyAsync(string apiKey)
    {
        return await _securityAppService.RotateApiKeyAsync(apiKey);
    }
}

public class ValidateApiKeyRequest
{
    public string ApiKey { get; set; } = string.Empty;
}
