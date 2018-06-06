using Server.FileWork;
using Server.App_Data;
using Server.Registration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Server.Security;
using System.Security.Cryptography;

//Data Source = (LocalDB)\MSSQLLocalDB;AttachDbFilename=d:\GitHub\ServerForFileStorage\Server\Server\App_Data\Database.mdf;Integrated Security = True

namespace Server
{
    partial class Client: Commands
    {
        private const uint message_length = sizeof(ulong);  //Длина сообщения, посылаемого пользователем
        private const uint command_length = 1;              //Команда, посылаемая пользователем
        private NetworkStream networkStream;                //Сетевой поток с клиетом
        private User user_inf = null;                       //Информация о пользователе
        private TcpClient client;                           //Сокет клиента
        private byte[] response_buf = null;                 //Поток байтов для передачи клиенту 
        private bool flag = false;                          //Прошёл ли пользователь авторизацию
        private string folder = null;                       //Папка клиента, в которой хранятся файлы
        private DIFFIE_HELMAN aes = null;               //Объект защиты данных
        private NetworkFileWork file = new NetworkFileWork(); //Работа по передачи данных по сети

        //====================================================================================================
        //Регистрируем новый TCP-запрос
        public Client(TcpClient NewClient)
        {
            this.client = NewClient;
            this.networkStream = client.GetStream();
            //client.Client.LocalEndPoint;
        }
        //====================================================================================================
        public void Handler()
        {
            try
            {
                ClientCommands currCommand = GetCommand();
                switch (currCommand)
                {
                    case ClientCommands.AUTH:
                        {
                            GetClientInfHash(currCommand);
                            break;
                        }
                    case ClientCommands.REG:
                        {
                            GetClientInfHash(currCommand);
                            break;
                        }
                    case ClientCommands.LOAD:
                        {
                            GetClientInf(currCommand);
                            break;
                        }
                    case ClientCommands.SEND:
                        {
                            GetClientInf(currCommand);
                            break;
                        }
                    case ClientCommands.EX:
                        {
                            Disconnect();
                            break;
                        }
                }
            }
            catch (Exception error)
            {
                Console.Error.WriteLine(error.Message);
                Disconnect();
                return;
            }
        }

        public void GetClientInf(ClientCommands command)
        {
             byte[] length = new byte[sizeof(ulong)];
             networkStream.Read(length, 0, length.Length);

             byte[] message = new byte[BitConverter.ToUInt64(length, 0)];
             networkStream.Read(message, 0, message.Length);
             ProcessingRequest(command,message);
        }

