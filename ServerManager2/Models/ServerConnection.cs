using Newtonsoft.Json;
using ServerManager2.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using static System.Environment;

namespace ServerManager2.Models
{
    public class ServerConnection
    {
        public ServerConnection(string alias, string address,
            int port)
        {
            Alias = alias;
            Address = address;
            Port = port;
        }

        public string Alias { get; private set; }
        public string Address { get; private set; }
        public int Port { get; private set; }

        public void ChangeAlias(string newAlias)
        {
            if (string.IsNullOrEmpty(newAlias))
                throw new Exception("New alias is empty");
            if (newAlias.Equals(Alias))
                throw new Exception("New alias not changed");
            Alias = newAlias;
        }

        public bool RequiresAuthentication { get; private set; }
        public string UserName { get; private set; }
        public string Password { get; private set; }

        public void SetAuthentication(string userName, string password)
        {
            RequiresAuthentication = true;
            UserName = userName.Criptografar();
            Password = userName.Criptografar();
        }

        public string RevealUserName()
        {
            if (!RequiresAuthentication)
                return string.Empty;
            return UserName.Descriptografar();
        }

        public string RevealPassword()
        {
            if (!RequiresAuthentication)
                return string.Empty;
            return Password.Descriptografar();
        }

        public static string GetAppDataFolder()
        {
            var appData = GetFolderPath(SpecialFolder.ApplicationData);
            appData += @"\ServerManager2\";
            if (!Directory.Exists(appData))
                Directory.CreateDirectory(appData);
            return appData;
        }

        public void SaveChanges()
        {
            using (FileStream fs = new FileStream($@"{GetAppDataFolder()}\{Alias}.dat", FileMode.OpenOrCreate))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, this);
            }
        }

        public static ServerConnection Load(string alias)
        {
            string file = $@"{GetAppDataFolder()}\{alias}.dat";
            using (FileStream fs = new FileStream(file, FileMode.Open))
            {
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    ServerConnection conn = (ServerConnection)formatter.Deserialize(fs);
                    return conn;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Cannot be open connection data file: {ex.Message}");
                }
            }
        }
    }
}
