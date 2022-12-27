using System.Threading.Tasks;

namespace BWJ.Core.Cryptography
{
    public interface ICryptographyServiceSettingsProvider
    {
        Task<byte[]> GetInitializationVector(string? vector);
        Task<byte[]> GetEncryptionKey();
    }
}