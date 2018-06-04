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
    class RSASecurity1
    {
        private static BigInteger g; //Открытая часть для вычисления ключа
        private static BigInteger p; //Открытая часть для вычисления ключа
        private static BigInteger b; //Секретный ключ
        private static BigInteger B; //Пересылаемое звено ключ

        static public byte[] RSAEncrypt(byte[] DataToEncrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                byte[] encryptedData;
                //RSAParameters RSAKeyInfo = new RSAParameters();
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {

                    //Import the RSA Key information. This only needs
                    //toinclude the public key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    //Encrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    encryptedData = RSA.Encrypt(DataToEncrypt, DoOAEPPadding);
                }
                return encryptedData;
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return null;
            }

        }

        static public byte[] RSADecrypt(byte[] DataToDecrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            try
            {
                byte[] decryptedData;
                //Create a new instance of RSACryptoServiceProvider.
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    //Import the RSA Key information. This needs
                    //to include the private key information.
                    RSA.ImportParameters(RSAKeyInfo);

                    //Decrypt the passed byte array and specify OAEP padding.  
                    //OAEP padding is only available on Microsoft Windows XP or
                    //later.  
                    decryptedData = RSA.Decrypt(DataToDecrypt, DoOAEPPadding);
                }
                return decryptedData;
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.ToString());

                return null;
            }

        }

        /*public RSASecurity()
        {
            g = new BigInteger(RandomKey.GetKey(8));
            p = new BigInteger(RandomKey.GetKey(64));
            b = new BigInteger(RandomKey.GetKey(64));
            ECDiffieHellmanCng diffieHellmanCng = new ECDiffieHellmanCng();
            ECDiffieHellmanPublicKey f = diffieHellmanCng.PublicKey;
        }*/
    }

    /*class Alice
    {
        public static byte[] alicePublicKey;

        public static void Main(string[] args)
        {
            using (ECDiffieHellmanCng alice = new ECDiffieHellmanCng())
            {

                alice.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                alice.HashAlgorithm = CngAlgorithm.Sha256;
                alicePublicKey = alice.PublicKey.ToByteArray();
                Bob bob = new Bob();
                CngKey k = CngKey.Import(bob.bobPublicKey, CngKeyBlobFormat.EccPublicBlob);
                byte[] aliceKey = alice.DeriveKeyMaterial(CngKey.Import(bob.bobPublicKey, CngKeyBlobFormat.EccPublicBlob));
                byte[] encryptedMessage = null;
                byte[] iv = null;
                Send(aliceKey, "Secret message", out encryptedMessage, out iv);
                bob.Receive(encryptedMessage, iv);
            }

        }

        private static void Send(byte[] key, string secretMessage, out byte[] encryptedMessage, out byte[] iv)
        {
            using (Aes aes = new AesCryptoServiceProvider())
            {
                aes.Key = key;
                iv = aes.IV;

                // Encrypt the message
                using (MemoryStream ciphertext = new MemoryStream())
                using (CryptoStream cs = new CryptoStream(ciphertext, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    byte[] plaintextMessage = Encoding.UTF8.GetBytes(secretMessage);
                    cs.Write(plaintextMessage, 0, plaintextMessage.Length);
                    cs.Close();
                    encryptedMessage = ciphertext.ToArray();
                }
            }
        }

    }*/
    /*public class Bob
    {
        public byte[] bobPublicKey;
        private byte[] bobKey;
        public Bob()
        {
            using (ECDiffieHellmanCng bob = new ECDiffieHellmanCng())
            {

                bob.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                bob.HashAlgorithm = CngAlgorithm.Sha256;
                bobPublicKey = bob.PublicKey.ToByteArray();
                bobKey = bob.DeriveKeyMaterial(CngKey.Import(Alice.alicePublicKey, CngKeyBlobFormat.EccPublicBlob));

            }
        }

        public void Receive(byte[] encryptedMessage, byte[] iv)
        {

            using (Aes aes = new AesCryptoServiceProvider())
            {
                aes.Key = bobKey;
                aes.IV = iv;
                // Decrypt the message
                using (MemoryStream plaintext = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(plaintext, aes.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(encryptedMessage, 0, encryptedMessage.Length);
                        cs.Close();
                        string message = Encoding.UTF8.GetString(plaintext.ToArray());
                        Console.WriteLine(message);
                    }
                }
            }
        }

    }*/

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
                 CngKey k = CngKey.Import(ClientPublicKey, CngKeyBlobFormat.GenericPrivateBlob);
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

        public void Encrypt(ref byte[] data)
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
                        data = ciphertext.ToArray();
                    }
                }

            }
        }

        public void Decript(ref byte[] data)
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
                        data = plaintext.ToArray();
                    }
                }
            }
        }
    }
}
