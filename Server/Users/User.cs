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
        public string login { get; set; }
        public string password { get; set; }
        public byte[] key { get; set; }

        public User(string login, string password)
        { 
            this.login = login;
            this.password = password;
            this.key = RandomKey.GetKey(8);
        }
    }
}
