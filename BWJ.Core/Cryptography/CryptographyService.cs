using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BWJ.Core.Cryptography
{
    public class CryptographyService : ICryptographyService
    {
        private readonly ICryptographyServiceSettingsProvider _settingsProvider;

        public CryptographyService(
            ICryptographyServiceSettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;
        }

        public async Task<string> EncryptText(string text, string? keyName = default, string? IV = default)
        {
            byte[] result = Encoding.UTF8.GetBytes(text);
            result = await EncryptData(result, keyName, IV);

            return Convert.ToBase64String(result);
        }

        public async Task<string> DecryptText(string text, string? keyName = default, string? IV = default)
        {
            byte[] binaryData = Convert.FromBase64String(text);
            binaryData = await DecryptData(binaryData, keyName, IV);

            return Encoding.UTF8.GetString(binaryData);
        }

        public async Task<byte[]> EncryptData(byte[] data, string? keyName = default, string? IV = default)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = await _settingsProvider.GetEncryptionKey(keyName);
                aes.IV = await _settingsProvider.GetInitializationVector(IV);
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor())
                using (var encDataStream = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(encDataStream, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(data, 0, data.Length);
                    }

                    return encDataStream.ToArray();
                }
            }
        }

        public async Task<byte[]> DecryptData(byte[] data, string? keyName = default, string? IV = default)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = await _settingsProvider.GetEncryptionKey(keyName);
                aes.IV = await _settingsProvider.GetInitializationVector(IV);
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor())
                using (var decDataStream = new MemoryStream())
                {
                    using (var csDecrypt = new CryptoStream(decDataStream, decryptor, CryptoStreamMode.Write))
                    {
                        csDecrypt.Write(data, 0, data.Length);
                    }

                    return decDataStream.ToArray();
                }
            }
        }

        public byte[] HashData(byte[] data, byte[]? salt = null)
        {
            salt = salt ?? Array.Empty<byte>();
            var concat = new List<byte>();
            concat.AddRange(data);
            concat.AddRange(salt);
            data = concat.ToArray();

            using var hasher = SHA256.Create();
            return hasher.ComputeHash(data);
        }

        public string HashText(string text, string? salt = null)
        {
            var textBytes = Encoding.UTF8.GetBytes(text);
            var saltBytes = salt is not null ? Encoding.UTF8.GetBytes(salt) : null;

            var data = HashData(textBytes, saltBytes);
            return Convert.ToBase64String(data);
        }
    }
}
