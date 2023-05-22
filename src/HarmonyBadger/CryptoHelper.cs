using System.Security.Cryptography;

namespace HarmonyBadger;

/// <summary>
/// A collection of helpers to deal with cryptography.
/// </summary>
public class CryptoHelper
{
    private const int AesIVSizeInBytes = 16;

    /// <summary>
    /// Encrypts the given string using the AES265 algorithm.
    /// The ciphertext is returned base64 encoded, and is prefixed with the 16 byte IV.
    /// </summary>
    /// <param name="algo">The AES algorithm containing the key.</param>
    /// <param name="plainText">The text to encrypt.</param>
    /// <returns>
    /// Base64 encoded ciphertext, which can be decrypted using <see cref="DecryptDataAes256(Aes, string)"/>.
    /// </returns>
    public static string EncryptDataAes256(Aes algo, string plainText)
    {
        algo.GenerateIV();
        if (algo.IV.Length != AesIVSizeInBytes)
        {
            throw new InvalidOperationException("AES IV has unexpected length");
        }

        var encryptor = algo.CreateEncryptor(algo.Key, algo.IV);

        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        using var cryptoWriter = new StreamWriter(cryptoStream);

        cryptoWriter.Write(plainText);
        cryptoWriter.Flush();
        cryptoWriter.Close();

        byte[] encryptedBytes = memoryStream.ToArray();
        byte[] encryptedBytesWithIV = new byte[AesIVSizeInBytes + encryptedBytes.Length];
        Array.Copy(
            sourceArray: algo.IV,
            sourceIndex: 0,
            destinationArray: encryptedBytesWithIV,
            destinationIndex: 0,
            length: algo.IV.Length);
        Array.Copy(
            sourceArray: encryptedBytes,
            sourceIndex: 0,
            destinationArray: encryptedBytesWithIV,
            destinationIndex: algo.IV.Length,
            length: encryptedBytes.Length);

        return Convert.ToBase64String(encryptedBytesWithIV);
    }

    /// <summary>
    /// Decrypts the given ciphertext back to its original string. The ciphertext is assumed
    /// to be formatted as described by <see cref="EncryptDataAes256(Aes, string)"/>.
    /// </summary>
    /// <param name="algo">The AES algorithm containing the key.</param>
    /// <param name="cipherText">The ciphertext to decrypt.</param>
    /// <returns>The original plaintext message.</returns>
    public static string DecryptDataAes256(Aes algo, string cipherText)
    {
        byte[] encryptedBytesWithIV = Convert.FromBase64String(cipherText);
        byte[] iv = encryptedBytesWithIV[0..AesIVSizeInBytes];
        byte[] encryptedBytes = encryptedBytesWithIV[AesIVSizeInBytes..];

        ICryptoTransform decryptor = algo.CreateDecryptor(algo.Key, iv);

        using var memoryStream = new MemoryStream(encryptedBytes);
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var streamReader = new StreamReader(cryptoStream);

        return streamReader.ReadToEnd();
    }
}
