using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Server.Security
{
    static class RandomKey
    {
        public static byte[] GetKey(ulong size)
        {
            byte[] key = new byte[size];
            Random rand = new Random();
            rand.NextBytes(key);
            return key;
        }
    }
}
