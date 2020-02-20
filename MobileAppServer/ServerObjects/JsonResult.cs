using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace MobileAppServer.ServerObjects
{
    public class JsonResult : ActionResult
    {
        public double bytesUsed = 0;

        public JsonResult(object obj, int status, string message)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                StringWriter writer = new StringWriter(sb);
                using (JsonWriter jsonWriter = new JsonTextWriter(writer))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(writer, obj);
                }

                string json = sb.ToString();//JsonConvert.SerializeObject(obj);
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
