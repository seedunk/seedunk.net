  
using Newtonsoft.Json.Linq; 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using MD5 = Seedunk.Net.Utils.MD5;
namespace Seedunk.Net
{
    public static class StringExtensions
    {
        public static string ToUnderLineStyle(this string data)
        {
            return Regex.Replace(data, "([A-Z])", "_$1").ToLower().TrimStart('_');
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="left"></param>
        /// <param name=""></param>
        /// <returns></returns>
        /// "\[(.+?)\]\(#import\)"
        public static string[] Find(this string str,string str1) {
             
            var matches = Regex.Matches(str,str1);
            string[] result = new string[matches.Count];
             
            int i = 0;
            foreach (Match m in matches)
            { 
                result[i] = m.Groups[1].Value;
                i++; 
            }
            return result;
        }
      
        public static Dictionary<string,object> ToKeyValue(this string jsonString)
        {


            Dictionary<string, object> kv=  new Dictionary<string, object>();
           JObject jsonObject = JObject.Parse(jsonString);

            // 遍历JSON对象中的属性
            foreach (var property in jsonObject.Properties())
            { 
                   kv.Add(property.Name, property.Value.ToString()); 
            }
            return kv; 

        }

         public static string GetMD5(this string s, string _input_charset="utf-8" )
         {
                return MD5.Encrypt(s);
         }
 
        
           public static string Encrypt(this string plainText, string key, string iv = "1234567890123456")
            {
                byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
                byte[] keyBytes = Encoding.UTF8.GetBytes(key.GetMD5());
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = keyBytes;
                    aesAlg.IV = ivBytes;
                    aesAlg.Mode = CipherMode.CBC; // 使用CBC模式
                    aesAlg.Padding = PaddingMode.PKCS7; // 使用PKCS7填充
                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            csEncrypt.Write(plainTextBytes, 0, plainTextBytes.Length);
                            csEncrypt.FlushFinalBlock();
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            } 
        public static string Decrypt(this string cipherText, string key, string iv= "1234567890123456")
        {
            byte[] ivBytes = Encoding.UTF8.GetBytes(iv);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key.GetMD5());
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = keyBytes;
                aesAlg.IV = ivBytes;
                aesAlg.Mode = CipherMode.CBC; // 使用CBC模式
                aesAlg.Padding = PaddingMode.PKCS7; // 使用PKCS7填充
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherTextBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader reader = new StreamReader(csDecrypt))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 格式化
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this string str )
        {
            return DateTime.Parse(str);
        }
        public static string GetExtension(this string str)
        {
            return Path.GetExtension(str);  
        }
        public static bool HasExtension(this string str,string ext)
        {
            return  Path.GetExtension(str).ToLower().Equals(ext.ToLower());
        }
        public static byte[] ToASCIIBytes(this string str )
        {
            return Encoding.ASCII.GetBytes(str);
        }
        public static string ToASCIIBase64(this string str) {
            return Convert.ToBase64String(ToASCIIBytes(str));
        }
        public static byte[] ToUTF8Bytes(this string str)
        {
             return Encoding.UTF8.GetBytes(str);
        }
        public static string ToUTF8Base64(this string str)
        {
            return Convert.ToBase64String(ToUTF8Bytes(str));
        }
    }
}
