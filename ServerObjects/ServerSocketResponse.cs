using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileAppServer.ServerObjects
{
    internal class ServerSocketResponse
    {
        public int status { get; set; }
        public string message { get; set; }
        public object entity { get; set; }

        public ServerSocketResponse(int status,
            string message, object entity)
        {
            this.status = status;
            this.message = message;
            this.entity = entity;
        }
    }
}
