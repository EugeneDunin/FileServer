using Server.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;
using Server.FileWork;
using System.IO;
using Server.App_Data;

namespace Server
{
    class Server
    {
        public static List<Client> ClientList = new List<Client>();
        public static List<User> UserList = new List<User>();
        public static string users_inf_way = "users.json";
        private TcpListener ServerListener;

        public Server(int port)
        {
            UsersData.ReadUsersInf(ref users_inf_way, ref UserList);
            ServerListener = new TcpListener(IPAddress.Any, port);
            ServerListener.Start();
        }

        public void Listener()
        {
            while (true)
            {
                ServerListener.Pending();
                Client client = new Client(ServerListener.AcceptTcpClient());
                ClientList.Add(client);
                Thread clientThread = new Thread(new ThreadStart(client.Listener));
                clientThread.Start();
            }
        }

        ~Server()
        {
            if (ServerListener != null)
            {
                ServerListener.Stop();
            }
            UsersData.WriteUsersInf(users_inf_way,UserList);
        }
    }
}
