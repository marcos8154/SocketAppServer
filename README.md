# SocketAppServer
A simple, lightweight and fast MVC-like Socket server


How to Setup:
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


How to map you Controllers and Actions

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
