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
using SocketAppServerClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SocketAppServer.LoadBalancingServices
{
    internal class LoadBalanceServer : IBasicServerController
    {
        internal static int AttemptsToGetSubServerAvailable { get; private set; }
        internal static bool EnabledCachedResultsForUnreachableServers { get; private set; }
        internal static INotifiableSubServerRequirement NotifiableSubServerRequirement { get; private set; }
        internal static int ServersLifetimeInMinutes { get; private set; }
        private static ILoggingService Logger { get; set; }

        internal static void EnableSubServerAutoRequirement(INotifiableSubServerRequirement notifiable,
            int serversLifetimeInMinutes = 10)
        {
            NotifiableSubServerRequirement = notifiable;
            ServersLifetimeInMinutes = serversLifetimeInMinutes;
        }

        internal static void SetAttemptsToGetSubServerAvailable(int attempts)
        {
            AttemptsToGetSubServerAvailable = attempts;
        }

        private static void SubServer_OnLifeTimeEnded(SubServer subServer)
        {
            Logger.WriteLog($"Sub-server '{subServer.Address}:{subServer.Port}' is no longer needed and will be detached from the pool of sub-servers. A notification was issued for the target sub-server to be shut down", ServerLogType.ALERT);
            SubServers.Remove(subServer);
        }

        internal static List<SubServer> SubServers { get; private set; }

        public static void EnableCachedResultsForUnreachableServers()
        {
            EnabledCachedResultsForUnreachableServers = true;
        }

        private static void AddSubServerInternal(SubServer server, bool isDynamicInstance = false)
        {
            if (Logger == null)
                Logger = ServiceManager.GetInstance().GetService<ILoggingService>();
            SubServers.Add(server);

            if (SubServers.Count > 1)
            {
                if (isDynamicInstance)
                {
                    if (NotifiableSubServerRequirement != null)
                    {
                        server.EnableLifetime(NotifiableSubServerRequirement, ServersLifetimeInMinutes);
                        server.OnLifeTimeEnded += SubServer_OnLifeTimeEnded;
                    }
                }
            }

            Logger.WriteLog($"Added sub-server node named as '{server.Address}:{server.Port}'", ServerLogType.INFO);

        }

        public static void AddSubServer(string address, int port,
            Encoding encoding, int maxConnectionAttempts,
            int acceptableProcesses)
        {
            if (SubServers == null)
                SubServers = new List<SubServer>();

            var server = new SubServer(address, port, encoding,
                maxConnectionAttempts, acceptableProcesses);

            AddSubServerInternal(server);
        }

        public LoadBalanceServer()
        {
            IServiceManager manager = ServiceManager.GetInstance();
            Logger = manager.GetService<ILoggingService>();
        }

        private ISocketClientConnection BuildClient(SubServer server)
        {
            try
            {
                return SocketConnectionFactory.GetConnection(new SocketClientSettings(
                        server: server.Address,
                        port: server.Port,
                        encoding: server.Encoding,
                        maxAttempts: server.MaxConnectionAttempts
                    ));
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"Fail to connect server {server.Address}:{server.Port} : {ex.Message}", ServerLogType.ERROR);
                return null;
            }
        }

        private int GetCurrentThreadCountOnServer(SubServer server)
        {
            using (ISocketClientConnection client = BuildClient(server))
            {
                Logger.WriteLog($"Querying availability on '{server.Address}:{server.Port}'", ServerLogType.INFO);

                client.SendRequest("ServerInfoController", "GetCurrentThreadsCount");

                int result = int.Parse(client.GetResult().Entity.ToString());
                return result;
            }
        }

        int attemptsToGetServerAvailable = 0;

        private bool IsServerUnavailable(SubServer server)
        {
            string key = $"unavailable-{server.Address}:{server.Port}";
            Cache<bool> cached = CacheRepository<bool>.Get(key);
            if (cached != null)
                Logger.WriteLog($"The sub-server node '{server.Address}:{server.Port}' is unreachable. It is temporarily ignored but will be reconsidered in less than 120 seconds", ServerLogType.ALERT);
            return cached != null;
        }

        private bool AreServerConnected(SubServer server, string cacheKey)
        {
            try
            {
                using (ISocketClientConnection client = BuildClient(server))
                    return true;
            }
            catch
            {

                Logger.WriteLog($"Sub-server node '{server.Address}:{server.Port}' is unreachable", ServerLogType.ALERT);
                CacheRepository<bool>.Set(cacheKey, true, 120);
                return false;
            }
        }

        private bool CanOperate(string key, SubServer server)
        {
            int serverCurrentThreadsCount;
            try
            {
                serverCurrentThreadsCount = GetCurrentThreadCountOnServer(server);
            }
            catch
            {
                CacheRepository<bool>.Set(key, true, 120);
                return false;
            }

            if (serverCurrentThreadsCount > server.AcceptableProcesses)
            {
                Logger.WriteLog($"Sub-server node '{server.Address}:{server.Port}' is too busy", ServerLogType.ALERT);
                return false;
            }

            return true;
        }

        private static object lckGtAvlb = new object();


        private static int selectedSubServerIndex = 0;
        private SubServer GetAvailableSubServer()
        {
            lock(lckGtAvlb)
            {
                for(int i = 0; i < SubServers.Count; i++)
                {
                    if (selectedSubServerIndex >= SubServers.Count)
                        selectedSubServerIndex = 0;

                    SubServer server = SubServers[selectedSubServerIndex];

                    if (IsServerUnavailable(server))
                        continue;

                    string key = $"unavailable-{server.Address}:{server.Port}";

                    if (!CanOperate(key, server))
                        continue;

                    selectedSubServerIndex += 1;
                    return server;
                }

                return null;
            }
        }

        private string BuildCacheResultKey(SocketAppServerClient.RequestBody rb)
        {
            string cacheResultKey = $"{rb.Controller}-{rb.Action}";
            if (rb.Parameters != null)
                rb.Parameters.ForEach(p => cacheResultKey += $"-{p.Name}:{p.Value}");
            return cacheResultKey;
        }

        private ActionResult ResolveResultOnUreachableServer(string cacheResultKey)
        {
            try
            {
                if (EnabledCachedResultsForUnreachableServers)
                {
                    Cache<SocketAppServerClient.OperationResult> cached = CacheRepository<SocketAppServerClient.OperationResult>.Get(cacheResultKey);
                    if (cached != null)
                        return ActionResult.Json(cached.Value);
                    else
                        throw new Exception();
                }
                else throw new Exception();
            }
            catch
            {
                return ActionResult.Json(null, 500, "None of the sub-servers are available to fulfill the request");
            }
        }

        private static object lckAllocate = new object();
        private void CheckAllocateNewInstance()
        {
            if (NotifiableSubServerRequirement != null)
            {
                lock (lckAllocate)
                {
                    try
                    {
                        SubServer subServer = NotifiableSubServerRequirement.StartNewInstance();
                        if (subServer == null)
                            return;

                        AddSubServerInternal(subServer, true);
                        Logger.WriteLog("A new server instance was requested to meet the next requests", ServerLogType.ALERT);
                        Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteLog($"Failed to request a new sub-server instance for the implementation of 'NotifiableSubServerRequirement': {ex.Message}",
                            ServerLogType.ERROR);
                    }
                }
            }
        }

        private bool retried = false;

        public ActionResult RunAction(string receivedData)
        {
            SocketAppServerClient.RequestBody rb = JsonConvert.DeserializeObject<SocketAppServerClient.RequestBody>(receivedData,
                AppServerConfigurator.SerializerSettings);

            if (rb.Controller.Equals("ServerInfoController") &&
                rb.Action.Equals("FullServerInfo"))
                return new ServerInfoController().FullServerInfo();

            string cacheResultKey = BuildCacheResultKey(rb);
            SubServer targetServer = GetAvailableSubServer();

            if (targetServer == null)
            {
                CheckAllocateNewInstance();

                if (retried)
                    return ResolveResultOnUreachableServer(cacheResultKey);
                else
                {
                    retried = true;
                    return RunAction(receivedData);
                }
            }

            using (ISocketClientConnection client = BuildClient(targetServer))
            {
                client.SendRequest(rb);

                SocketAppServerClient.OperationResult result = client.GetResult();

                if (EnabledCachedResultsForUnreachableServers)
                    CacheRepository<SocketAppServerClient.OperationResult>.Set(cacheResultKey, result, 380);

                targetServer.RefreshLifetimeIfHas();
                return ActionResult.Json(result);
            }
        }
    }
}
