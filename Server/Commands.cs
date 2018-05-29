using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public abstract class Commands
    {
        public enum ClientCommands { EX = -1, LOAD, SEND, REG, AUTH };
            /* EX - some exeeption, that server may repair
             * LOAD - save clien file
             * SEND - allow client to download his file
             * REG - request for a new user
             * AUTH - account request
             */
        public enum ServerAnswers { OK, NOPE };
            /* OK - server approved the request
             * NOPE - server disapproved request
             */
    }
}
