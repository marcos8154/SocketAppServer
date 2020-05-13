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
                //Here, we will enable and configure 
                //services and server modules.
                //More details on the Wiki project
                RegisterController(typeof(DeviceController));
                DisableStatisticsComputing();
            }

            public override ServerConfiguration GetServerConfiguration()
            {
                //Here, we must return the object that contains
                //the server's operating parameters, such as port, 
                //Encoding, buffer and connection limit
                return new ServerConfiguration(Encoding.UTF8,
                         5000, 1024000 * 10, false, 100, true);
            }
        }

        public class UserRepository : IServerUserRepository
        {
            public ServerUser Authenticate(string userNameOrEmail, string password)
            {
                return new ServerUser("1235", "UserName", "email@provider.com", "Organization Test Name");
            }
        }

        public class DeviceController : IController
        {
            static List<Customer> result = new List<Customer>();
            [ServerAction]
            public List<Customer> SearchCustomers()
            {
                if (result.Count > 0)
                    return result;

                for (int i = 0; i < 95000; i++)
                    result.Add(new Customer($"Customer {i + 1}", "24 99856865"));
                return result;
            }

            [ServerAction]
            public string BigString()
            {
                string result = "";
                for (int i = 0; i < 1000; i++)
                    result += new Random(i).Next().ToString();
                return result;
            }
        }
    }

    public class Customer
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Phone { get; private set; }

        public Customer(string name, string phone)
        {
            Id = Guid.NewGuid();
            Name = name;
            Phone = phone;
        }
    }
}
