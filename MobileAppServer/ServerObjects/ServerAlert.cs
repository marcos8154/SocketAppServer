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

using Newtonsoft.Json;
using SocketAppServer.CoreServices;
using System;
using System.Collections.Generic;
using System.IO;

namespace SocketAppServer.ServerObjects
{
    public class ServerAlert
    {
        public DateTime Date { get; set; }

        public string Controller { get; set; }

        public string Action { get; set; }

        public string Message { get; set; }

        public ServerAlert(string controller, string action,
            string message)
        {
            Date = DateTime.Now;
            Controller = controller;
            Action = action;
            Message = message;
        }

        public ServerAlert()
        {
            Date = DateTime.Now;
        }
    }

    public class ServerAlertManager
    {
        private static object readLock = new object();
        internal static List<ServerAlert> Load()
        {
            lock(readLock)
            {
                string file = @".\ServerAlerts.json";
                try
                {
                    if (!File.Exists(file))
                        return new List<ServerAlert>();

                    string txt = File.ReadAllText(file);
                    List<ServerAlert> json = JsonConvert.DeserializeObject<List<ServerAlert>>(txt, AppServerConfigurator.SerializerSettings);
                    return json;
                }
                catch
                {
                    //file was corrupted
                    File.Delete(file);
                    return new List<ServerAlert>();
                }
            }
        }
        
        private static object writeLock = new object();
        public static void CreateAlert(ServerAlert alert)
        {
            lock (writeLock)
            {
                string file = @".\ServerAlerts.json";
                var alerts = Load();

                if (alerts.Count > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\nYour server has {alerts.Count} alerts!");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }

                alerts.Add(alert);
                File.WriteAllText(file, JsonConvert.SerializeObject(alerts, AppServerConfigurator.SerializerSettings));
            }
        }
    }
}
