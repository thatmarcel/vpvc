using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace VPVC.Helpers;

public class EncryptionHelper {
    public static byte[] Encrypt(byte[] data, byte[] key) {
        using var aes = Aes.Create();
        aes.Key = key;

        using var memoryStream = new MemoryStream();
        
        memoryStream.Write(aes.IV, 0, aes.IV.Length);

        using var cryptoStream = new CryptoStream(
            memoryStream,
            aes.CreateEncryptor(),
            CryptoStreamMode.Write
        );
            
        using var binaryWriter = new BinaryWriter(cryptoStream);
            
        binaryWriter.Write(data);
        
        cryptoStream.FlushFinalBlock();

        return memoryStream.ToArray();
    }

    public static byte[] Decrypt(byte[] data, byte[] key) {
        using var aes = Aes.Create();
        aes.Key = key;

        aes.IV = data.Take(aes.IV.Length).ToArray();
        
        using var encryptedMemoryStream = new MemoryStream(data.Skip(aes.IV.Length).ToArray());
        
        using var cryptoStream = new CryptoStream(
            encryptedMemoryStream,
            aes.CreateDecryptor(),
            CryptoStreamMode.Read
        );
        
        using var plainMemoryStream = new MemoryStream();
        cryptoStream.CopyTo(plainMemoryStream);
        
        return plainMemoryStream.ToArray();
    }
}