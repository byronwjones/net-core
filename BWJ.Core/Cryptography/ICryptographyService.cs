using System.Threading.Tasks;

namespace BWJ.Core.Cryptography
{
    public interface ICryptographyService
    {
        Task<byte[]> DecryptData(byte[] data, string? IV = null);
        Task<string> DecryptText(string text, string? IV = null);
        Task<byte[]> EncryptData(byte[] data, string? IV = null);
        Task<string> EncryptText(string text, string? IV = null);
        byte[] HashData(byte[] data, byte[]? salt = null);
        string HashText(string text, string? salt = null);
    }
}