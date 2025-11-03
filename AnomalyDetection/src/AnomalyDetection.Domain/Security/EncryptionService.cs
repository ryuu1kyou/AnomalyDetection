using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Volo.Abp.DependencyInjection;

namespace AnomalyDetection.Security;

/// <summary>
/// AES-256 encryption service for sensitive data protection
/// </summary>
public class EncryptionService : IEncryptionService, ITransientDependency
{
    private readonly string _encryptionKey;
    private readonly byte[] _keyBytes;

    public EncryptionService(IConfiguration configuration)
    {
        // Load encryption key from configuration
        // In production, use Azure Key Vault or similar secure storage
        _encryptionKey = configuration["Security:EncryptionKey"]
            ?? "AnomalyDetection2025SecureKey!"; // Default for development only

        // Generate 256-bit key from string
        using var sha256 = SHA256.Create();
        _keyBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(_encryptionKey));
    }

    public async Task<string> EncryptAsync(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return string.Empty;
        }

        try
        {
            using var aes = Aes.Create();
            aes.Key = _keyBytes;
            aes.GenerateIV();
            var iv = aes.IV;

            using var encryptor = aes.CreateEncryptor(aes.Key, iv);
            using var ms = new MemoryStream();

            // Write IV to the beginning of the stream
            await ms.WriteAsync(iv, 0, iv.Length);

            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                await sw.WriteAsync(plainText);
            }

            var encrypted = ms.ToArray();
            return Convert.ToBase64String(encrypted);
        }
        catch (Exception ex)
        {
            throw new Exception("Encryption failed", ex);
        }
    }

    public async Task<string> DecryptAsync(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            return string.Empty;
        }

        try
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = _keyBytes;

            // Extract IV from the beginning
            var iv = new byte[aes.IV.Length];
            Array.Copy(fullCipher, 0, iv, 0, iv.Length);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return await sr.ReadToEndAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Decryption failed", ex);
        }
    }

    public string HashData(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            return string.Empty;
        }

        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(data);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public bool VerifyHash(string data, string hash)
    {
        if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(hash))
        {
            return false;
        }

        var dataHash = HashData(data);
        return string.Equals(dataHash, hash, StringComparison.Ordinal);
    }

    public string GenerateSecureToken(int length = 32)
    {
        var bytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);

        // Convert to URL-safe base64
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}
