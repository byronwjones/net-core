using System.Threading.Tasks;

namespace BWJ.Core.Cryptography
{
    public interface ICryptographyService
    {
        Task<byte[]> DecryptData(byte[] data, string? keyName = default, string? IV = default);
        Task<string> DecryptText(string text, string? keyName = default, string? IV = default);
        Task<byte[]> EncryptData(byte[] data, string? keyName = default, string? IV = default);
        Task<string> EncryptText(string text, string? keyName = default, string? IV = default);
        byte[] HashData(byte[] data, byte[]? salt = default);
        string HashText(string text, string? salt = default);
    }
}