using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    static class FileProtocolReader
    {
        public static void Read(ref byte[] buf,NetworkStream net)
        {
            byte[] length = new byte[sizeof(ulong)];
            net.Read(length, 0, length.Length);

            buf = new byte[BitConverter.ToUInt64(length, 0)];
            net.Read(buf, 0, buf.Length);
        }
    }
}
