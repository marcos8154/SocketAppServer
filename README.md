# SocketAppServer
A simple, lightweight and fast MVC-like Socket server


**How to Setup**

First, install the framework into your project through Nuget by searching for **"MobileAppServer"**, or by running the following command at the Visual Studio prompt: **Install-Package MobileAppServer -Version 1.4.3** (or higher)

The framework will add **NewtonSoft.Json** together, and also create a folder called **"Mappings"** in your project.

The Mappings folder will contain all mapping XMLs for your server Controllers.

**ATTENTION:** *ALL XML's in this folder must be copied to the project binaries folder at compile time.
To do this, all XML files in the folder must be marked **"Copy if newer"** in the file properties window.*

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

            //Buffer-Size input for server
            server.BufferSize = 4096;

            //server global encoding for requests and responses
            server.ServerEncoding = Encoding.UTF8;

            //register you server controllers
            server.RegisterAllControllers(Assembly.GetExecutingAssembly(), "FullNamespaceNameForControllers");

            //register you server entities/models
            server.RegisterAllModels(Assembly.GetExecutingAssembly(), "FullNamespaceNameForModels");

            //start server :D
            server.Start();

            //send Reboot Signal call to CoreServer
            server.SendReboot();
        }
```


**How to map you Controllers and Actions**

To map your Controllers and Actions, you must create a folder in your project called "Mappings", and within it, create configuration XML files whose xml name is the Controller name.

The file syntax and structure should look similar to the example below:

```XML
<?xml version="1.0" encoding="utf-8" ?>
<ControllerMapping class="FullNamespace.ControllerTypeName">
	
	<!-- 
		this mapping is used for simple actions, WITHOUT
		entities/models in parameters, ex.:
		
		public ActionResult ActionWithOutModelParameter(int param1, string param2, decimal param3) ...
	-->
	<RequestMapping value="ActionWithOutModelParameter"/>


	<!-- 
		this mapping is used for complex actions, WITH
		entities/models in parameters, ex.:
		
		public ActionResult ActionWithModelParameter(Product product, Customer customer) ...
		
		NOTE: this feature requires model registration in server: 
		    server.RegisterAllModels(Assembly.GetExecutingAssembly(), "FullNamespaceNameForModels");
	-->
	<RequestMapping value="ActionWithModelParameter">
		<RequestParameter alias="product" entity="FullNamespace.Product" />
		<RequestParameter alias="customer" entity="FullNamespace.Customer" />
	</RequestMapping>
	
</ControllerMapping>
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
