﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocketAppServerClient
{
    public class ServerResponse
    {
        public double ResponseLenght { get; set; }
        public double PercentUsage { get; set; }
        public string FileState { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
        public object Content { get; set; }
        public string ServerMessage { get; set; }

        public ServerResponse(int status,
            string message, object entity, double bytesUsed)
        {
            this.Status = status;
            this.Message = message;
            this.Content = entity;
            ResponseLenght = bytesUsed;
        }

        public ServerResponse()
        {

        }
    }
}
