using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Functions.Crypto
{
    /// <summary>
    /// The available hashing functions.
    /// </summary>
    public enum Hash
    {
        /// <summary>The MD4 hashing function (128 bits digest).</summary>
        MD4,

        /// <summary>The MD5 hashing function (128 bits digest).</summary>
        MD5,

        /// <summary>The SHA-1 hashing function (160 bits digest).</summary>
        SHA1,

        /// <summary>The SHA-256 hashing function (256 bits digest).</summary>
        SHA256,

        /// <summary>The SHA-384 hashing function (384 bits digest).</summary>
        SHA384,

        /// <summary>The SHA-512 hashing function (512 bits digest).</summary>
        SHA512,
    }

    /// <summary>
    /// Provides methods for encrypting, decrypting and generating signatures.
    /// </summary>
    public static class Crypto
    {
        #region Hash and Hmac
        
        /// <summary>
        /// Hashes a string through MD4.
        /// </summary>
        /// <param name="input">The string for which to calculate the hsh</param>
        /// <returns>An uppercase hex string.</returns>
        public static string MD4(string input)
        {
            return MD4Digest(Encoding.UTF8.GetBytes(input)).ToHex();
        }

        /// <summary>
        /// Hashes a string through MD5.
        /// </summary>
        /// <param name="input">The string for which to calculate the hash</param>
        /// <returns>An uppercase hex string.</returns>
        public static string MD5(string input)
        {
            using (MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                return md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input)).ToHex();
            }
        }

        /// <summary>
        /// Calculates an MD5 hash signature.
        /// </summary>
        /// <param name="input">The message for which a signature will be generated</param>
        /// <param name="key">The secret key to use to sign the message</param>
        /// <param name="base64">Whether to return the string as Base64 or as a Hex</param>
        /// <returns>A base64 or uppercase hex string.</returns>
        public static string HMACMD5(string input, string key, bool base64)
        {
            HMACMD5 hmac = new HMACMD5(System.Text.Encoding.ASCII.GetBytes(key));
            var hash = hmac.ComputeHash(System.Text.Encoding.ASCII.GetBytes(input));
            if (base64) { return Convert.ToBase64String(hash); }
            else { return ToHex(hash); }
        }

        /// <summary>
        /// Hashes a string through SHA-1.
        /// </summary>
        /// <param name="input">The string for which to calculate the hash</param>
        /// <returns>An uppercase hex string.</returns>
        public static string SHA1(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                return sha1.ComputeHash(Encoding.UTF8.GetBytes(input)).ToHex();
            }
        }

        /// <summary>
        /// Calculates a SHA-1 hash signature.
        /// </summary>
        /// <param name="input">The message for which a signature will be generated</param>
        /// <param name="key">The secret key to use to sign the message</param>
        /// <param name="base64">Whether to return the string as Base64 or as a Hex</param>
        /// <returns>A base64 or uppercase hex string.</returns>
        public static string HMACSHA1(string input, string key, bool base64)
        {
            HMACSHA1 hmac = new HMACSHA1(System.Text.Encoding.ASCII.GetBytes(key));
            var hash = hmac.ComputeHash(System.Text.Encoding.ASCII.GetBytes(input));
            if (base64) { return Convert.ToBase64String(hash); }
            else { return ToHex(hash); }
        }

        /// <summary>
        /// Hashes a string through SHA-256.
        /// </summary>
        /// <param name="input">The string for which to calculate the hash</param>
        /// <returns>An uppercase hex string.</returns>
        public static string SHA256(string input)
        {
            using (SHA256Managed sha256 = new SHA256Managed())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(input)).ToHex();
            }
        }

        /// <summary>
        /// Calculates an SHA-256 hash signature.
        /// </summary>
        /// <param name="input">The message for which a signature will be generated</param>
        /// <param name="key">The secret key to use to sign the message</param>
        /// <param name="base64">Whether to return the string as Base64 or as a Hex</param>
        /// <returns>A base64 or uppercase hex string.</returns>
        public static string HMACSHA256(string input, string key, bool base64)
        {
            HMACSHA256 hmac = new HMACSHA256(System.Text.Encoding.ASCII.GetBytes(key));
            var hash = hmac.ComputeHash(System.Text.Encoding.ASCII.GetBytes(input));
            if (base64) { return Convert.ToBase64String(hash); }
            else { return ToHex(hash); }
        }

        /// <summary>
        /// Hashes a string through SHA-384.
        /// </summary>
        /// <param name="input">The string for which to calculate the hash</param>
        /// <returns>An uppercase hex string.</returns>
        public static string SHA384(string input)
        {
            using (SHA384Managed sha384 = new SHA384Managed())
            {
                return sha384.ComputeHash(Encoding.UTF8.GetBytes(input)).ToHex();
            }
        }

        /// <summary>
        /// Calculates a SHA-384 hash signature.
        /// </summary>
        /// <param name="input">The message for which a signature will be generated</param>
        /// <param name="key">The secret key to use to sign the message</param>
        /// <param name="base64">Whether to return the string as Base64 or as a Hex</param>
        /// <returns>A base64 or uppercase hex string.</returns>
        public static string HMACSHA384(string input, string key, bool base64)
        {
            HMACSHA384 hmac = new HMACSHA384(System.Text.Encoding.ASCII.GetBytes(key));
            var hash = hmac.ComputeHash(System.Text.Encoding.ASCII.GetBytes(input));
            if (base64) { return Convert.ToBase64String(hash); }
            else { return ToHex(hash); }
        }

        /// <summary>
        /// Hashes a string through SHA-512.
        /// </summary>
        /// <param name="input">The string for which to calculate the hash</param>
        /// <returns>An uppercase hex string.</returns>
        public static string SHA512(string input)
        {
            using (SHA512Managed sha512 = new SHA512Managed())
            {
                return sha512.ComputeHash(Encoding.UTF8.GetBytes(input)).ToHex();
            }
        }

        /// <summary>
        /// Calculates a SHA-512 hash signature.
        /// </summary>
        /// <param name="input">The message for which a signature will be generated</param>
        /// <param name="key">The secret key to use to sign the message</param>
        /// <param name="base64">Whether to return the string as Base64 or as a Hex</param>
        /// <returns>A base64 or uppercase hex string.</returns>
        public static string HMACSHA512(string input, string key, bool base64)
        {
            HMACSHA512 hmac = new HMACSHA512(System.Text.Encoding.ASCII.GetBytes(key));
            var hash = hmac.ComputeHash(System.Text.Encoding.ASCII.GetBytes(input));
            if (base64) { return Convert.ToBase64String(hash); }
            else { return ToHex(hash); }
        }

        /// <summary>
        /// Converts a byte array to a hexadecimal string.
        /// </summary>
        /// <param name="bytes">The byte array to convert</param>
        /// <returns>An uppercase hexadecimal string.</returns>
        public static string ToHex(this byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }

        /// <summary>
        /// Converts a hex string to a byte array.
        /// </summary>
        /// <param name="input">The hex string</param>
        /// <returns>A byte array</returns>
        public static byte[] FromHex(this string input)
        {
            var resultantArray = new byte[input.Length / 2];
            for (var i = 0; i < resultantArray.Length; i++)
            {
                resultantArray[i] = System.Convert.ToByte(input.Substring(i * 2, 2), 16);
            }
            return resultantArray;
        }

        /// <summary>
        /// Converts from the Hash enum to the HashAlgorithmName default struct.
        /// </summary>
        /// <param name="type">The hash type as a Hash enum</param>
        /// <returns>The HashAlgorithmName equivalent.</returns>
        public static HashAlgorithmName ToHashAlgorithmName(this Hash type)
        {
            switch (type)
            {
                case Hash.MD5:
                    return HashAlgorithmName.MD5;

                case Hash.SHA1:
                    return HashAlgorithmName.SHA1;

                case Hash.SHA256:
                    return HashAlgorithmName.SHA256;

                case Hash.SHA384:
                    return HashAlgorithmName.SHA384;

                case Hash.SHA512:
                    return HashAlgorithmName.SHA512;

                default:
                    throw new NotSupportedException("No such algorithm name");
            }
        }

        #endregion

        #region RSA

        private static string RSAEncrypt(string dataToEncrypt, RSAParameters RSAKeyInfo, bool doOAEPPadding)
        {
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();

            RSA.ImportParameters(RSAKeyInfo);

            return Convert.ToBase64String(RSA.Encrypt(Convert.FromBase64String(dataToEncrypt), doOAEPPadding));
        }

        private static string RSADecrypt(string dataToDecrypt, RSAParameters RSAKeyInfo, bool doOAEPPadding)
        {
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();

            RSA.ImportParameters(RSAKeyInfo);

            return Convert.ToBase64String(RSA.Decrypt(Convert.FromBase64String(dataToDecrypt), doOAEPPadding));
        }
        
        /// <summary>
        /// Encrypts a string using RSA.
        /// </summary>
        /// <param name="data">The data to encrypt as a base64 string</param>
        /// <param name="password">The private key as a base64 string</param>
        /// <param name="modulus">The public key's modulus as a base64 string</param>
        /// <param name="exponent">The public key's exponent as a base64 string</param>
        /// <param name="oaep">Whether to use OAEP-SHA1 padding mode instead of PKCS1</param>
        /// <returns>The encrypted data encoded as base64.</returns>
        public static string RSAEncrypt(string data, string password, string modulus, string exponent, bool oaep)
        {
            return RSAEncrypt(
                data,
                new RSAParameters { 
                    D = Encoding.UTF8.GetBytes(password),
                    Modulus = Encoding.UTF8.GetBytes(modulus),
                    Exponent = Encoding.UTF8.GetBytes(exponent)
                },
                oaep
            );
        }

        /// <summary>
        /// Decrypts a string using RSA.
        /// </summary>
        /// <param name="data">The data to decrypt as a base64 string</param>
        /// <param name="password">The private key as a base64 string</param>
        /// <param name="modulus">The public key's modulus as a base64 string</param>
        /// <param name="exponent">The public key's exponent as a base64 string</param>
        /// <param name="oaep">Whether to use OAEP-SHA1 padding mode instead of PKCS v1.5</param>
        /// <returns>The decrypted data encoded as base64.</returns>
        public static string RSADecrypt(string data, string password, string modulus, string exponent, bool oaep)
        {
            return RSADecrypt(
                data,
                new RSAParameters
                {
                    D = Encoding.UTF8.GetBytes(password),
                    Modulus = Encoding.UTF8.GetBytes(modulus),
                    Exponent = Encoding.UTF8.GetBytes(exponent)
                },
                oaep
            );
        }
        #endregion

        #region KDF
        /// <summary>
        /// Generates a PKCS v5 #2.0 key using a Password-Based Key Derivation Function.
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <param name="salt">The salt to use encoded as base64. If empty, a random salt will be generated</param>
        /// <param name="saltSize">The random salt size that gets generated in case no salt is provided</param>
        /// <param name="iterations">The number of times the algorithm should be executed</param>
        /// <param name="type">The hashing algorithm to use</param>
        /// <param name="keyLength">The generated key length in bytes</param>
        /// <returns>The generated key as a base64 string.</returns>
        public static string PBKDF2PKCS5(string password, string salt, int saltSize = 8, int iterations = 1, int keyLength = 16, Hash type = Hash.SHA1)
        {
            if (salt != string.Empty)
            {
                using (var deriveBytes = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), iterations, type.ToHashAlgorithmName()))
                {
                    return Convert.ToBase64String(deriveBytes.GetBytes(keyLength));
                }
            }
            else
            {
                using (var deriveBytes = new Rfc2898DeriveBytes(password, saltSize, iterations, type.ToHashAlgorithmName()))
                {
                    return Convert.ToBase64String(deriveBytes.GetBytes(keyLength));
                }
            }
        }
        #endregion

        #region AES
        /// <summary>
        /// Encrypts a string with AES.
        /// </summary>
        /// <param name="data">The AES-encrypted data</param>
        /// <param name="key">The decryption key as base64</param>
        /// <param name="iv">The initial value as base64</param>
        /// <param name="mode">The cipher mode</param>
        /// <param name="padding">The padding mode</param>
        /// <returns>The AES-encrypted string encoded as base64</returns>
        public static string AESEncrypt(string data, string key, string iv = "", CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.None)
        {
            string encData = null;
            byte[][] keys = GetHashKeys(key, iv);

            try
            {
                encData = EncryptStringToBytes_Aes(data, keys[0], keys[1], mode, padding);
            }
            catch (CryptographicException) { }
            catch (ArgumentNullException) { }

            return encData;
        }

        /// <summary>
        /// Decrypts an AES-encrypted string.
        /// </summary>
        /// <param name="data">The AES-encrypted data encoded as base64</param>
        /// <param name="key">The decryption key as base64</param>
        /// <param name="iv">The initial value as base64</param>
        /// <param name="mode">The cipher mode</param>
        /// <param name="padding">The padding mode</param>
        /// <returns>The plaintext string</returns>
        public static string AESDecrypt(string data, string key, string iv = "", CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.None)
        {
            string decData = null;
            byte[][] keys = GetHashKeys(key, iv);

            try
            {
                decData = DecryptStringFromBytes_Aes(data, keys[0], keys[1], mode, padding);
            }
            catch (CryptographicException) { }
            catch (ArgumentNullException) { }

            return decData;
        }

        private static byte[][] GetHashKeys(string key, string iv)
        {
            byte[][] result = new byte[2][];

            SHA256 sha2 = new SHA256CryptoServiceProvider();

            byte[] rawKey = Convert.FromBase64String(key);
            byte[] rawIV = iv != string.Empty ? Convert.FromBase64String(iv) : Convert.FromBase64String(key);

            byte[] hashKey = sha2.ComputeHash(rawKey);
            byte[] hashIV = sha2.ComputeHash(rawIV);

            Array.Resize(ref hashIV, 16);

            result[0] = hashKey;
            result[1] = hashIV;

            return result;
        }

        private static string EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV, CipherMode mode, PaddingMode padding)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            byte[] encrypted;

            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Mode = mode;
                aesAlg.Padding = padding;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt =
                            new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(encrypted);
        }

        private static string DecryptStringFromBytes_Aes(string cipherTextString, byte[] Key, byte[] IV, CipherMode mode, PaddingMode padding)
        {
            byte[] cipherText = Convert.FromBase64String(cipherTextString);

            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            string plaintext = null;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Mode = mode;
                aesAlg.Padding = padding;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt =
                            new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }
        #endregion

        #region Private Methods
        private static byte[] MD4Digest(byte[] input)
        {
            // get padded uints from bytes
            var bytes = input.ToList();
            uint bitCount = (uint)(bytes.Count) * 8;
            bytes.Add(128);
            while (bytes.Count % 64 != 56) bytes.Add(0);
            var uints = new List<uint>();
            for (int i = 0; i + 3 < bytes.Count; i += 4)
                uints.Add(bytes[i] | (uint)bytes[i + 1] << 8 | (uint)bytes[i + 2] << 16 | (uint)bytes[i + 3] << 24);
            uints.Add(bitCount);
            uints.Add(0);

            // run rounds
            uint a = 0x67452301, b = 0xefcdab89, c = 0x98badcfe, d = 0x10325476;
            Func<uint, uint, uint> rol = (x, y) => x << (int)y | x >> 32 - (int)y;
            for (int q = 0; q + 15 < uints.Count; q += 16)
            {
                var chunk = uints.GetRange(q, 16);
                uint aa = a, bb = b, cc = c, dd = d;
                Action<Func<uint, uint, uint, uint>, uint[]> round = (f, y) =>
                {
                    foreach (uint i in new[] { y[0], y[1], y[2], y[3] })
                    {
                        a = rol(a + f(b, c, d) + chunk[(int)(i + y[4])] + y[12], y[8]);
                        d = rol(d + f(a, b, c) + chunk[(int)(i + y[5])] + y[12], y[9]);
                        c = rol(c + f(d, a, b) + chunk[(int)(i + y[6])] + y[12], y[10]);
                        b = rol(b + f(c, d, a) + chunk[(int)(i + y[7])] + y[12], y[11]);
                    }
                };
                round((x, y, z) => (x & y) | (~x & z), new uint[] { 0, 4, 8, 12, 0, 1, 2, 3, 3, 7, 11, 19, 0 });
                round((x, y, z) => (x & y) | (x & z) | (y & z), new uint[] { 0, 1, 2, 3, 0, 4, 8, 12, 3, 5, 9, 13, 0x5a827999 });
                round((x, y, z) => x ^ y ^ z, new uint[] { 0, 2, 1, 3, 0, 8, 4, 12, 3, 9, 11, 15, 0x6ed9eba1 });
                a += aa; b += bb; c += cc; d += dd;
            }

            // return bytes
            return new[] { a, b, c, d }.SelectMany(BitConverter.GetBytes).ToArray();
        }
        #endregion
    }
}
