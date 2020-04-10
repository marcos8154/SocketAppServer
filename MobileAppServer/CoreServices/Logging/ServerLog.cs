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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileAppServer.CoreServices.Logging
{
    public sealed class ServerLog
    {
        [JsonProperty]
        public DateTime EventDate { get; internal set; }

        [JsonProperty]
        public string LogText { get; internal set; }

        [JsonProperty]
        public string ControllerName { get; internal set; }

        [JsonProperty]
        public string ActionName { get; internal set; }

        [JsonProperty]
        public ServerLogType Type { get; internal set; }

        public ServerLog(string logText, ServerLogType type = ServerLogType.INFO)
        {
            EventDate = DateTime.Now;
            LogText = logText;
            Type = type;
        }

        public ServerLog(string logText, string controller, string action, ServerLogType type = ServerLogType.INFO)
        {
            LogText = logText;
            ControllerName = controller;
            ActionName = action;
            Type = type;
        }

        public ServerLog()
        {

        }
    }
}
