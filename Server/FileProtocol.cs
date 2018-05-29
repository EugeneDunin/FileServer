using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public static class FileProtocol
    {
        public const uint message_length = sizeof(ulong);
    }
}
