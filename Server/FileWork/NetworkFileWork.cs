using Server.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.FileWork
{
    class NetworkFileWork
    {
        private const int block_size = 1024; 
        public byte[] buf = new byte[block_size];
        private FileStream stream;
        public byte[] key = null;

        public byte[] file_length = new byte[FileProtocol.message_length];

        public void SecurityLoadAndSend(string file_name, NetworkStream networkStream, byte[] key, DIFFIE_HELMAN aes)
        {
            using (stream = new FileStream(file_name, FileMode.Open))
            {
                long length = stream.Length;
                int read = 0;
                file_length = BitConverter.GetBytes(stream.Length);
                aes.Encrypt(this.file_length);                      //Шифруем длину данных
                networkStream.Write(file_length, 0, file_length.Length);
                
                while (length > 0)
                {
                    read = stream.Read(buf, 0, buf.Length);
                    this.buf = StreamEncryptor.Encrypt(this.buf, key); //Дешифруем данные ключём потокового шифратора
                    aes.Encrypt(this.buf);                         //Шифруем данные для передачи по сети
                    networkStream.Write(this.buf, 0, read);
                    length -= read;
                }
            }
        }
        
        public void SecurityLoadAndSave(string file_name, NetworkStream networkStream, byte[] key, DIFFIE_HELMAN aes)
        {
            using (stream = new FileStream(file_name, FileMode.CreateNew))
            {
                /*networkStream.Read(file_length, 0, file_length.Length); //Считываем  длину данных в потоке
                long length = BitConverter.ToInt64(file_length, 0);     //Переводим длину*/

                byte[] file_data = null;
                FileProtocolReader.Read(ref file_data,networkStream);
                file_data = aes.Decript(file_data);
                this.buf = StreamEncryptor.Encrypt(file_data, key);   //Шифруем потоковым шифратором
                stream.Write(buf, 0, buf.Length); //Записываем данные в файл

                /*int read = 0;
                while (networkStream.DataAvailable)
                {
                    read = networkStream.Read(this.buf, 0, buf.Length); //Считываем данные из сетевого потока
                    aes.Decript(this.buf);                          //Дешифруем данные из сети
                    this.buf = StreamEncryptor.Encrypt(this.buf,key);   //Шифруем потоковым шифратором
                    stream.Write(buf, 0, read); //Записываем данные в файл
                    length -= read;
                }*/
            }
        }

        public void FastFlush(NetworkStream networkStream)
        {
            networkStream.Read(file_length, 0, file_length.Length); //Считываем  длину данных в потоке
            long length = BitConverter.ToInt64(file_length, 0);              //Переводим длину

            int read = 0;
            while (length > 0)
            {
                read = networkStream.Read(this.buf, 0, buf.Length);
                length -= read;
            }
        }

        public static bool IsFileExist(string file_name)
        {
            FileInfo file = new FileInfo(file_name);
            if (file.Exists)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void KeysExchange(TcpClient client, NetworkStream networkStream, DIFFIE_HELMAN aes)
        {
            //Отправляем свой ключ
            FileProtocolReader.Write(aes.PublicKey, networkStream);
            FileProtocolReader.Write(aes.IV, networkStream);
            while (client.Connected)
            {
                if (networkStream.DataAvailable)
                {
                   byte[] client_key = null;
                   FileProtocolReader.Read(ref client_key, networkStream);
                   aes.CreateSecretKey(client_key);
                   return;
                }
            }
        }
    }
}
