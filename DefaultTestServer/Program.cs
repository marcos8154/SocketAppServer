using Newtonsoft.Json;
using SocketAppServer.CoreServices;
using SocketAppServer.CoreServices.CoreServer;
using SocketAppServer.Extensions.ClientMaker;
using SocketAppServer.Hosting;
using SocketAppServer.ManagedServices;
using SocketAppServer.Security;
using SocketAppServer.ServerObjects;
using SocketAppServer.TelemetryServices;
using SocketAppServerClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace DefaultTestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
                Startup.serverPort = 6000;
            else
                Startup.serverPort = int.Parse(args[0]);
            /*
            string ipAddress = "192.168.15.11";

            SocketClientSettings settings = new SocketClientSettings(ipAddress,
                4050, Encoding.UTF8, 3, 1000);

            SocketConnectionFactory.SetDefaultSettings(settings);

            using (ISocketClientConnection conn = SocketConnectionFactory.GetConnection())
            {

            }
            */

            Console.ForegroundColor = ConsoleColor.White;
            SocketServerHost.CreateHostBuilder()
                   .UseStartup<Startup>()
                   .Run();
        }



        public class UserRepository : IServerUserRepository
        {
            public ServerUser Authenticate(string userNameOrEmail, string password)
            {
                return new ServerUser("1235", "UserName", "email@provider.com", "Organization Test Name");
            }

            public void OnSuccessFulAuthentication(string token)
            {
            //    throw new NotImplementedException();
            }
        }


        public class CustomerController : IController
        {
            private static List<string> customers;

            public CustomerController()
            {
                if (customers == null)
                {
                    customers = new List<string>();
                }
            }

            [ServerAction]
            public void AddCustomer(string customer)
            {
                Thread.Sleep(1000);
                customers.Add(customer);
                Console.WriteLine($"Added customer. Count: {customers.Count}");
            }
        }
    }

    public class Info
    {
        public string InfoName { get; set; }

        public Customer Customer { get; set; }

        public Info(string infoName, Customer customer)
        {
            InfoName = infoName;
            Customer = customer;
        }
    }

    public class Customer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public Info Info { get; set; }

        public Customer(string name, string phone,
            Info info)
        {
            Id = Guid.NewGuid();
            Name = name;
            Phone = phone;
            Info = info;
        }
    }
}
