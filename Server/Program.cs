using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        private static Thread ServerThread;
        private static string LocalGroup = "228.228.228.228";

        public static string[] ServerManage = { "CUSTOMERS", "INFO", "HELP", "EXIT" };
        /* CUSTOMERS - all customers that has been connected
         * INFO - information about program
         * HELP - short description about server owner commands
         * EXIT - end of server work 
         */

        static void Main(string[] args)
        {
            ServerThread = new Thread(startServer);
            ServerThread.IsBackground = true;
            ServerThread.Start();

            ServerThread = new Thread(UdpListener);
            ServerThread.IsBackground = true;
            ServerThread.Start();

            Server server = new Server(8080);
            server.Listener();
        }

        private static void startServer()
        {
            string command = string.Empty;
            do
            {
                command = Console.ReadLine();
                switch (command)
                {
                    case "INFO": break;
                    case "HELP": 
                        {
                            Console.WriteLine("COMMANDS");
                            Console.WriteLine(ServerManage);
                            break;
                        }
                    case "CUSTOMERS":
                        {
                            Console.WriteLine(Server.ClientList);
                            break;
                        }
                }
            } while (command != "EXIT");
            Environment.Exit(0);
        }

        private async static void UdpListener()
        {
            /*string Host = Dns.GetHostName();
            IPAddress my_ip = Dns.GetHostByName(Host).AddressList[0];
            IPEndPoint iPEnd = new IPEndPoint(my_ip, 8081);*/

            UdpClient udp = new UdpClient(8081);
            udp.JoinMulticastGroup(IPAddress.Parse(LocalGroup),1);
            udp.MulticastLoopback = false;
            udp.EnableBroadcast = true;
            

            string IP = Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString();
            byte[] datagram = Encoding.Default.GetBytes("Server-"+IP);
            byte[] mes = Encoding.Default.GetBytes("Client");

            IPEndPoint iPEnd = null;
            while (true)
            {
                UdpReceiveResult result = await udp.ReceiveAsync();
                Console.WriteLine(result.Buffer);
                if (result.Buffer == mes)
                {
                    await udp.SendAsync(datagram,datagram.Length);
                }
                /*byte[] data = udp.Receive(ref iPEnd);
                string message = Encoding.Unicode.GetString(data);
                Console.WriteLine(message);*/
            }
        }
    }
}
