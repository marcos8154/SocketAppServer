using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace MobileAppServer.ServerObjects
{
    public class LogController
    {
        private static object Locked = new object();
        public static void WriteLog(string msg)
        {
            new Thread(() =>
            {
                lock (Locked)
                {
                    StreamWriter writer = null;
                    try
                    {
                        if (!Directory.Exists(@"C:\Temp\"))
                            Directory.CreateDirectory(@"C:\Temp\");

                        string fileName = @"C:\Temp\SocketAppServer-" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";

                        writer = (File.Exists(fileName)
                            ? File.AppendText(fileName)
                            : new StreamWriter(fileName));

                        writer.WriteLine($" [{DateTime.Now.ToString()}]: {msg}");
                        writer.WriteLine(Environment.NewLine);
                        writer.Close();

                        Console.Write($"\n [{DateTime.Now.ToString()}]: {msg}");
                        writer.Dispose();
                        writer = null;
                    }
                    catch (Exception ex)
                    {
                        if (writer != null)
                        {
                            writer.Close();
                            writer.Dispose();
                            writer = null;
                        }

                        WriteLog(msg);
                    }
                }
            }).Start();
        }
    }
}
