using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Security
{
    static class StreamEncryptor
    {
        public static byte[] Encrypt(byte[] plainText, byte[] key)
        {
            int j = 0;
            for (int i = 0; i < plainText.Length; i++)
            {
                plainText[i] = (byte)(plainText[i] ^ key[j]);
                j++;
                if (j == key.Length)
                {
                    j = 0;
                }
            }
            return plainText;
        }
    }
}
