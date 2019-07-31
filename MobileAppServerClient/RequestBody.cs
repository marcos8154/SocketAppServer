using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobileAppServerClient
{
    public class RequestBody
    {
        public string Action { get; set; }
        public string Controller { get; set; }
        public List<RequestParameter> Parameters { get; set; }

        private RequestBody()
        {

        }

        public static RequestBody Create(string controller, string action)
        {
            RequestBody rb = new RequestBody();
            rb.Controller = controller;
            rb.Action = action;

            return rb;
        }

        public RequestBody AddParameter(string name, object value)
        {
            if (Parameters == null)
                Parameters = new List<RequestParameter>();

            Parameters.Add(new RequestParameter(name, value));
            return this;
        }
    }
}
