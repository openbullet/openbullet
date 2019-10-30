using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encoding;
using System.Threading.Tasks;

namespace RuriLib.Functions.Crypto
{
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

        #endregion

        #region RSA
        public static string RSAEncrypt(string input, int size)
        {
            var data = Encoding.UTF8.GetBytes(input);

            using (var rsa = new RSACryptoServiceProvider(size))
            {
                try
                {
                    rsa.FromXmlString(input);
                    var encryptedData = rsa.Encrypt(data, true);
                    Console.WriteLine(Convert.ToString(encryptedData));
                    return Convert.ToString(encryptedData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return "";
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

        public static string RSADecrypt(string input, int size)
        {
            var data = Encoding.UTF8.GetBytes(input);

            using (var rsa = new RSACryptoServiceProvider(size))
            {
                try
                {
                    rsa.FromXmlString(input);
                    var resultBytes = System.Text.Encoding.ASCII.GetBytes(input);
                    var decryptedBytes = rsa.Decrypt(resultBytes, true);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

        public static string SteamRSAEncrypt(RsaParameters rsaParam)
        {
            // Convert the public keys to BigIntegers
            var modulus = CreateBigInteger(rsaParam.Modulus);
            var exponent = CreateBigInteger(rsaParam.Exponent);

            // (modulus.ToByteArray().Length - 1) * 8
            //modulus has 256 bytes multiplied by 8 bits equals 2048
            var encryptedNumber = Pkcs1Pad2(rsaParam.Password, (2048 + 7) >> 3);

            // And now, the RSA encryption
            encryptedNumber = System.Numerics.BigInteger.ModPow(encryptedNumber, exponent, modulus);

            //Reverse number and convert to base64
            var encryptedString = Convert.ToBase64String(encryptedNumber.ToByteArray().Reverse().ToArray());

            return encryptedString;
        }

        private static System.Numerics.BigInteger CreateBigInteger(string hex)
        {
            return System.Numerics.BigInteger.Parse("00" + hex, NumberStyles.AllowHexSpecifier);
        }

        private static System.Numerics.BigInteger Pkcs1Pad2(string data, int keySize)
        {
            if (keySize < data.Length + 11)
                return new System.Numerics.BigInteger();

            var buffer = new byte[256];
            var i = data.Length - 1;

            while (i >= 0 && keySize > 0)
            {
                buffer[--keySize] = (byte)data[i--];
            }

            // Padding, I think
            var random = new Random();
            buffer[--keySize] = 0;
            while (keySize > 2)
            {
                buffer[--keySize] = (byte)random.Next(1, 256);
                //buffer[--keySize] = 5;
            }

            buffer[--keySize] = 2;
            buffer[--keySize] = 0;

            Array.Reverse(buffer);

            return new System.Numerics.BigInteger(buffer);
        }

        #endregion


        #region AES
        /// <summary>
        /// Encrypts a string with AES.
        /// </summary>
        /// <param name="key">The encryption key</param>
        /// <param name="data">The data to encrypt</param>
        /// <returns>The AES-encrypted string</returns>
        public static string AESEncrypt(string key, string data)
        {
            string encData = null;
            byte[][] keys = GetHashKeys(key);

            try
            {
                encData = EncryptStringToBytes_Aes(data, keys[0], keys[1]);
            }
            catch (CryptographicException) { }
            catch (ArgumentNullException) { }

            return encData;
        }

        /// <summary>
        /// Decrypts an AES-encrypted string.
        /// </summary>
        /// <param name="key">The decryption key</param>
        /// <param name="data">The AES-encrypted data</param>
        /// <returns>The plaintext string</returns>
        public static string AESDecrypt(string key, string data)
        {
            string decData = null;
            byte[][] keys = GetHashKeys(key);

            try
            {
                decData = DecryptStringFromBytes_Aes(data, keys[0], keys[1]);
            }
            catch (CryptographicException) { }
            catch (ArgumentNullException) { }

            return decData;
        }

        private static byte[][] GetHashKeys(string key)
        {
            byte[][] result = new byte[2][];
            Encoding enc = Encoding.UTF8;

            SHA256 sha2 = new SHA256CryptoServiceProvider();

            byte[] rawKey = enc.GetBytes(key);
            byte[] rawIV = enc.GetBytes(key);

            byte[] hashKey = sha2.ComputeHash(rawKey);
            byte[] hashIV = sha2.ComputeHash(rawIV);

            Array.Resize(ref hashIV, 16);

            result[0] = hashKey;
            result[1] = hashIV;

            return result;
        }

        private static string EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
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

        private static string DecryptStringFromBytes_Aes(string cipherTextString, byte[] Key, byte[] IV)
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

    /// <summary>
    /// The RSA parameters.
    /// </summary>
    public struct RsaParameters
    {
        /// <summary>The key exponent.</summary>
        public string Exponent;

        /// <summary>The key modulus.</summary>
        public string Modulus;

        /// <summary>The encryption password.</summary>
        public string Password;
    }
}
