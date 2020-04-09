using MobileAppServer.CoreServices;
using MobileAppServer.ManagedServices;
using MobileAppServer.ServerObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAppServer
{
    internal class TestController : IController
    {
        public ActionResult SimpleAction(string param1, int param2)
        {
            int count = 0;
            byte[] array = null;
            while (count < 10)
            {
                 array = File.ReadAllBytes(@"C:\oraclexe\app\oracle\oradata\XE\UNDOTBS1.DBF");

                count += 1;
            }
            return ActionResult.Json(new OperationResult(true, 600, $"p1:{param1}, p2:{param2}"));
        }

        public ActionResult SaveEntity(Entity entity)
        {
            return ActionResult.Json(new OperationResult(entity, 600, "Entity Saved."));
        }
    }

    internal class Entity
    {
        public string Name { get; set; }

        public string Phone { get; set; }
    }
}
