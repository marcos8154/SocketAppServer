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
