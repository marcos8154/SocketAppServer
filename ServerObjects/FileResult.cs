using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MobileAppServer.ServerObjects
{
    public class FileResult : ActionResult
    {
        public FileResult(string filePath, int status = ResponseStatus.SUCCESS,
            string message = "Request success")
        {
            try
            {
                this.Content = System.IO.File.ReadAllBytes(filePath);
            }
            catch(Exception ex)
            {
                this.Status = (int) ResponseStatus.ERROR;
                this.Message = ex.Message;
                return;
            }

            this.Type = 1;
            this.Status = status;
            this.Message = message;
        }
    }
}
