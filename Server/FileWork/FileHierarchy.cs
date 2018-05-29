using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Net.Sockets;
using Server.Security;

namespace Server.FileWork
{
    
    class FileHierarchyInfo
    {
        public string[] data { get; set; }
    }

    class FileHierarchy
    {
        public byte[] file_length = new byte[FileProtocol.message_length];
        private string dir_name;

        public FileHierarchy(string name)
        {
            this.dir_name = name;
        }

        private string GetInfo()
        {
            FileHierarchyInfo send_hierarhy = new FileHierarchyInfo();
            send_hierarhy.data = Directory.GetFiles(this.dir_name); //Получаем информацию о файлах
            return JsonConvert.SerializeObject(send_hierarhy);
        }

        public void NetworkSendInfo(NetworkStream stream, AES_DiffieHellman aes)
        {
            string hierarhy = GetInfo();
            byte[] arr = new byte[hierarhy.Length];
            arr = Encoding.Default.GetBytes(hierarhy);
            aes.Encrypt(ref arr);
            file_length = BitConverter.GetBytes(arr.LongLength);
            stream.Write(file_length, 0, file_length.Length); //Длина сериализованных даных
            stream.Write(arr,0,arr.Length); //Сериализованные данные
        }
    }
}
