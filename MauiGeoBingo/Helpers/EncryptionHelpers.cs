using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MauiGeoBingo.Helpers
{

    // TODO: jag har inspererats av denna artikel till helpern här: https://code-maze.com/csharp-string-encryption-decryption/

    internal class EncryptionHelpers
    {
        private static byte[] IV =
        {
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
        };

        private static byte[] DeriveKeyFromPassword(string password)
        {
            var emptySalt = Array.Empty<byte>();
            var iterations = 1000;
            var desiredKeyLength = 16; // 16 bytes equal 128 bits.
            var hashMethod = HashAlgorithmName.SHA384;
            return Rfc2898DeriveBytes.Pbkdf2(Encoding.Unicode.GetBytes(password),
                                             emptySalt,
                                             iterations,
                                             hashMethod,
                                             desiredKeyLength);
        }

        public static async Task<byte[]> EncryptAsync(string clearText, string passphrase)
        {
            using Aes aes = Aes.Create();
            aes.Key = DeriveKeyFromPassword(passphrase);
            aes.IV = IV;
            using MemoryStream output = new();
            using CryptoStream cryptoStream = new(output, aes.CreateEncryptor(), CryptoStreamMode.Write);
            await cryptoStream.WriteAsync(Encoding.Unicode.GetBytes(clearText));
            await cryptoStream.FlushFinalBlockAsync();
            return output.ToArray();
        }

        public static async Task<string> DecryptAsync(byte[] encrypted, string passphrase)
        {
            using Aes aes = Aes.Create();
            aes.Key = DeriveKeyFromPassword(passphrase);
            aes.IV = IV;
            using MemoryStream input = new(encrypted);
            using CryptoStream cryptoStream = new(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using MemoryStream output = new();
            await cryptoStream.CopyToAsync(output);
            return Encoding.Unicode.GetString(output.ToArray());
        }

        public static async Task SetAsync(string key, string clearText, string passphrase)
        {
            byte[] data = await EncryptAsync(clearText, passphrase);
            StoreNotStandardData.Set(key, data);
        }

        public static async Task<string?> GetAsync(string key, string passphrase)
        {
            byte[]? bytes = StoreNotStandardData.Get<byte[]>(key);
            if (bytes == null) return null;

            return await DecryptAsync(bytes, passphrase);
        }
    }

    public class StoreNotStandardData
    {

        /// <summary>
        /// Store item using key
        /// </summary>
        /// <param name="key">string representation of key</param>
        /// <param name="value">value</param>
        public static void Set(string key, object value)
        {
            string jsonString = JsonSerializer.Serialize(value);
            if (jsonString != null && !string.IsNullOrEmpty(jsonString))
            {
                Preferences.Set(key, jsonString);
            }
        }

        /// <summary>
        /// Get an item using a certain key, with type T
        /// </summary>
        /// <typeparam name="T">type of parameter</typeparam>
        /// <param name="key">string representation of key</param>
        /// <returns></returns>
        public static T? Get<T>(string key)
        {
            T? result = default;
            string jsonString = Preferences.Get(key, string.Empty);

            if (jsonString != null && !string.IsNullOrEmpty(jsonString))
            {
                result = JsonSerializer.Deserialize<T>(jsonString);
            }
            return result;
        }

        /// <summary>
        /// Delete an item with a certain key
        /// </summary>
        /// <param name="key">string representation of key</param>
        public static void Delete(string key)
        {
            Preferences.Remove(key);
        }

        /// <summary>
        /// Check if an item with a certain key exists
        /// </summary>
        /// <param name="key">string representation of key</param>
        /// <returns>bool</returns>
        public static bool Exists(string key)
        {
            return Preferences.ContainsKey(key);
        }

        
    }
}
