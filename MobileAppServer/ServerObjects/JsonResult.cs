using Newtonsoft.Json;
using System;

namespace MobileAppServer.ServerObjects
{
    public class JsonResult : ActionResult
    {
        public double bytesUsed = 0;

        public JsonResult(object obj, int status, string message)
        {
            try
            {
                string json = JsonConvert.SerializeObject(obj);
          
                this.Content = json;
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
