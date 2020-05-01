# SocketAppServer
A simple, extensible, lightweight and fast MVC-like Socket server

This framework will allow you to create a server that makes it easy to display data on intranet networks (and also the internet). The server will work in an "embedded" manner, with the premise of rapid deployment and startup in a production environment, without the need to configure application servers such as IIS or Apache.

The main communication of the server is based on Socket TCP / IP, which requires a specific client to connect and consume it. But if this is a flaw, the framework also allows to enable HTTP communication, which facilitates and expands the integration with a greater variety of clients (including mobile devices)

For more informations and tutorials, see project Wiki here: https://github.com/marcos8154/SocketAppServer/wiki

**How to Setup**

First, install the framework into your project through Nuget by searching for **"MobileAppServer"**, or by running the following command at the Visual Studio prompt: **Install-Package MobileAppServer -Version 1.6.0** (or higher)

The framework will add **NewtonSoft.Json** together

Now let's implement a basic code that makes our server startup

1 - Create a class in your project and name it Startup.cs, inheriting the AppServerConfigurator class
```C#
        public class Startup : AppServerConfigurator
        {
            public override void ConfigureServices(IServiceManager serviceManager)
            {
                //Here, we will enable and configure 
                //services and server modules.
                //More details on the Wiki project
                RegisterController(typeof(DeviceController));
            }

            public override ServerConfiguration GetServerConfiguration()
            {
	        //Here, we must return the object that contains
                //the server's operating parameters, such as port, 
                //Encoding, buffer and connection limit
                return new ServerConfiguration(Encoding.UTF8,
                         5000, 1024 * 100, false, 100, true);
            }
        }
```

2 - In the Main method, create a Host for the server, pointing the Startup class as a startup provider

```C#
   static void Main(string[] args)
   {
        SocketServerHost.CreateHostBuilder()
               .UseStartup<Startup>()
               .Run();
   }
```

3 - Run the project and your server will be live :)

![](https://raw.githubusercontent.com/marcos8154/SocketAppServer/master/CLI.png)

**Implementing actions on your server**

Now that your server is properly mapped and configured, the next step is the most fun: D
Let's create classes for our Controllers, implementing the IContoller interface, and creating the methods we want to expose.
Actions methods must have the annotation/attribute **[ServerAction]**

Below is an example of how to implement an action with simple parameters, and another with parameters of complex types:

```C#
        public class DeviceController : IController
        {
            [ServerAction]
            public void RegisterDevice(CustomerDevice device) //action with Complex type as paramneter
            {
                using (CustomerRepository repository = new CustomerRepository())
                {
                    IServiceManager services = ServiceManager.GetInstance();
                    ILoggingService log = services.GetService<ILoggingService>();

                    log.WriteLog($"Device registered");

                    repository.RegisterDevice(device);
                }
            }

            [ServerAction]
            public List<Customer> SearchCustomers(string search) //action with simple type as parameter
            {
                using(CustomerRepository repository = new CustomerRepository())
                {
                    return repository.SearchCustomers(search);
                }
            }
        }
```

**Making calls to your server**

In addition to responding by default in JSON, the server also receives requests through JSON syntax.
The calling commands are simple and clear and can be executed from any client-socket program.

Take this ProductController action as an example:

```C#
public ActionResult SaveProduct(Product product, string oltherParam)
{
    .......
}
```
For the above action we will have the following request syntax sent by the client:

```JSON
"Controller" : "DeviceController",
"Action" : "RegisterDevice",
"Parameters" : [
	{ "Name" : "device", "Value" : "{ CustomerDevice JSON object HERE }" }
]
```

**Making requests via the server's default client library**
The framework has a standard lib client written for it, and you can find it through Nuget:
"Install-Package MobileAppServer.Client -Version 1.2.0" | or higher

Having it installed, you can submit requests for the same action example as follows:

```C#
	    using MobileAppServerClient;
	    
	    .....

            //default config for client
            //necessary only on app startup
            Client.Configure("serveraddress", 5000, 200000);

            //Instantiating the client. 
            //This will already result in an open connection on the server.
            Client client = new Client();

            CustomerDevice deviceObj = new CustomerDevice();
	    deviceObj.CustomerId = 8586965666;
	    deviceObj.DeviceName = "Customer Device Example Name";
	    deviceObj.Serial = "ETL-PX00014185D9"

            //creating request with parameters
            RequestBody rb = RequestBody.Create("ProductController", "SaveProduct")
                .AddParameter("device", deviceObj);

            //submit request to server
            client.SendRequest(rb);

            //get response and closes connection
            client.GetResult();
```

If your server has actions that return objects, you can easily convert them on the client side:

```C#
            List<CustomerDevice> devices = client.GetResult<List<CustomerDevice>>();
```

If you want to enable HTTP communication on your server, so you don't need to use the specific client, see the step-by-step here https://github.com/marcos8154/SocketAppServer/wiki/Enable-HTTP-communication-on-your-server
