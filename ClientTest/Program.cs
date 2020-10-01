using Newtonsoft.Json;
using SocketAppServerClient;
using System;
using System.Diagnostics;
using System.Text;

namespace ClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                SocketConnectionFactory.SetDefaultSettings(new SocketClientSettings(
                        "localhost", 7001,
                        Encoding.UTF8, 3
                    ));

                using(ISocketClientConnection conn = SocketConnectionFactory.GetConnection())
                {

                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
            }
        }
    }

}
