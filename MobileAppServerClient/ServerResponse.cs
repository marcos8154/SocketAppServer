using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileAppServerClient
{
    public class ServerResponse
    {
        public string FileState { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
        public object Content { get; set; }

        public ServerResponse(int status,
            string message, object entity)
        {
            this.Status = status;
            this.Message = message;
            this.Content = entity;
        }
    }
}
