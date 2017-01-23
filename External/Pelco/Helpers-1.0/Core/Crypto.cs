using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Pelco.Helpers
{
    public class Crypto
    {
        private const string Salt = "XbQL?pdr h=w/G4qF+[e@-:X<cPelco!cN[865$%,mN+)-)<e#`I+^My _|NnZXXj(g5S";
        private const ushort RandomSaltSize = 4;
        private string _password;

        public Crypto(string password)
        {
            _password = password;
        }

        public void EncryptFile(string inPath, string outPath, bool deleteInPath = false)
        {
            try
            {
                byte[] bytesToBeEncrypted = File.ReadAllBytes(inPath);
                byte[] bytesEncrypted = AESEncrypt(bytesToBeEncrypted);
                File.WriteAllBytes(outPath, bytesEncrypted);
                if (deleteInPath)
                    File.Delete(inPath);
            }
            catch (Exception)
            { }
        }

        public void DecryptFile(string inPath, string outPath, bool deleteInPath = false)
        {
            try
            {
                byte[] bytesToBeDecrypted = File.ReadAllBytes(inPath);
                byte[] bytesDecrypted = AESDecrypt(bytesToBeDecrypted);
                File.WriteAllBytes(outPath, bytesDecrypted);
                if (deleteInPath)
                    File.Delete(inPath);
            }
            catch (Exception)
            { }
        }

        public string Encrypt<T>(T objectToEncrypt) where T : class
        {
            string base64String = null;
            try
            {
                string serializedObj = Serialization.Serialize(objectToEncrypt);
                base64String = Encrypt(serializedObj);
            }
            catch (Exception)
            { }
            return base64String;
        }

        public T Decrypt<T>(string base64String) where T : class
        {
            T decryptedObj = default(T);
            try
            {
                string serializedObj = Decrypt(base64String);
                decryptedObj = Serialization.Deserialize<T>(serializedObj);
            }
            catch (Exception)
            { }
            return decryptedObj;
        }

        public byte[] AESEncrypt(byte[] buffer)
        {
            byte[] encryptedBytes = null;

            try
            {
                // Set your salt here, change it to meet your flavor:
                // The salt bytes must be at least 8 bytes.
                byte[] saltBytes = Encoding.UTF8.GetBytes(Salt);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(_password);
                passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                using (System.IO.MemoryStream ms = new MemoryStream())
                {
                    using (RijndaelManaged AES = new RijndaelManaged())
                    {
                        AES.KeySize = 256;
                        AES.BlockSize = 128;

                        var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                        AES.Key = key.GetBytes(AES.KeySize / 8);
                        AES.IV = key.GetBytes(AES.BlockSize / 8);

                        AES.Mode = CipherMode.CBC;

                        using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(buffer, 0, buffer.Length);
                            cs.Close();
                        }
                        encryptedBytes = ms.ToArray();
                    }
                }
            }
            catch (Exception)
            { }
            return encryptedBytes;
        }

        public byte[] AESDecrypt(byte[] buffer)
        {
            byte[] decryptedBytes = null;

            try
            {
                // Set your salt here, change it to meet your flavor:
                // The salt bytes must be at least 8 bytes.
                byte[] saltBytes = Encoding.UTF8.GetBytes(Salt);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(_password);
                passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (RijndaelManaged AES = new RijndaelManaged())
                    {
                        AES.KeySize = 256;
                        AES.BlockSize = 128;

                        var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                        AES.Key = key.GetBytes(AES.KeySize / 8);
                        AES.IV = key.GetBytes(AES.BlockSize / 8);

                        AES.Mode = CipherMode.CBC;

                        using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(buffer, 0, buffer.Length);
                            cs.Close();
                        }
                        decryptedBytes = ms.ToArray();
                    }
                }
            }
            catch (Exception)
            { }
            return decryptedBytes;
        }

        private string Encrypt(string text)
        {
            string base64String = null;
            byte[] originalBytes = Encoding.UTF8.GetBytes(text);
            byte[] encryptedBytes = null;

            // Generating salt bytes
            byte[] saltBytes = GetRandomBytes();

            // Appending salt bytes to original bytes
            byte[] bytesToBeEncrypted = new byte[saltBytes.Length + originalBytes.Length];
            for (int i = 0; i < saltBytes.Length; i++)
                bytesToBeEncrypted[i] = saltBytes[i];
            for (int i = 0; i < originalBytes.Length; i++)
                bytesToBeEncrypted[i + saltBytes.Length] = originalBytes[i];

            encryptedBytes = AESEncrypt(bytesToBeEncrypted);
            base64String = Convert.ToBase64String(encryptedBytes);
            return base64String;
        }

        private string Decrypt(string text)
        {
            string decryptedString = null;
            byte[] bytesToBeDecrypted = Convert.FromBase64String(text);
            byte[] decryptedBytes = AESDecrypt(bytesToBeDecrypted);

            // Removing salt bytes, retrieving original bytes
            byte[] originalBytes = new byte[decryptedBytes.Length - RandomSaltSize];
            for (int i = RandomSaltSize; i < decryptedBytes.Length; i++)
                originalBytes[i - RandomSaltSize] = decryptedBytes[i];

            decryptedString = Encoding.UTF8.GetString(originalBytes);
            return decryptedString;
        }

        private byte[] GetRandomBytes()
        {
            byte[] ba = new byte[RandomSaltSize];
            RNGCryptoServiceProvider.Create().GetBytes(ba);
            return ba;
        }
    }
}