        public void GetClientInfHash(ClientCommands command)
        {
            try
            {
                byte[] login_hash = null;
                FileProtocolReader.Read(ref login_hash, networkStream);

                byte[] password_hash = null;
                FileProtocolReader.Read(ref password_hash, networkStream);

                ToUserHash(command,login_hash, password_hash);
            }
            catch (Exception error)
            {
                Console.Error.WriteLine(error.Message);
                Disconnect();
                return;
            }
        }
        //====================================================================================================
        //Услуга, запрашиваемая клиентом
        public ClientCommands GetCommand()
        {
            ClientCommands currCommand = ClientCommands.EX;
            try
            {
                byte[] command = new byte[Client.command_length];
                networkStream.Read(command, 0, 1);
                currCommand = (ClientCommands)command[0];
            }
            catch (Exception error)
            {
                Console.Error.WriteLine(error.Message);
            }
            return currCommand;
        }
        //====================================================================================================
        //Конвертирует байты в объект User
        public void ToUserHash(ClientCommands recieve_command, byte[] login, byte[] password)
        {
            this.user_inf = new User(login, password);
            switch (recieve_command)
            {
                case ClientCommands.AUTH:
                    {
                        AUTH_Send_Message();
                        break;
                    }
                case ClientCommands.REG:
                    {
                        REG_Send_Message();
                        break;
                    }
                default:
                    Disconnect();
                    break;
            }
        }
        //====================================================================================================
        //Вызывает метод обраблтки пользовательского запроса 
        private void ProcessingRequest(ClientCommands recieve_command, byte[] message)
        {
            switch (recieve_command)
            {
                case ClientCommands.LOAD:
                    {
                        LOAD_Send_Message(message);
                        break;
                    }
                case ClientCommands.SEND:
                    {
                        SEND_Send_Message(message);
                        break;
                    }
            }
        }
        //====================================================================================================
        //Проверяем поток на наличие данных
        public void Listener()
        {
            while (client.Connected)
            {
                try
                {
                    if (client.Available != 0) //Есть ли в потоке сокета данные 0 - нет, > 0 есть
                    {
                        Handler(); //Обрабатываем клиентский запрос
                    }
                }
                catch (Exception error)
                {
                    Console.Error.WriteLine(error.Message);
                    Disconnect();
                }
            }
        }
        //====================================================================================================
        //Ответ на попытку авторизации
        private void AUTH_Send_Message()
        {
            this.response_buf = new byte[Client.command_length];
            if (UsersData.IsUserExist(ref user_inf, ref Server.UserList))
            {
                this.flag = true;                                      //Взаимодействие с пользователем активировано
                this.folder = Encoding.Default.GetString(user_inf.login_hash) + "//";   //Задаём персональное имя папки пользователя

                //Отправляем ответ об успешной авторизации
                this.response_buf[0] = Convert.ToByte(ServerAnswers.OK);        
                networkStream.Write(response_buf, 0, response_buf.Length);

                //Обмениваемся ключами и создаём секретный
                aes = new DIFFIE_HELMAN();                                 
                file.KeysExchange(client, networkStream,aes);

                //Ждать зашифрованные хэши
                while (client.Connected)
                {
                    if (networkStream.DataAvailable)
                    {
                        byte[] login = null;
                        FileProtocolReader.Read(ref login, networkStream);
                        login = aes.Decript(login);

                        byte[] password = null;
                        FileProtocolReader.Read(ref password, networkStream);
                        password = aes.Decript(password);

                        SHA256 sHA = SHA256Managed.Create();

                        if (sHA.ComputeHash(login).ToString() == user_inf.login_hash.ToString() &&
                            sHA.ComputeHash(password).ToString() == user_inf.login_hash.ToString())
                        {
                            this.response_buf[0] = Convert.ToByte(ServerAnswers.OK);
                            networkStream.Write(response_buf, 0, response_buf.Length);

                            //Посылаем информацию о названиях всех доступных файлах
                            FileHierarchy fileHierarchy = new FileHierarchy(this.folder);
                            fileHierarchy.NetworkSendInfo(networkStream,aes); 
                        }
                        else
                        {
                            this.response_buf[0] = Convert.ToByte(ServerAnswers.NOPE);
                            networkStream.Write(response_buf, 0, response_buf.Length);
                        }
                        break;
                    }
                }
            }
            else
            {
                this.response_buf[0] = Convert.ToByte(ServerAnswers.NOPE);
                networkStream.Write(response_buf, 0, response_buf.Length);
                Disconnect();
            }
        }
        //====================================================================================================
        //Ответ на попытку регистрации
        private void REG_Send_Message()
        {
            this.response_buf = new byte[Client.command_length];
            if (UsersData.IsUserExist(ref user_inf, ref Server.UserList))
            {
                this.response_buf[0] = Convert.ToByte(ServerAnswers.NOPE); //Такой пользователь существует
            }
            else
            {
                user_inf.key = RandomKey.GetKey(8);
                Server.UserList.Add(user_inf);                           //Пополняем список новым пользователем
                UsersData.WriteUsersInf(Server.users_inf_way,Server.UserList);
                this.folder = Encoding.Default.GetString(user_inf.login_hash) + "//";
                Directory.CreateDirectory(this.folder);
                this.response_buf[0] = Convert.ToByte(ServerAnswers.OK); //Регистрания пройдена успешно
            }
            networkStream.Write(response_buf, 0, response_buf.Length);
        }
        //====================================================================================================
        //Отправляем ответ файл
        private void SEND_Send_Message(byte[] data)
        {
            if (!flag)
            {
                Disconnect();
                return;
            }

            this.response_buf = new byte[Client.command_length];

            if (NetworkFileWork.IsFileExist(this.folder+Encoding.UTF8.GetString(data)))
            {
                this.response_buf[0] = Convert.ToByte(ServerAnswers.OK);
                networkStream.Write(response_buf, 0, response_buf.Length);
                file.SecurityLoadAndSend(this.folder+Encoding.UTF8.GetString(data),networkStream, user_inf.key, aes); //Загружаем файл
            }
            else //Файл не существует
            {
                this.response_buf[0] = Convert.ToByte(ServerAnswers.NOPE);
                networkStream.Write(response_buf, 0, response_buf.Length);
            }
        }
        //====================================================================================================
        //Записываем файл
        private void LOAD_Send_Message(byte[] data)
        {

            if (!flag)
            {
                Disconnect();
                return;
            }

            this.response_buf = new byte[Client.command_length];

            data = aes.Decript(data);
            if (!NetworkFileWork.IsFileExist(this.folder+Encoding.Default.GetString(data)))
            {
                file.SecurityLoadAndSave(this.folder+Encoding.Default.GetString(data), networkStream, user_inf.key, aes); //Загружаем файл
                this.response_buf[0] = Convert.ToByte(ServerAnswers.OK);
                networkStream.Write(response_buf, 0, response_buf.Length);
            }
            else //Файл существует
            {
                file.FastFlush(networkStream);
                this.response_buf[0] = Convert.ToByte(ServerAnswers.NOPE);
                networkStream.Write(response_buf, 0, response_buf.Length);
            }
        }
        //====================================================================================================
        //Разрываем сеанс с клиентом
        private void Disconnect()
        {
            if (client.Connected)
            {
                Console.WriteLine(client.Client.RemoteEndPoint.ToString() + "severed connection.");
                client.Close();
            }
            Server.ClientList.Remove(this);
        }
    }
}
