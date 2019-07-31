using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileAppServer.ServerObjects
{
    public class JsonResult : ActionResult
    {
        public JsonResult(object obj, int status, string message)
        {
            try
            {
                this.Content = JsonConvert.SerializeObject(obj);
            }
            catch (Exception ex)
            {
                Status = ResponseStatus.ERROR;
                Message = ex.Message;

                return;
            }

            Status = status;
            Message = message;
        }
    }
}
