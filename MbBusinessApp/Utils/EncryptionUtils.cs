using System;
using System.Security.Cryptography;
using System.Text;

namespace MbBusinessApp.Utils
{
    public class EncryptionUtils
    {
        private const int GCM_IV_LENGTH = 12;   // 12 bytes IV
        private const int GCM_TAG_LENGTH = 16;  // 16 bytes tag (128 bits)

        /// <summary>
        /// Encrypts text using AES-GCM encryption
        /// </summary>
        /// <param name="cleartext">Text to encrypt</param>
        /// <param name="encKey">Encryption key</param>
        /// <returns>Base64 encoded encrypted string</returns>
        public static string Encrypt(string cleartext, string encKey)
        {
            try
            {
                byte[] clearTextBytes = Encoding.UTF8.GetBytes(cleartext);
                
                // Ensure key is proper length for AES (16, 24, or 32 bytes)
                byte[] keyBytes = GetValidKeyBytes(encKey);

                // Generate random IV
                byte[] iv = new byte[GCM_IV_LENGTH];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(iv);
                }

                // Create AES-GCM cipher
                using (var aesGcm = new AesGcm(keyBytes, GCM_TAG_LENGTH))
                {
                    byte[] ciphertext = new byte[clearTextBytes.Length];
                    byte[] tag = new byte[GCM_TAG_LENGTH];

                    aesGcm.Encrypt(iv, clearTextBytes, ciphertext, tag);

                    // Combine IV + ciphertext + tag
                    byte[] message = new byte[GCM_IV_LENGTH + ciphertext.Length + GCM_TAG_LENGTH];
                    Buffer.BlockCopy(iv, 0, message, 0, GCM_IV_LENGTH);
                    Buffer.BlockCopy(ciphertext, 0, message, GCM_IV_LENGTH, ciphertext.Length);
                    Buffer.BlockCopy(tag, 0, message, GCM_IV_LENGTH + ciphertext.Length, GCM_TAG_LENGTH);

                    return Convert.ToBase64String(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Encryption error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Decrypts text using AES-GCM decryption
        /// </summary>
        /// <param name="base64Cipher">Base64 encoded encrypted string</param>
        /// <param name="keyStr">Decryption key</param>
        /// <returns>Decrypted text</returns>
        public static string Decrypt(string base64Cipher, string keyStr)
        {
            byte[] decoded = Convert.FromBase64String(base64Cipher);
            byte[] iv = new byte[GCM_IV_LENGTH];
            byte[] tag = new byte[GCM_TAG_LENGTH];
            byte[] cipherText = new byte[decoded.Length - GCM_IV_LENGTH - GCM_TAG_LENGTH];

            Buffer.BlockCopy(decoded, 0, iv, 0, GCM_IV_LENGTH);
            Buffer.BlockCopy(decoded, GCM_IV_LENGTH, cipherText, 0, cipherText.Length);
            Buffer.BlockCopy(decoded, GCM_IV_LENGTH + cipherText.Length, tag, 0, GCM_TAG_LENGTH);

            byte[] keyBytes = GetValidKeyBytes(keyStr);

            using (var aesGcm = new AesGcm(keyBytes, GCM_TAG_LENGTH))
            {
                byte[] plaintext = new byte[cipherText.Length];
                aesGcm.Decrypt(iv, cipherText, tag, plaintext);
                return Encoding.UTF8.GetString(plaintext);
            }
        }

        /// <summary>
        /// Generates HMAC-SHA256 hash
        /// </summary>
        /// <param name="message">Message to hash</param>
        /// <param name="key">HMAC key</param>
        /// <returns>Hex string of hash</returns>
        public static string GetHmac(string message, string key)
        {
            try
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                using (var hmac = new HMACSHA256(keyBytes))
                {
                    byte[] hashBytes = hmac.ComputeHash(messageBytes);
                    return ToHexString(hashBytes);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HMAC error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Converts byte array to hex string
        /// </summary>
        private static string ToHexString(byte[] bytes)
        {
            StringBuilder hexString = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                hexString.Append(b.ToString("x2"));
            }
            return hexString.ToString();
        }

        /// <summary>
        /// Ensures key is valid AES key length (16, 24, or 32 bytes)
        /// </summary>
        private static byte[] GetValidKeyBytes(string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            
            // If key is already a valid AES size, use it as-is
            if (keyBytes.Length == 16 || keyBytes.Length == 24 || keyBytes.Length == 32)
            {
                return keyBytes;
            }
            
            // Otherwise, pad or truncate to 32 bytes (AES-256)
            byte[] validKey = new byte[32];
            if (keyBytes.Length > 32)
            {
                // Truncate if too long
                Buffer.BlockCopy(keyBytes, 0, validKey, 0, 32);
            }
            else
            {
                // Pad with zeros if too short
                Buffer.BlockCopy(keyBytes, 0, validKey, 0, keyBytes.Length);
            }
            return validKey;
        }

        /// <summary>
        /// Generates encryption key with current date
        /// </summary>
        /// <param name="baseKey">Base key (e.g., "MBBOB12#")</param>
        /// <returns>Key with date appended</returns>
        public static string GetEncryptionKey(string baseKey)
        {
            string currentDate = DateTime.UtcNow.ToString("yyyyMMdd");
            return baseKey + currentDate;
        }
    }
}
