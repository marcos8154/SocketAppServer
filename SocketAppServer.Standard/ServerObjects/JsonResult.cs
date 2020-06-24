/*
MIT License

Copyright (c) 2020 Marcos Vinícius

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Newtonsoft.Json;
using SocketAppServer.ServerUtils;
using System;
using System.IO;
using System.Text;

namespace SocketAppServer.ServerObjects
{
    public class JsonResult : ActionResult
    {
        public double bytesUsed = 0;

        public JsonResult(ref object obj, ref  int status, ref string message)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                using (StringWriter writer = new StringWriter(sb))
                {
                    using (JsonWriter jsonWriter = new JsonTextWriter(writer))
                    {
                        var serializer = new JsonSerializer();
                        serializer.ApplyCustomSettings();
                        serializer.Serialize(writer, obj);
                    }
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
