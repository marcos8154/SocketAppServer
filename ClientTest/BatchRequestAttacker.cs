using SocketAppServerClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientTest
{
    public class BatchRequestAttacker
    {
        private readonly BatchRequestManager manager;
        public int AttackerId;

        public BatchRequestAttacker(BatchRequestManager manager,
            int attackerId, int totalFired)
        {
            this.manager = manager;
            this.AttackerId = attackerId;
            this.TotalFired = totalFired;
        }

        public int RequestsSuccess { get; private set; }
        public int RequestFails { get; private set; }
        public int TotalFired { get; private set; }

        public void Attack()
        {
            Task.Run(() =>
            {
                Thread.Sleep(300);
                using (ISocketClientConnection connection = SocketConnectionFactory.GetConnection())
                {
                    for (int i = 0; i < TotalFired; i++)
                    {
                        try
                        {
                            connection.SendRequest("CustomerController", "AddCustomer", new
                            {
                                customer = $"Customer n {i + 1} of attacker #{AttackerId}"
                            });

                            if (connection.GetResult().Status == 600)
                                RequestsSuccess += 1;
                            else
                                RequestFails += 1;
                        }
                        catch
                        {
                            RequestFails += 1;
                        }
                    }

                    manager.EndBatch(TotalFired, RequestsSuccess, RequestFails);
                }
            });
        }
    }
}
