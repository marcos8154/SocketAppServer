/*
MIT License

Copyright (c) 2020 Marcos Vinícius

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using SocketAppServer.CoreServices;
using SocketAppServer.CoreServices.Logging;
using SocketAppServer.ManagedServices;
using SocketAppServer.ServerObjects;
using System.Text;

namespace SocketAppServer.LoadBalancingServices
{
    public class SubServer
    {
        public string Address { get; private set; }

        public int Port { get; private set; }

        public Encoding Encoding { get; private set; }

        public int BufferSize { get; private set; }

        public int MaxConnectionAttempts { get; private set; }

        public int AcceptableProcesses { get; private set; }
        public int ServerLifetimeInMinutes { get; private set; }
        public INotifiableSubServerRequirement NotifiableRequirement { get; private set; }

        private ILoggingService Logger { get; set; }

        public SubServer(string address, int port, Encoding encoding,
            int bufferSize, int maxConnectionAttempts,
            int acceptableProcesses)
        {
            Address = address;
            Port = port;
            Encoding = encoding;
            BufferSize = bufferSize;
            MaxConnectionAttempts = maxConnectionAttempts;
            AcceptableProcesses = acceptableProcesses;

            Logger = ServiceManager.GetInstance().GetService<ILoggingService>();
        }

        public SubServer(string address, int port, Encoding encoding,
        int bufferSize, int maxConnectionAttempts,
        int acceptableProcesses, 
        INotifiableSubServerRequirement notifiableRequirement,
        int serverLifetimeInMinutes = 10)
        {
            Address = address;
            Port = port;
            Encoding = encoding;
            BufferSize = bufferSize;
            MaxConnectionAttempts = maxConnectionAttempts;
            AcceptableProcesses = acceptableProcesses;
            ServerLifetimeInMinutes = serverLifetimeInMinutes;
            NotifiableRequirement = notifiableRequirement;

            Logger = ServiceManager.GetInstance().GetService<ILoggingService>();
        }

        internal void EnableLifetime(INotifiableSubServerRequirement notifiableRequirement,
        int serverLifetimeInMinutes = 10)
        {
            NotifiableRequirement = notifiableRequirement;
            ServerLifetimeInMinutes = serverLifetimeInMinutes;
            RefreshLifetimeIfHas();
        }

        private bool HasLifetime()
        {
            return NotifiableRequirement != null;
        }

        private System.Timers.Timer lifetime;
        internal void RefreshLifetimeIfHas()
        {
            if (!HasLifetime())
                return;
           
            if(lifetime != null)
            {
                lifetime.Stop();
                lifetime.Dispose();
                lifetime = null;
                Logger.WriteLog($"Lifetime refreshed to sub-server '{Address}:{Port}'", ServerLogType.INFO);
            }
            else Logger.WriteLog($"Lifetime started to sub-server '{Address}:{Port}'", ServerLogType.INFO);

            elapsedMinutesLifetime = 0;
            lifetime = new System.Timers.Timer();
            lifetime.Interval = (60 * 1000);
            lifetime.Elapsed += Lifetime_Elapsed;
            lifetime.Start();
        }

        private int elapsedMinutesLifetime = 0;
        private void Lifetime_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            elapsedMinutesLifetime += 1;
            
            if(elapsedMinutesLifetime >= ServerLifetimeInMinutes)
            {
                Logger.WriteLog($"Lifetime ended to sub-server '{Address}:{Port}'", ServerLogType.INFO);
                NotifiableRequirement.StopInstance(this);
                OnLifeTimeEnded?.Invoke(this);
                lifetime.Stop();
                lifetime.Dispose();
                lifetime = null;
            }
        }

        public delegate void LifeTimeEnd(SubServer subServer);
        public event LifeTimeEnd OnLifeTimeEnded;
    }
}
