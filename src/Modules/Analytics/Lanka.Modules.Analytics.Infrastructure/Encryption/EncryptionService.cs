using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace Lanka.Modules.Analytics.Infrastructure.Encryption;

public sealed class EncryptionService
{
    private readonly byte[] _masterKey;
    private const int IvSize = 16;

    public EncryptionService(IOptions<EncryptionOptions> options)
    {
        this._masterKey = Convert.FromBase64String(options.Value.Key);
    }

    public string Encrypt(string plainText)
    {
        try
        {
            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = this._masterKey;
            aes.IV = RandomNumberGenerator.GetBytes(IvSize);
            using var memoryStream = new MemoryStream();
            memoryStream.Write(aes.IV, 0, IvSize);
#pragma warning disable CA5401
            using (ICryptoTransform encryptor = aes.CreateEncryptor())
#pragma warning restore CA5401
            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            using (var streamWriter = new StreamWriter(cryptoStream))
            {
                streamWriter.Write(plainText);
            }

            return Convert.ToBase64String(memoryStream.ToArray());
        }
        catch (CryptographicException exception)
        {
            throw new InvalidOperationException("Encryption failed", exception);
        }
    }

    public string Decrypt(string cipherText)
    {
        try
        {
            byte[] cipherData = Convert.FromBase64String(cipherText);

            if (cipherData.Length < IvSize)
            {
                throw new InvalidOperationException("Invalid cipher text format.");
            }

            byte[] iv = new byte[IvSize];
            byte[] encryptedData = new byte[cipherData.Length - IvSize];

            Buffer.BlockCopy(cipherData, 0, iv, 0, IvSize);
            Buffer.BlockCopy(cipherData, IvSize, encryptedData, 0, encryptedData.Length);

            using var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = this._masterKey;
            aes.IV = iv;

            using MemoryStream memoryStream = new(encryptedData);
            using ICryptoTransform decryptor = aes.CreateDecryptor();
            using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
            using StreamReader streamReader = new(cryptoStream);

            return streamReader.ReadToEnd();
        }
        catch (CryptographicException ex)
        {
            throw new InvalidOperationException("Decryption failed", ex);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException("Invalid cipher text format", ex);
        }
    }
}
