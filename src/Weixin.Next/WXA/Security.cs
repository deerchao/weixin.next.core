using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Weixin.Next.WXA
{
    public class Security
    {
        public static bool VerifySignature(string rawData, string sessionKey, string signature)
        {
            var input = Encoding.UTF8.GetBytes(rawData + sessionKey);
            var output = SHA1.Create().ComputeHash(input);
            return string.Concat(output.Select(x => x.ToString("x2"))).Equals(signature, StringComparison.OrdinalIgnoreCase);
        }

        public static string DecryptData(string encryptedData, string sessionKey, string iv)
        {
            try
            {
                using (var rijndaelManaged = new RijndaelManaged
                {
                    Key = Convert.FromBase64String(sessionKey),
                    IV = Convert.FromBase64String(iv),
                    Mode = CipherMode.CBC
                })
                {
                    using (var memoryStream = new MemoryStream(Convert.FromBase64String(encryptedData)))
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, rijndaelManaged.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            return new StreamReader(cryptoStream, Encoding.UTF8).ReadToEnd();
                        }
                    }
                }
            }
            catch (CryptographicException)
            {
                return null;
            }
        }
    }
}
