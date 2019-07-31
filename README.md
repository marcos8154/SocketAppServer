# SocketAppServer
A simple, lightweight and fast MVC-like Socket server


**How to Setup**
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

        public ActionResult ComplexAction(Product product, string otherParam)
        {
            if (product.Price > 200)
                return ActionResult.Json(product, ResponseStatus.ERROR, "Invalid product");
            else
                return ActionResult.Json(true);
        }
    }
```
