using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace Server.Security
{
    class DIFFIE_HELMAN
    {
        private AesCryptoServiceProvider AES = new AesCryptoServiceProvider();
        public byte[] IV
        {
            get
            {
                return AES.IV;
            }
        }
        private ECDiffieHellmanCng keyManager = new ECDiffieHellmanCng();
        private byte[] publicKey = null;
        public byte[] PublicKey
        {
            get
            {
                return publicKey;
            }
            private set
            {
                publicKey = value;
            }
        }

        private byte[] secretKey = null;

        public DIFFIE_HELMAN()
        {
            keyManager.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
            keyManager.HashAlgorithm = CngAlgorithm.Sha256;
            publicKey = keyManager.PublicKey.ToByteArray();
        }

        public void CreateSecretKey(byte[] publicKey)
        {
            CngKey key = CngKey.Import(publicKey, CngKeyBlobFormat.EccPublicBlob);
            secretKey = keyManager.DeriveKeyMaterial(key);
            AES.Key = secretKey;
        }

        /*public void setIV(byte[] receivedIV)
        {
            AES.IV = receivedIV;
        }*/

        public byte[] Encrypt(byte[] message)
        { 
            using (MemoryStream ciphertext = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ciphertext, AES.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(message, 0, message.Length);
                cs.Close();
                return ciphertext.ToArray();
            }
        }

        public byte[] Decript(byte[] message)
        {
            // Decrypt the message
            using (MemoryStream plaintext = new MemoryStream())
            {
                //Console.WriteLine("Длина потока - " + plaintext.Length);
                //plaintext.SetLength(long.MaxValue);
                using (CryptoStream cs = new CryptoStream(plaintext, AES.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(message, 0, message.Length);
                    
                    //Console.WriteLine("Длина потока - "+cs.Length);
                    //cs.SetLength(long.MaxValue);
                    cs.Flush();
                    cs.Close();
                    
                }
                return plaintext.ToArray();
            }
        }
    }
}
