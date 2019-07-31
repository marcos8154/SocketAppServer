# SocketAppServer
A simple, lightweight and fast MVC-like Socket server


How to Setup:

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
