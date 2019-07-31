using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileAppServer.ServerObjects
{
    public abstract class ActionResult
    {
        public virtual string FileState { get; set; }
        public virtual int Type { get; set; }
        public virtual string Message { get; set; }
        public virtual int Status { get; set; }
        public virtual Object Content { get; set; }

        public static ActionResult Json(object obj, int status = ResponseStatus.SUCCESS, string message = "Request success")
        {
            return new  JsonResult(obj, status, message);
        }

        public static ActionResult File(string filePath, int status = ResponseStatus.SUCCESS, string message = "Request success")
        {
            return new FileResult(filePath, status, message);
        }
    }
}
