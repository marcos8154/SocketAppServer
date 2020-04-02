# SocketAppServer
A simple, lightweight and fast MVC-like Socket server

This framework will allow you to create a server that makes it easy to display data on intranet networks (and also the internet). The server will work in an "embedded" manner, with the premise of rapid deployment and startup in a production environment, without the need to configure application servers such as IIS or Apache.

The main communication of the server is based on Socket TCP / IP, which requires a specific client to connect and consume it. But if this is a flaw, the framework also allows to enable HTTP communication, which facilitates and expands the integration with a greater variety of clients (including mobile devices)

**How to Setup**

First, install the framework into your project through Nuget by searching for **"MobileAppServer"**, or by running the following command at the Visual Studio prompt: **Install-Package MobileAppServer -Version 1.6.0** (or higher)

The framework will add **NewtonSoft.Json** together

Now let's implement a basic code that makes our server startup

```C#
        public static void Main(string[] args)
        {
            //create server
            Server server = new Server();

            //set port
            server.Port = 4590;

            //set Threads Limit; default value is 999999 threads
            server.MaxThreadsCount = 4;

            //define if server is Single-Threaded; if true, MaxThreadsCount is ignored
            server.IsSingleThreaded = false;

            //Buffer-Size output for server
            server.BufferSize = 4096;

            //server global encoding for requests and responses
            server.ServerEncoding = Encoding.UTF8;

            //register you server controllers
            server.RegisterAllControllers(Assembly.GetExecutingAssembly(), "FullNamespaceNameForControllers");

            //register you server entities/models
            server.RegisterAllModels(Assembly.GetExecutingAssembly(), "FullNamespaceNameForModels");

            //start server :D
            server.Start();
        }
```

**Implementing actions on your server**

Now that your server is properly mapped and configured, the next step is the most fun: D
Let's create classes for our Controllers, implementing the IContoller interface, and creating the methods we want to expose.
Methods must return an instance of ActionResult

Below is an example of how to implement an action with simple parameters, and another with parameters of complex types:

```C#
    public  class ProductController : IController
    {
        //This is a very simple action whose parameters are either primitive or basic C # types
        public ActionResult SimpleAction(int param1, string arg2, decimal param3)
        {
            //... do things...

            var product = new Product
            {
                Name = "Product Name",
                Price = 500
            };
            return ActionResult.Json(product);
        }

        //Here we are getting a product instance in our action
        public ActionResult ComplexAction(Product product, string otherParam)
        {
            if (product.Price > 200)
                return ActionResult.Json(product, ResponseStatus.ERROR, "Invalid product");
            else
                return ActionResult.Json(true);
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
"Controller" : "ProductController",
"Action" : "SaveProduct",
"Parameters" : [
	{ "Name" : "product.Name", "Value" : "Notebook DELL XPS" },
	{ "Name" : "product.Price", "Value" : "1,999.90" },
	{ "Name" : "otherParam", "Value" : "Another param to action" }
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

            //creating request with parameters
            RequestBody rb = RequestBody.Create("ProductController", "SaveProduct")
                .AddParameter("product.Name", "Notebook DELL XPS")
                .AddParameter("product.Price", "1,999.90")
                .AddParameter("otherParam", "Another param to action");

            //submit request to server
            client.SendRequest(rb);

            //get response and closes connection
            client.GetResult();
```

If your server has actions that return objects, you can easily convert them on the client side:

```C#
            List<Product> myProducts = (List<Product>) client.GetResult(typeof(List<Product>)).Entity;
```

If you want to enable HTTP communication on your server, so you don't need to use the specific client, see the step-by-step here https://github.com/marcos8154/SocketAppServer/wiki/Enable-HTTP-communication-on-your-server
