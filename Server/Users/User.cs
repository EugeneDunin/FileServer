using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Server.Security;

namespace Server.Registration
{
    public class User
    {
        public byte[] login_hash = new byte[32];
        public byte[] password_hash = new byte[32];
        //public string login { get; set; }
        //public string password { get; set; }
        public byte[] key { get; set; }

        /*public User(string login, string password)
        { 
            this.login = login;
            this.password = password;
            this.key = RandomKey.GetKey(8);
        }*/
        public User(byte[] login_hash, byte[] password_hash)
        {
            this.login_hash = login_hash;
            this.password_hash = password_hash;
        }
    }
}
