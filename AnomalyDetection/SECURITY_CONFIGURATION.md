# Security Configuration Guide

## Overview

This guide explains how to configure the security features in AnomalyDetection application, including encryption, API key management, and vulnerability scanning.

## Configuration Sections

### 1. Encryption Settings

```json
"Security": {
  "EncryptionKey": "YOUR-256-BIT-ENCRYPTION-KEY-CHANGE-IN-PRODUCTION",
  "RequireHttps": true
}
```

**EncryptionKey**: Used for AES-256 encryption of sensitive data (API keys, webhook secrets, etc.)

- **Development**: Use a placeholder key for local testing
- **Production**: Generate a strong, unique key using the command below
- **Storage**: Store in Azure Key Vault or environment variable (never commit to source control)

**Generate Production Key (PowerShell)**:

```powershell
$bytes = New-Object byte[] 32
[Security.Cryptography.RNGCryptoServiceProvider]::Create().GetBytes($bytes)
[Convert]::ToBase64String($bytes)
```

**RequireHttps**: Enforces HTTPS for all API endpoints

- Set to `true` in production
- Set to `false` only in local development

### 2. Rate Limiting

```json
"RateLimiting": {
  "Enabled": true,
  "PermitLimit": 100,
  "Window": "00:01:00"
}
```

- **Enabled**: Enable/disable rate limiting
- **PermitLimit**: Maximum requests per time window
- **Window**: Time window duration (format: HH:MM:SS)

**Recommended Settings**:

- Public API: 100 requests/minute
- Admin API: 1000 requests/minute
- WebSocket: 10 connections/minute per IP

### 3. Authentication

```json
"Authentication": {
  "MinPasswordLength": 8,
  "RequireUppercase": true,
  "RequireDigit": true,
  "TokenLifetime": "01:00:00"
}
```

- **MinPasswordLength**: Minimum password length (8-32 characters)
- **RequireUppercase**: Require at least one uppercase letter
- **RequireDigit**: Require at least one digit
- **TokenLifetime**: JWT token expiration time

**OWASP Recommendations**:

- Minimum 8 characters
- Mix of uppercase, lowercase, digits, special chars
- Token lifetime: 15 minutes (critical systems), 1 hour (standard)

### 4. CORS Configuration

```json
"Cors": {
  "AllowedOrigins": [
    "https://*.AnomalyDetection.com",
    "http://localhost:4200"
  ]
}
```

- **AllowedOrigins**: Whitelist of allowed origins for CORS
- Use specific domains in production (avoid wildcards)
- Include localhost for local development only

## Azure Key Vault Integration

### Step 1: Create Key Vault

```bash
az keyvault create \
  --name anomaly-detection-kv \
  --resource-group your-resource-group \
  --location japaneast
```

### Step 2: Store Encryption Key

```bash
# Generate and store encryption key
$encryptionKey = [Convert]::ToBase64String((New-Object byte[] 32))
az keyvault secret set \
  --vault-name anomaly-detection-kv \
  --name EncryptionKey \
  --value $encryptionKey
```

### Step 3: Configure Application

Update `appsettings.Production.json`:

```json
{
  "Azure": {
    "KeyVault": {
      "VaultUri": "https://anomaly-detection-kv.vault.azure.net/"
    }
  }
}
```

Update `Program.cs` to load from Key Vault:

```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri(builder.Configuration["Azure:KeyVault:VaultUri"]),
    new DefaultAzureCredential());
```

## Security Scanning

### Manual Scan

```bash
curl -X POST https://localhost:44318/api/security/scan \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

### Scheduled Scan

Configure background job in `AnomalyDetectionApplicationModule.cs`:

```csharp
context.Services.AddBackgroundJob<SecurityScanJob>(options =>
{
    options.CronExpression = "0 2 * * *"; // Daily at 2 AM
});
```

### Scan Result Interpretation

**Security Score**:

- 90-100: Excellent (production ready)
- 70-89: Good (minor issues)
- 50-69: Fair (attention needed)
- Below 50: Poor (critical issues)

**Issue Severity**:

- **Critical**: Immediate action required (SQL injection, missing encryption)
- **High**: Fix within 7 days (weak passwords, insecure CORS)
- **Medium**: Fix within 30 days (missing rate limiting)
- **Low**: Best practice recommendation (token lifetime optimization)

## API Key Management

### Generate API Key

```bash
curl -X POST https://localhost:44318/api/security/api-keys \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "identifier": "external-integration-system",
    "scope": "integration:read,integration:write"
  }'
