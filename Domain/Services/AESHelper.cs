using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Common.Services
{
    public static class AESHelper
    {
        private static readonly byte[] cryptkey = Encoding.ASCII.GetBytes("1234567891234567");
        private static readonly byte[] initVector = Encoding.ASCII.GetBytes("1234567891234567");
        public static string Encrypt(string text)
        {
            try
            {
                using (var rijndaelManaged = new RijndaelManaged
                {
                    Key = cryptkey,
                    IV = initVector,
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7,
                    KeySize = 128
                })
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, rijndaelManaged.CreateEncryptor(), CryptoStreamMode.Write))
                        using (var streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.WriteAsync(text);
                        }

                        // Convert the encrypted data to Base64 after closing the CryptoStream
                        return Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
            catch (CryptographicException e)
            {
                Console.WriteLine($"A Cryptographic error occurred: {e.Message}");
                return null;
            }
        }

        public static string Decrypt(string cipherData)
        {
            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherData);

                using (var rijndaelManaged = new RijndaelManaged
                {
                    Key = cryptkey,
                    IV = initVector,
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7,
                    KeySize = 128
                })
                {
                    using (var memoryStream = new MemoryStream(cipherBytes))
                    using (var cryptoStream = new CryptoStream(memoryStream, rijndaelManaged.CreateDecryptor(), CryptoStreamMode.Read))
                    using (var streamReader = new StreamReader(cryptoStream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
            catch (CryptographicException e)
            {
                Console.WriteLine($"A Cryptographic error occurred: {e.Message}");
                return null;
            }
        }
    }
}
