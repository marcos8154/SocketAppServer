using Newtonsoft.Json;
using SocketAppServer.CoreServices;
using SocketAppServer.CoreServices.CoreServer;
using SocketAppServer.Extensions.ClientMaker;
using SocketAppServer.Hosting;
using SocketAppServer.ManagedServices;
using SocketAppServer.Security;
using SocketAppServer.ServerObjects;
using SocketAppServer.TelemetryServices;
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
            Console.ForegroundColor = ConsoleColor.White;
            SocketServerHost.CreateHostBuilder()
                   .UseStartup<Startup>()
                   .Run();
        }

        public class Startup : AppServerConfigurator
        {
            public override void ConfigureServices(IServiceManager serviceManager)
            {
                DisableStatisticsComputing();
                DisableTelemetryServices();
                RegisterController(typeof(CustomerController));
                RegisterModel(typeof(Customer));
                RegisterModel(typeof(Info));
            }

            public override ServerConfiguration GetServerConfiguration()
            {
                //Here, we must return the object that contains
                //the server's operating parameters, such as port, 
                //Encoding, buffer and connection limit
                return new ServerConfiguration(Encoding.UTF8,
                         6000, 10000000, false, 100, true);
            }
        }


        public class UserRepository : IServerUserRepository
        {
            public ServerUser Authenticate(string userNameOrEmail, string password)
            {
                return new ServerUser("1235", "UserName", "email@provider.com", "Organization Test Name");
            }

            public void OnSuccessFulAuthentication(string token)
            {
                throw new NotImplementedException();
            }
        }


        public class CustomerController : IController
        {
            [ServerAction]
            public void AddCustomer(Customer customer)
            {

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
