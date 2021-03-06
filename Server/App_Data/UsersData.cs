﻿using Newtonsoft.Json;
using Server.FileWork;
using Server.Registration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.App_Data
{
    static class UsersData
    {
        public static void ReadUsersInf(ref string users_inf_way, ref List<User> UserList)
        {
            try
            {
                if (NetworkFileWork.IsFileExist(users_inf_way))
                {
                    UserList = JsonConvert.DeserializeObject<List<User>>(File.ReadAllText(users_inf_way));
                }
                else
                {
                    users_inf_way = "users.json";
                    FileInfo file = new FileInfo(users_inf_way);
                    file.Create();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.Out.Flush();
                Console.ReadKey();
                Environment.Exit(ex.HResult);
            }

        }

        public static void WriteUsersInf(string users_inf_way, object UserList)
        {
            File.WriteAllText(users_inf_way, JsonConvert.SerializeObject(UserList));
        }

        public static bool IsUserExist(ref User user, ref List<User> UserList)
        {
            if (UserList != null)
            {
                foreach (User u in UserList)
                {
                    if (Encoding.Default.GetString(u.login_hash) == Encoding.Default.GetString(user.login_hash) && Encoding.Default.GetString(u.password_hash) == Encoding.Default.GetString(user.password_hash))
                    {
                        user.key = u.key; 
                        return true;
                    }
                }
                return false;
            }
            else
            {
                UserList = new List<User>();
                return false;
            }
        }
    }
}