```

Response:

```json
{
  "apiKey": "ak_7x9k2m4n6p8q1r3s5t7v9w1x3y5z7a9c",
  "identifier": "external-integration-system",
  "scope": "integration:read,integration:write",
  "createdAt": "2024-01-15T10:30:00Z",
  "expiresAt": "2025-01-15T10:30:00Z"
}
```

### Validate API Key

```bash
curl -X POST https://localhost:44318/api/security/api-keys/validate \
  -H "Content-Type: application/json" \
  -d '{
    "apiKey": "ak_7x9k2m4n6p8q1r3s5t7v9w1x3y5z7a9c"
  }'
```

### Rotate API Key

```bash
curl -X POST https://localhost:44318/api/security/api-keys/ak_OLD_KEY/rotate \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

**Rotation Policy**:

- Automatic rotation: Every 90 days
- Grace period: 30 days (both old and new keys work)
- Manual rotation: Any time via API

### Revoke API Key

```bash
curl -X DELETE https://localhost:44318/api/security/api-keys/ak_KEY_TO_REVOKE \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

## Environment-Specific Configuration

### Development (appsettings.Development.json)

```json
{
  "Security": {
    "EncryptionKey": "DEV-KEY-ONLY-FOR-LOCAL-TESTING-123",
    "RequireHttps": false,
    "RateLimiting": {
      "Enabled": false
    }
  }
}
```

### Staging (appsettings.Staging.json)

```json
{
  "Security": {
    "EncryptionKey": "${ENCRYPTION_KEY}",
    "RequireHttps": true,
    "RateLimiting": {
      "Enabled": true,
      "PermitLimit": 500
    }
  }
}
```

### Production (appsettings.Production.json)

```json
{
  "Security": {
    "EncryptionKey": "${ENCRYPTION_KEY}",
    "RequireHttps": true,
    "RateLimiting": {
      "Enabled": true,
      "PermitLimit": 100
    }
  }
}
```

## Security Best Practices

### 1. Encryption Key Rotation

- Rotate encryption keys annually
- Implement key versioning for backward compatibility
- Use Azure Key Vault automatic rotation

### 2. API Key Security

- Store API keys hashed (SHA-256)
- Never log API keys in plain text
- Implement IP whitelisting for critical keys
- Monitor API key usage patterns

### 3. Input Validation

- Validate all user inputs using `ISecurityScanner.ValidateInput()`
- Sanitize HTML inputs to prevent XSS
- Use parameterized queries to prevent SQL injection
- Validate file paths to prevent directory traversal

### 4. HTTPS Enforcement

- Use HTTPS for all production endpoints
- Implement HSTS (HTTP Strict Transport Security)
- Use TLS 1.2 or higher

### 5. Monitoring and Alerting

- Enable security scan logging
- Set up alerts for Critical/High severity issues
- Monitor API key validation failures
- Track rate limiting violations

## Troubleshooting

### Issue: Encryption fails with "Invalid key length"

**Solution**: Ensure EncryptionKey is at least 16 characters. Generate a proper key using the PowerShell command above.

### Issue: Security scan shows "Missing EncryptionKey"

**Solution**: Add `Security:EncryptionKey` to appsettings.json or Azure Key Vault.

### Issue: API key validation returns "Invalid"

**Solution**: Check that:

1. API key format is correct (starts with "ak\_")
2. Key has not expired (check ExpiresAt date)
3. Key has not been revoked (IsActive = true)

### Issue: CORS error in browser

**Solution**: Add the frontend origin to `Security:Cors:AllowedOrigins` in appsettings.json.

## Compliance Checklist

- [ ] Encryption key stored in Azure Key Vault (not in source control)
- [ ] HTTPS enforced in production (RequireHttps = true)
- [ ] Rate limiting enabled with appropriate limits
- [ ] Strong password policy configured (min 8 chars, uppercase, digit)
- [ ] CORS whitelist configured (no wildcards in production)
- [ ] Security scanning scheduled (daily)
- [ ] API keys rotated every 90 days
- [ ] Input validation implemented for all user inputs
- [ ] Security logs monitored and alerted
- [ ] Penetration testing completed before production deployment

## References

- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [Azure Key Vault Documentation](https://learn.microsoft.com/en-us/azure/key-vault/)
- [ABP Security Best Practices](https://docs.abp.io/en/abp/latest/Best-Practices/Security)
- [ASP.NET Core Security](https://learn.microsoft.com/en-us/aspnet/core/security/)
