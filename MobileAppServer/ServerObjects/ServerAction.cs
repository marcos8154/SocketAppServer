using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketAppServer.ServerObjects
{
    /// <summary>
    /// Allows you to define void or object as an action return, instead of ActionResult
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ServerAction : Attribute
    {

        public ServerAction()
        {
            if (AppServerConfigurator.DefaultExceptionHandlerType != null)
                ExceptionHandler = AppServerConfigurator.DefaultExceptionHandlerType;
        }


        /// <summary>
        /// Customized error code for response to the client, 
        /// instead of using the default code of the server (500)
        /// </summary>
        public int DefaultErrorCode { get; set; }

        /// <summary>
        /// Defines the type of an IActionExceptionHandler for handling 
        /// exceptions thrown by this action
        /// </summary>
        public Type ExceptionHandler { get; set; }
    }
}
