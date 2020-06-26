using Newtonsoft.Json;
using SocketAppServerClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //Configuração do Json em modo GLOBAL
        
            Client.Configure("localhost", 6000,
                Encoding.UTF8, 4096, 10, 0, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
        

            Customer ct = new Customer();
            ct.Info = new Info
            {
              //  Customer = ct
            };

            Client c = new Client();

            RequestBody rb = RequestBody.Create("CustomerController", "AddCustomer")
             .AddParameter("customer", ct);

            c.SendRequest(rb);
            c.GetResult();
        }
    }

    public class Info
    {
        public string InfoName { get; set; }

        public Customer Customer { get; set; }
    }

    public class Customer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public Info Info { get; set; }
    }
}
