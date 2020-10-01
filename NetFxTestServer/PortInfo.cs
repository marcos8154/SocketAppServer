using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetFxTestServer
{
    public class PortInfo
    {
        public int PortNumber { get; private set; }

        public bool IsAvailable { get; private set; }

        public PortInfo(int portNumber)
        {
            PortNumber = portNumber;
            IsAvailable = true;
        }

        private Process serverProcess;

        public void SetServerProccess(Process process)
        {
            serverProcess = process;
        }

        public void ChangeAvailable()
        {
            IsAvailable = !IsAvailable;

            if (IsAvailable)
            {
                try
                {
                    serverProcess.Kill();
                }
                finally
                {
                    serverProcess.Dispose();
                }
            }
            else
                serverProcess.Start();
        }
    }
}
