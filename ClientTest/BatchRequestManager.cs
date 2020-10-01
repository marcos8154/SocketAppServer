using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientTest
{
    public class BatchRequestManager
    {
        public List<BatchRequestAttacker> Attackers { get; private set; }

        private AutoResetEvent resetEvent;

        public BatchRequestManager()
        {
            Attackers = new List<BatchRequestAttacker>();
    
        }

        int finishedBatches = 0;
        public int Fired { get; private set; }
        public int Success { get; private set; }
        public int Fails { get; private set; }

        public void EndBatch(int fired, int success, int fails)
        {
            Fired += fired;
            Success += success;
            Fails += fails;

            finishedBatches += 1;

            if (finishedBatches >= Attackers.Count)
                resetEvent.Set();
        }

        public void AddAttacker(BatchRequestAttacker attacker)
        {
            Attackers.Add(attacker);
        }

        public void RunAll()
        {
            resetEvent = new AutoResetEvent(false);

            Attackers.ForEach(a => a.Attack());

            resetEvent.WaitOne();
        }
    }
}
