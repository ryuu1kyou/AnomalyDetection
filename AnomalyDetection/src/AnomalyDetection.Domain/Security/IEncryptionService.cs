using System.Threading.Tasks;

namespace AnomalyDetection.Security;

/// <summary>
/// Service for encrypting and decrypting sensitive data
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypt sensitive data (e.g., API keys, passwords)
    /// </summary>
    Task<string> EncryptAsync(string plainText);

    /// <summary>
    /// Decrypt encrypted data
    /// </summary>
    Task<string> DecryptAsync(string cipherText);

    /// <summary>
    /// Hash data for one-way encryption (e.g., webhook secrets)
    /// </summary>
    string HashData(string data);

    /// <summary>
    /// Verify hashed data
    /// </summary>
    bool VerifyHash(string data, string hash);

    /// <summary>
    /// Generate secure random token
    /// </summary>
    string GenerateSecureToken(int length = 32);
}
