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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocketAppServer.ServerObjects
{
    public abstract class ActionResult
    {
        public virtual double PercentUsage { get; set; }
        public virtual double ResponseLenght { get; set; }
        public virtual string FileState { get; set; }
        public virtual int Type { get; set; }
        public virtual string Message { get; set; }
        public virtual int Status { get; set; }
        public virtual Object Content { get; set; }

        public static ActionResult Json(object obj, int status = ResponseStatus.SUCCESS, string message = "Request success")
        {
            return new JsonResult(ref obj, ref status, ref message);
        }

        public static ActionResult File(string filePath, int status = ResponseStatus.SUCCESS, string message = "Request success")
        {
            return new FileResult(filePath, status, message);
        }
    }
}
