using SocketAppServer.ServerObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace DefaultTestServer
{
    public class MySimpleExceptionHandler : IActionExceptionHandler
    {
        public OperationResult Handle(Exception exception, SocketRequest request)
        {
            return new OperationResult(null, 550, "Não foi possível registrar o dispositivo. Tente novamente mais tarde");
        }
    }
}
