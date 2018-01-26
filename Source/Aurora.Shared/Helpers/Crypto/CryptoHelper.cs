// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage.Streams;

namespace Aurora.Shared.Helpers.Crypto
{
    public class CryptoHelper
    {
        public static string ToBase64(string str)
        {
            var buffer = CryptographicBuffer.ConvertStringToBinary(str, BinaryStringEncoding.Utf8);
            return CryptographicBuffer.EncodeToBase64String(buffer);
        }

        public static string FromBase64(string str)
        {
            var buffer = CryptographicBuffer.DecodeFromBase64String(str);
            return CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, buffer);
        }

        /// <summary>
        /// 对指定 utf-8 字符串执行 MD5 校验
        /// </summary>
        /// <param name="str">注意编码格式为 utf-8</param>
        /// <returns></returns>
        public static string ComputeMD5(string str)
        {
            var alg = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);

            IBuffer buff = CryptographicBuffer.CreateFromByteArray(Encoding.UTF8.GetBytes(str));
            var hashed = alg.HashData(buff);
            var res = CryptographicBuffer.EncodeToHexString(hashed);
            return res;
        }


        /// <summary>
        /// 对字符串依据指定的算法和密钥进行加密，如果使用 CBC 算法，还需要初始化向量
        /// </summary>
        /// <param name="content">源字符串</param>
        /// <param name="strAlgName">加密算法</param>
        /// <param name="encoding">字符串编码方式</param>
        /// <param name="key">密钥</param>
        /// <param name="iniVec">CBC 初始化向量</param>
        /// <returns></returns>
        public static IBuffer CipherEncryption(string content, string strAlgName,
            BinaryStringEncoding encoding, CryptographicKey key, IBuffer iniVec = null)
        {
            // Create a buffer that contains the encoded message to be encrypted. 
            IBuffer buffContent = CryptographicBuffer.ConvertStringToBinary(content, encoding);

            // Open a symmetric algorithm provider for the specified algorithm. 
            SymmetricKeyAlgorithmProvider objAlg = SymmetricKeyAlgorithmProvider.OpenAlgorithm(strAlgName);

            // Determine whether the message length is a multiple of the block length.
            // This is not necessary for PKCS #7 algorithms which automatically pad the
            // message to an appropriate length.
            if (!strAlgName.Contains("PKCS7"))
            {
                if ((buffContent.Length % objAlg.BlockLength) != 0)
                {
                    throw new Exception("Message buffer length must be multiple of block length.");
                }
            }

            if (strAlgName.Contains("CBC") && iniVec == null)
            {
                throw new ArgumentException("Using CBC Encryption, initial vector must have value");
            }
            // Encrypt the data and return.
            IBuffer buffEncrypt = CryptographicEngine.Encrypt(key, buffContent, iniVec);
            return buffEncrypt;
        }

        /// <summary>
        /// 根据加密算法和长度生成一个对称密钥，现在尚不能导出密钥
        /// </summary>
        /// <param name="strAlgName">算法</param>
        /// <param name="keyLength">密钥长度</param>
        /// <returns></returns>
        public static CryptographicKey GenerateKey(string strAlgName, uint keyLength)
        {
            SymmetricKeyAlgorithmProvider objAlg = SymmetricKeyAlgorithmProvider.OpenAlgorithm(strAlgName);
            IBuffer keyMaterial = CryptographicBuffer.GenerateRandom(keyLength);
            return objAlg.CreateSymmetricKey(keyMaterial);
        }

        /// <summary>
        /// 生成 CBC 算法初始化向量
        /// </summary>
        /// <param name="strAlgName"></param>
        /// <returns></returns>
        public static IBuffer GenerateCBCVector(string strAlgName)
        {
            if (strAlgName.Contains("CBC"))
            {
                SymmetricKeyAlgorithmProvider objAlg = SymmetricKeyAlgorithmProvider.OpenAlgorithm(strAlgName);
                return CryptographicBuffer.GenerateRandom(objAlg.BlockLength);
            }
            else return null;
        }

        /// <summary>
        /// 对一段 Buffer 根据指定的算法和密钥解密，如果使用 CBC 算法，还需要初始化向量
        /// </summary>
        /// <param name="strAlgName">算法</param>
        /// <param name="buffEncrypt">加密缓冲区</param>
        /// <param name="encoding">编码方式</param>
        /// <param name="key">密钥</param>
        /// /// <param name="iniVec">初始化向量</param>
        /// <returns></returns>
        public static string CipherDecryption(string strAlgName, IBuffer buffEncrypt,
            BinaryStringEncoding encoding, CryptographicKey key, IBuffer iniVec = null)
        {
            // Declare a buffer to contain the decrypted data.
            IBuffer buffDecrypted;

            // Open an symmetric algorithm provider for the specified algorithm. 
            SymmetricKeyAlgorithmProvider objAlg = SymmetricKeyAlgorithmProvider.OpenAlgorithm(strAlgName);

            // The input key must be securely shared between the sender of the encrypted message
            // and the recipient. The initialization vector must also be shared but does not
            // need to be shared in a secure manner. If the sender encodes a message string 
            // to a buffer, the binary encoding method must also be shared with the recipient.
            buffDecrypted = CryptographicEngine.Decrypt(key, buffEncrypt, iniVec);

            // Convert the decrypted buffer to a string (for display). If the sender created the
            // original message buffer from a string, the sender must tell the recipient what 
            // BinaryStringEncoding value was used. Here, BinaryStringEncoding.Utf8 is used to
            // convert the message to a buffer before encryption and to convert the decrypted
            // buffer back to the original plaintext.
            return ToString(encoding, buffDecrypted);
        }

        public static string ToString(BinaryStringEncoding encoding, IBuffer buff)
        {
            return CryptographicBuffer.ConvertBinaryToString(encoding, buff);
        }

        public static async Task<IBuffer> ProtectAsync(string strMsg, string strDescriptor, BinaryStringEncoding encoding)
        {
            // Create a DataProtectionProvider object for the specified descriptor.
            DataProtectionProvider Provider = new DataProtectionProvider(strDescriptor);

            // Encode the plaintext input message to a buffer.
            encoding = BinaryStringEncoding.Utf8;
            IBuffer buffMsg = CryptographicBuffer.ConvertStringToBinary(strMsg, encoding);

            // Encrypt the message.
            IBuffer buffProtected = await Provider.ProtectAsync(buffMsg);

            // Execution of the SampleProtectAsync function resumes here
            // after the awaited task (Provider.ProtectAsync) completes.
            return buffProtected;
        }

        public static async Task<string> UnprotectAsync(IBuffer buffProtected, BinaryStringEncoding encoding)
        {
            // Create a DataProtectionProvider object.
            DataProtectionProvider Provider = new DataProtectionProvider();

            // Decrypt the protected message specified on input.
            IBuffer buffUnprotected = await Provider.UnprotectAsync(buffProtected);

            // Execution of the SampleUnprotectData method resumes here
            // after the awaited task (Provider.UnprotectAsync) completes
            // Convert the unprotected message from an IBuffer object to a string.
            string strClearText = CryptographicBuffer.ConvertBinaryToString(encoding, buffUnprotected);

            // Return the plaintext string.
            return strClearText;
        }
    }
}