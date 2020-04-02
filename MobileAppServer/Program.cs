using MobileAppServer.LoadBalancingServices;
using MobileAppServer.ServerObjects;
using System.Text;

namespace MobileAppServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //create server
            Server server = new Server();

            //set port
            server.Port = 4000;

            //set Threads Limit; default value is 999999 threads
            server.MaxThreadsCount = 5;

            //define if server is Single-Threaded; if true, MaxThreadsCount is ignored
            server.IsSingleThreaded = false;

            //Buffer-Size input for server
            server.BufferSize = 400000;
            server.RegisterController(typeof(TestController));
            //server global encoding for requests and responses
            server.ServerEncoding = Encoding.UTF8;
            server.RegisterModelType(typeof(Entity));
            //start server :D
            server.Start();

        }
    }
}
