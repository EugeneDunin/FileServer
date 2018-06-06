using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Numerics;
using System.IO;

namespace Server.Security
{
    //====================================================================================================
    //====================================================================================================

    class AES_DiffieHellman
    {
        public byte[] PublicKey { get; private set; } //Ключ, передаваемый по сети для последующего получения секретного
        private byte[] Key = null;
        private ECDiffieHellmanCng server = new ECDiffieHellmanCng();
        public Aes aes { get; private set; }
        private byte[] Client_IV;
        private byte[] Own_IV;

        public AES_DiffieHellman()
        {
            this.aes = new AesCryptoServiceProvider();
            Own_IV = this.aes.IV;

            server.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
            server.HashAlgorithm = CngAlgorithm.Sha256;
            this.PublicKey = server.PublicKey.ToByteArray();
            Console.WriteLine(server.Key.KeySize.ToString());

            //this.Key = server.DeriveKeyMaterial(CngKey.Import(RandomKey.GetKey(140), CngKeyBlobFormat.EccPublicBlob));
        }
       

        public void GetSecretKey(byte[] ClientPublicKey, byte[] iv)
        {

            /*using (this.server)
            {
                CngKey k = CngKey.Import(ClientPublicKey, CngKeyBlobFormat.EccPublicBlob);
                this.Key = server.DeriveKeyMaterial(CngKey.Import(ClientPublicKey, CngKeyBlobFormat.EccPublicBlob));
                Client_IV = iv;
            }*/
            try
            {
                Console.WriteLine(server.Key);
                CngKey k = CngKey.Import(ClientPublicKey, CngKeyBlobFormat.GenericPrivateBlob);
                this.Key = server.DeriveKeyMaterial(CngKey.Import(ClientPublicKey, CngKeyBlobFormat.EccPublicBlob));
                Client_IV = iv;
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public byte[] Encrypt(byte[] data)
        {
            using (this.aes)
            {
                aes.Key = this.Key;     //Устанавливаем ключ
                aes.IV = Own_IV;

                using (MemoryStream ciphertext = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ciphertext, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.Close();
                        return ciphertext.ToArray();
                    }
                }

            }
        }

        public byte[] Decript(byte[] data)
        {
            using (this.aes)
            {
                aes.Key = Key;
                aes.IV = Client_IV;

                using (MemoryStream plaintext = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(plaintext, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.Close();
                        /*string message = Encoding.UTF8.GetString(plaintext.ToArray());
                        Console.WriteLine(message);*/
                        return plaintext.ToArray();
                    }
                }
            }
        }
    }
}
